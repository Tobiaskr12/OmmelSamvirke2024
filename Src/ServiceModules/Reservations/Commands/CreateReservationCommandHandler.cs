using Contracts.DataAccess.Base;
using Contracts.ServiceModules.Emails.DTOs;
using Contracts.ServiceModules.Emails.EmailTemplateEngine;
using Contracts.ServiceModules.Emails.Sending;
using Contracts.ServiceModules.Reservations;
using DomainModules.Emails.Constants;
using DomainModules.Emails.Entities;
using DomainModules.Reservations.Entities;
using DomainModules.Reservations.Enums;
using FluentResults;
using FluentValidation;
using JetBrains.Annotations;
using MediatR;
using ServiceModules.Errors;

namespace ServiceModules.Reservations.Commands;

[UsedImplicitly]
public class CreateReservationCommandValidator : AbstractValidator<CreateReservationCommand>
{
    public CreateReservationCommandValidator(IValidator<Reservation> reservationValidator)
    {
        RuleFor(x => x.Reservation).SetValidator(reservationValidator);
    }
}

public class CreateReservationCommandHandler : IRequestHandler<CreateReservationCommand, Result<List<Reservation>>>
{
    private readonly IRepository<Reservation> _reservationRepository;
    private readonly IRepository<ReservationHistory> _reservationHistoryRepository;
    private readonly IRepository<ReservationSeries> _reservationSeriesRepository;
    private readonly IEmailTemplateEngine _emailTemplateEngine;
    private readonly IMediator _mediator;

    public CreateReservationCommandHandler(
        IRepository<Reservation> reservationRepository,
        IRepository<ReservationHistory> reservationHistoryRepository,
        IRepository<ReservationSeries> reservationSeriesRepository,
        IEmailTemplateEngine emailTemplateEngine,
        IMediator mediator)
    {
        _reservationRepository = reservationRepository;
        _reservationHistoryRepository = reservationHistoryRepository;
        _reservationSeriesRepository = reservationSeriesRepository;
        _emailTemplateEngine = emailTemplateEngine;
        _mediator = mediator;
    }

    public async Task<Result<List<Reservation>>> Handle(CreateReservationCommand request, CancellationToken cancellationToken)
    {
        var createdReservations = new List<Reservation>();

        // If recurrence options are provided, generate multiple reservations
        if (request.RecurrenceOptions is not null && request.RecurrenceOptions.RecurrenceType != RecurrenceType.None)
        {
            Result<List<Reservation>> creationResult = await CreateRecurringReservations(request, cancellationToken);
            if (creationResult.IsSuccess)
            {
                createdReservations = creationResult.Value;
            }
            else
            {
                return creationResult;
            }
        }
        else
        {
            // Single reservation creation
            if (await HasConflict(request.Reservation, cancellationToken))
            {
                return Result.Fail<List<Reservation>>(ErrorMessages.Reservations_ReservationConflict);
            }

            createdReservations.Add(request.Reservation);
        }

        // Persist the reservations
        Result<List<Reservation>> addResult = await _reservationRepository.AddAsync(createdReservations, cancellationToken);
        if (addResult.IsFailed)
        {
            return Result.Fail<List<Reservation>>(ErrorMessages.GenericErrorWithRetryPrompt);
        }

        // Handle ReservationHistory: link reservations to an existing history or create a new one
        Result<List<ReservationHistory>> historyResult = await _reservationHistoryRepository.FindAsync(
            rh => rh.Email == request.Reservation.Email,
            cancellationToken: cancellationToken);
        ReservationHistory history;
        
        if (historyResult.Value.Count != 0)
        {
            history = historyResult.Value.First();
            history.Reservations.AddRange(addResult.Value);
            await _reservationHistoryRepository.UpdateAsync(history, cancellationToken);
        }
        else
        {
            history = new ReservationHistory
            {
                Email = request.Reservation.Email,
                Reservations = addResult.Value
            };
            Result<ReservationHistory> addHistoryResult = await _reservationHistoryRepository.AddAsync(history, cancellationToken);
            if (addHistoryResult.IsFailed)
            {
                return Result.Fail<List<Reservation>>(ErrorMessages.GenericErrorWithRetryPrompt);
            }
        }

        // Generate request confirmation email
        string reservationStatusLink = $"https://www.ommelsamvirke.com/reservationer/overblik?token={history.Token}";
        Result templateResult = _emailTemplateEngine.GenerateBodiesFromTemplate(
            Templates.Reservations.ReservationRequestConfirmation,
            ("ReservationStatusLink", reservationStatusLink),
            ("LocationName", request.Reservation.Location.Name)
        );
        
        if (templateResult.IsFailed) return Result.Fail<List<Reservation>>(ErrorMessages.GenericErrorWithRetryPrompt);

        var email = new Email
        {
            Subject = _emailTemplateEngine.GetSubject(),
            SenderEmailAddress = ValidSenderEmailAddresses.Auto,
            Recipients = [new Recipient { EmailAddress = request.Reservation.Email }],
            HtmlBody = _emailTemplateEngine.GetHtmlBody(),
            PlainTextBody = _emailTemplateEngine.GetPlainTextBody(),
            Attachments = []
        };

        // Send request confirmation email
        Result<EmailSendingStatus> sendResult = await _mediator.Send(new SendEmailCommand(email), cancellationToken);
        
        return sendResult.IsFailed 
            ? Result.Fail<List<Reservation>>(ErrorMessages.GenericErrorWithRetryPrompt) 
            : Result.Ok(addResult.Value);
    }

    private async Task<Result<List<Reservation>>> CreateRecurringReservations(CreateReservationCommand request, CancellationToken cancellationToken)
    {
        var createdReservations = new List<Reservation>();
        
        // Validate recurrence period
        if (request.RecurrenceOptions!.RecurrenceEndDate <= request.RecurrenceOptions.RecurrenceStartDate)
        {
            return Result.Fail<List<Reservation>>(ErrorMessages.Reservations_InvalidRecurrencePeriod);
        }

        // Create a ReservationSeries entity
        var series = new ReservationSeries
        {
            RecurrenceType = request.RecurrenceOptions.RecurrenceType,
            Interval = request.RecurrenceOptions.Interval,
            RecurrenceStartDate = request.RecurrenceOptions.RecurrenceStartDate,
            RecurrenceEndDate = request.RecurrenceOptions.RecurrenceEndDate,
        };

        // Generate the recurrence dates
        IEnumerable<DateTime> recurrenceDates = RecurringDatesHelper.GenerateRecurrenceDates(request.RecurrenceOptions);

        // For each recurrence date, create a new reservation instance
        foreach (DateTime date in recurrenceDates)
        {
            TimeSpan timeOffset = request.Reservation.EndTime - request.Reservation.StartTime;
            var newStart = new DateTime(date.Year, date.Month, date.Day,
                request.Reservation.StartTime.Hour,
                request.Reservation.StartTime.Minute,
                request.Reservation.StartTime.Second, 
                DateTimeKind.Utc);
            DateTime newEnd = newStart.Add(timeOffset);

            Reservation newReservation = CreateReservationInstance(request, newStart, newEnd);

            // Check for conflicts
            if (await HasConflict(newReservation, cancellationToken))
            {
                return Result.Fail<List<Reservation>>(ErrorMessages.Reservations_ReservationConflict);
            }

            createdReservations.Add(newReservation);
        }

        // Save the ReservationSeries
        Result<ReservationSeries> addSeriesResult = await _reservationSeriesRepository.AddAsync(series, cancellationToken);
        if (addSeriesResult.IsFailed)
        {
            return Result.Fail<List<Reservation>>(ErrorMessages.GenericErrorWithRetryPrompt);
        }
        // Assign the series ID to each reservation
        foreach (Reservation res in createdReservations)
        {
            res.ReservationSeriesId = addSeriesResult.Value.Id;
        }

        return createdReservations;
    }
    
    private static Reservation CreateReservationInstance(CreateReservationCommand request, DateTime newStartTime, DateTime newEndTime)
    {
        return new Reservation
        {
            Email = request.Reservation.Email,
            PhoneNumber = request.Reservation.PhoneNumber,
            Name = request.Reservation.Name,
            CommunityName = request.Reservation.CommunityName,
            StartTime = newStartTime,
            EndTime = newEndTime,
            State = ReservationState.Pending,
            Location = request.Reservation.Location
        };
    }

    private async Task<bool> HasConflict(Reservation newReservation, CancellationToken cancellationToken)
    {
        Result<List<Reservation>> conflictResult = await _reservationRepository.FindAsync(
            r => r.Email == newReservation.Email &&
                 r.State != ReservationState.Denied &&
                 r.StartTime < newReservation.EndTime &&
                 r.EndTime > newReservation.StartTime,
            readOnly: true,
            cancellationToken: cancellationToken);
            
        return conflictResult.Value.Count != 0;
    }
}

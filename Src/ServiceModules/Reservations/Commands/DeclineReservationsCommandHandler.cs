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
using MediatR;
using ServiceModules.Errors;

namespace ServiceModules.Reservations.Commands;

public class DeclineReservationsCommandHandler : IRequestHandler<DeclineReservationsCommand, Result<List<Reservation>>>
{
    private readonly IRepository<Reservation> _reservationRepository;
    private readonly IRepository<ReservationHistory> _reservationHistoryRepository;
    private readonly IEmailTemplateEngine _emailTemplateEngine;
    private readonly IMediator _mediator;

    public DeclineReservationsCommandHandler(
        IRepository<Reservation> reservationRepository,
        IRepository<ReservationHistory> reservationHistoryRepository,
        IEmailTemplateEngine emailTemplateEngine,
        IMediator mediator)
    {
        _reservationRepository = reservationRepository;
        _reservationHistoryRepository = reservationHistoryRepository;
        _emailTemplateEngine = emailTemplateEngine;
        _mediator = mediator;
    }

    public async Task<Result<List<Reservation>>> Handle(DeclineReservationsCommand request, CancellationToken cancellationToken)
    {
        var updatedReservations = new List<Reservation>();

        // Validate token by retrieving the reservation history
        Result<List<ReservationHistory>> historyResult = await _reservationHistoryRepository.FindAsync(rh => 
            rh.Token == request.Token, 
            cancellationToken: cancellationToken
        );
        if (historyResult.Value.Count == 0)
        {
            return Result.Fail<List<Reservation>>(ErrorMessages.Reservations_TokenMismatch);
        }
        ReservationHistory history = historyResult.Value.First();

        // Retrieve reservations by the provided IDs
        Result<List<Reservation>> reservationsResult = await _reservationRepository.FindAsync(r => 
            request.ReservationIds.Contains(r.Id), 
            readOnly: false,
            cancellationToken: cancellationToken
        );
        if (reservationsResult.IsFailed || reservationsResult.Value.Count == 0)
        {
            return Result.Fail<List<Reservation>>(ErrorMessages.Reservations_ReservationNotFound);
        }

        // Validate ownership and decline each reservation
        foreach (Reservation reservation in reservationsResult.Value)
        {
            if (!string.Equals(reservation.Email, history.Email, StringComparison.OrdinalIgnoreCase))
            {
                return Result.Fail<List<Reservation>>(ErrorMessages.Reservations_TokenMismatch);
            }
            reservation.State = ReservationState.Denied;
            updatedReservations.Add(reservation);
        }

        Result<List<Reservation>> updateResult = await _reservationRepository.UpdateAsync(updatedReservations, cancellationToken);
        if (updateResult.IsFailed)
        {
            return Result.Fail<List<Reservation>>(ErrorMessages.GenericErrorWithRetryPrompt);
        }
        
        // Create reservation declined email
        string reservationDetailsLink = $"https://www.ommelsamvirke.com/reservationer/overblik?token={history.Token}";
        Result templateResult = _emailTemplateEngine.GenerateBodiesFromTemplate(
            Templates.Reservations.ReservationDeclinedNotification,
            ("ReservationDetailsLink", reservationDetailsLink),
            ("LocationName", reservationsResult.Value.First().Location.Name)
        );
        
        if (templateResult.IsFailed) return Result.Fail<List<Reservation>>(ErrorMessages.GenericErrorWithRetryPrompt);

        var email = new Email
        {
            Subject = _emailTemplateEngine.GetSubject(),
            SenderEmailAddress = ValidSenderEmailAddresses.Auto,
            Recipients = [new Recipient { EmailAddress = history.Email }],
            HtmlBody = _emailTemplateEngine.GetHtmlBody(),
            PlainTextBody = _emailTemplateEngine.GetPlainTextBody(),
            Attachments = []
        };

        // Send reservation declined email
        Result<EmailSendingStatus> sendResult = await _mediator.Send(new SendEmailCommand(email), cancellationToken);
        
        return sendResult.IsFailed 
            ? Result.Fail<List<Reservation>>(ErrorMessages.GenericErrorWithRetryPrompt) 
            : Result.Ok(updateResult.Value);
    }
}

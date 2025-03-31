using Contracts.ServiceModules.Events.Events;
using Contracts.ServiceModules.Reservations;
using DomainModules.Events.Entities;
using DomainModules.Reservations.Entities;
using DomainModules.Common;
using MediatR;
using Contracts.DataAccess.Base;
using FluentResults;
using FluentValidation;
using JetBrains.Annotations;
using ServiceModules.Errors;
using ServiceModules.Reservations;

namespace ServiceModules.Events.Events.Commands;

[UsedImplicitly]
public class CreateEventCommandValidator : AbstractValidator<CreateEventCommand>
{
    public CreateEventCommandValidator(IValidator<Event> eventValidator)
    {
        RuleFor(x => x.Event).SetValidator(eventValidator);
    }
}

public class CreateEventCommandHandler : IRequestHandler<CreateEventCommand, Result<List<Event>>>
{
    private readonly IRepository<Event> _eventRepository;
    private readonly IMediator _mediator;

    public CreateEventCommandHandler(
        IRepository<Event> eventRepository,
        IMediator mediator)
    {
        _eventRepository = eventRepository;
        _mediator = mediator;
    }

    public async Task<Result<List<Event>>> Handle(CreateEventCommand request, CancellationToken cancellationToken)
    {
        List<Event> eventsToCreate = [];

        // Generate recurrence dates using the helper
        IEnumerable<DateTime> recurrenceDates;
        if (request.RecurrenceOptions is not null && request.RecurrenceOptions.RecurrenceType != RecurrenceType.None)
        {
            recurrenceDates = RecurringDatesHelper.GenerateRecurrenceDates(request.RecurrenceOptions);
        }
        else
        {
            recurrenceDates = new List<DateTime> { request.Event.StartTime.Date };
        }

        // Create an event occurrence for each recurrence date.
        foreach (DateTime date in recurrenceDates)
        {
            var occurrence = new Event
            {
                Title = request.Event.Title,
                Description = request.Event.Description,
                StartTime = new DateTime(
                    date.Year, date.Month, date.Day,
                    request.Event.StartTime.Hour, request.Event.StartTime.Minute, request.Event.StartTime.Second),
                EndTime = new DateTime(
                    date.Year, date.Month, date.Day,
                    request.Event.EndTime.Hour, request.Event.EndTime.Minute, request.Event.EndTime.Second),
                EventCoordinator = request.Event.EventCoordinator,
                Location = request.Event.Location,
                RemoteFiles = request.Event.RemoteFiles
            };
            eventsToCreate.Add(occurrence);
        }

        // If a Reservation is provided, delegate its creation to the existing CreateReservationCommand
        if (request.Reservation is not null)
        {
            var createReservationCommand = new CreateReservationCommand(request.Reservation, request.RecurrenceOptions);
            Result<List<Reservation>> reservationResult = await _mediator.Send(createReservationCommand, cancellationToken);
            if (reservationResult.IsFailed) return Result.Fail<List<Event>>(ErrorMessages.GenericErrorWithRetryPrompt);
                
            List<Reservation>? reservations = reservationResult.Value;
            
            // Link each event occurrence with the corresponding reservation based on the date.
            foreach (Event e in eventsToCreate)
            {
                Reservation? matchingReservation = reservations.FirstOrDefault(r => r.StartTime.Date == e.StartTime.Date);
                if (matchingReservation != null)
                {
                    e.Reservation = matchingReservation;
                }
            }
        }

        // Save each event
        List<Event> createdEvents = [];
        
        foreach (Event ev in eventsToCreate)
        {
            Result<Event> addResult = await _eventRepository.AddAsync(ev, cancellationToken);
            if (addResult.IsFailed) return Result.Fail<List<Event>>(ErrorMessages.GenericErrorWithRetryPrompt);
            
            createdEvents.Add(addResult.Value);
        }

        return Result.Ok(createdEvents);
    }
}

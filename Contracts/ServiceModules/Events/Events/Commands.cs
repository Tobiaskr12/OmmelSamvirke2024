using DomainModules.Events.Entities;
using DomainModules.Reservations.Entities;
using DomainModules.Common;
using FluentResults;
using MediatR;

namespace Contracts.ServiceModules.Events.Events;

public record CreateEventCommand(Event Event, RecurrenceOptions? RecurrenceOptions = null, Reservation? Reservation = null) : IRequest<Result<List<Event>>>;
public record UpdateEventCommand(Event Event) : IRequest<Result<Event>>;
public record DeleteEventCommand(int Id) : IRequest<Result>;

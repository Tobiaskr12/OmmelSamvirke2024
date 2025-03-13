using DomainModules.Reservations.Entities;
using DomainModules.Reservations.Enums;
using FluentResults;
using MediatR;

namespace Contracts.ServiceModules.Reservations;

public class RecurrenceOptions
{
    public RecurrenceType RecurrenceType { get; set; } = RecurrenceType.None;

    // For Daily/Weekly/Monthly types: e.g. every 2 days, 3 weeks, etc.
    public int Interval { get; set; } = 1;
    
    public DateTime RecurrenceStartDate { get; set; }
    public DateTime RecurrenceEndDate { get; set; }

    // For Custom recurrence: list the specific dates on which reservations should be created.
    public List<DateTime>? CustomDates { get; set; }
}

public record CreateBlockedReservationTimeSlot(BlockedReservationTimeSlot BlockedReservationTimeSlot, RecurrenceOptions? RecurrenceOptions)  : IRequest<Result<BlockedReservationTimeSlot>>;
public record CreateReservationCommand(Reservation Reservation, RecurrenceOptions? RecurrenceOptions) : IRequest<Result<List<Reservation>>>;
public record CreateReservationLocation(ReservationLocation ReservationLocation)  : IRequest<Result<ReservationLocation>>;
public record UpdateReservationLocation(ReservationLocation ReservationLocation)  : IRequest<Result<ReservationLocation>>;
public record DeleteReservationCommand(int Id, Guid Token) : IRequest<Result>;
public record DeleteRecurringReservationsCommand(Guid SeriesId, DateTime? FromDate, Guid Token) : IRequest<Result<List<Reservation>>>;
public record ApproveReservationsCommand(List<int> ReservationIds, Guid Token) : IRequest<Result<List<Reservation>>>;
public record DeclineReservationsCommand(List<int> ReservationIds, Guid Token) : IRequest<Result<List<Reservation>>>;

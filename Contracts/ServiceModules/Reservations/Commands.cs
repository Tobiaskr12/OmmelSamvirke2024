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

public record CreateBlockedReservationTimeSlotCommand(BlockedReservationTimeSlot BlockedReservationTimeSlot, RecurrenceOptions? RecurrenceOptions)  : IRequest<Result<List<BlockedReservationTimeSlot>>>;
public record CreateReservationCommand(Reservation Reservation, RecurrenceOptions? RecurrenceOptions) : IRequest<Result<List<Reservation>>>;
public record CreateReservationLocationCommand(ReservationLocation ReservationLocation)  : IRequest<Result<ReservationLocation>>;
public record UpdateReservationLocationCommand(ReservationLocation ReservationLocation)  : IRequest<Result<ReservationLocation>>;
public record DeleteReservationCommand(int Id, Guid Token) : IRequest<Result>;
public record DeleteRecurringReservationsCommand(int SeriesId, DateTime? FromDate, Guid Token) : IRequest<Result<List<Reservation>>>;
public record ApproveReservationsCommand(List<int> ReservationIds, Guid Token) : IRequest<Result<List<Reservation>>>;
public record DeclineReservationsCommand(List<int> ReservationIds, Guid Token) : IRequest<Result<List<Reservation>>>;

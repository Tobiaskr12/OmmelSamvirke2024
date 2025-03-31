using DomainModules.Common;
using DomainModules.Reservations.Entities;
using FluentResults;
using MediatR;

namespace Contracts.ServiceModules.Reservations;

public record CreateBlockedReservationTimeSlotCommand(BlockedReservationTimeSlot BlockedReservationTimeSlot, RecurrenceOptions? RecurrenceOptions)  : IRequest<Result<List<BlockedReservationTimeSlot>>>;
public record CreateReservationCommand(Reservation Reservation, RecurrenceOptions? RecurrenceOptions) : IRequest<Result<List<Reservation>>>;
public record CreateReservationLocationCommand(ReservationLocation ReservationLocation)  : IRequest<Result<ReservationLocation>>;
public record UpdateReservationLocationCommand(ReservationLocation ReservationLocation)  : IRequest<Result<ReservationLocation>>;
public record DeleteReservationCommand(int Id, Guid Token) : IRequest<Result>;
public record DeleteRecurringReservationsCommand(int SeriesId, DateTime? FromDate, Guid Token) : IRequest<Result<List<Reservation>>>;
public record ApproveReservationsCommand(List<int> ReservationIds, Guid Token) : IRequest<Result<List<Reservation>>>;
public record DeclineReservationsCommand(List<int> ReservationIds, Guid Token) : IRequest<Result<List<Reservation>>>;

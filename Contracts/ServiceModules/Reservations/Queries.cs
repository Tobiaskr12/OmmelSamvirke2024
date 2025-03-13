using DomainModules.Reservations.Entities;
using FluentResults;
using MediatR;

namespace Contracts.ServiceModules.Reservations;

public record GetReservation(int Id)  : IRequest<Result<Reservation>>;
public record GetReservations(DateTime StartTime, TimeSpan TimeSpan)  : IRequest<Result<List<Reservation>>>;
public record GetBlockedTimeSlots(DateTime StartTime, TimeSpan TimeSpan)  : IRequest<Result<List<BlockedReservationTimeSlot>>>;
public record GetReservationsHistory(Guid Token)  : IRequest<Result<ReservationHistory>>;

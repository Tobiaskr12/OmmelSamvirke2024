using DomainModules.Reservations.Entities;
using FluentResults;
using MediatR;

namespace Contracts.ServiceModules.Reservations;

public record GetReservationQuery(int Id)  : IRequest<Result<Reservation>>;
public record GetReservationsQuery(DateTime StartTime, TimeSpan TimeSpan)  : IRequest<Result<List<Reservation>>>;
public record GetBlockedTimeSlotsQuery(DateTime StartTime, TimeSpan TimeSpan)  : IRequest<Result<List<BlockedReservationTimeSlot>>>;
public record GetReservationsHistoryQuery(Guid Token)  : IRequest<Result<ReservationHistory>>;
public record GetReservationsInSeriesQuery(int SeriesId) : IRequest<Result<List<Reservation>>>;

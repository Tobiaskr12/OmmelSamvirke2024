using Contracts.DataAccess.Base;
using Contracts.ServiceModules.Reservations;
using DomainModules.Reservations.Entities;
using FluentResults;
using MediatR;
using ServiceModules.Errors;

namespace ServiceModules.Reservations.Queries;

public class GetReservationQueryHandler : IRequestHandler<GetReservationQuery, Result<Reservation>>
{
    private readonly IRepository<Reservation> _reservationRepository;

    public GetReservationQueryHandler(IRepository<Reservation> reservationRepository)
    {
        _reservationRepository = reservationRepository;
    }

    public async Task<Result<Reservation>> Handle(GetReservationQuery request, CancellationToken cancellationToken)
    {
        Result<Reservation> result = await _reservationRepository.GetByIdAsync(request.Id, cancellationToken: cancellationToken);
        if (result.IsFailed || result.Value == null)
        {
            return Result.Fail<Reservation>(ErrorMessages.Reservations_ReservationNotFound);
        }
        
        return Result.Ok(result.Value);
    }
}

using Contracts.DataAccess.Base;
using Contracts.ServiceModules.Reservations;
using DomainModules.Reservations.Entities;
using FluentResults;
using MediatR;
using ServiceModules.Errors;

namespace ServiceModules.Reservations.Queries;

public class GetReservationHistoryQueryHandler : IRequestHandler<GetReservationsHistoryQuery, Result<ReservationHistory>>
{
    private readonly IRepository<ReservationHistory> _reservationHistoryRepository;

    public GetReservationHistoryQueryHandler(IRepository<ReservationHistory> reservationHistoryRepository)
    {
        _reservationHistoryRepository = reservationHistoryRepository;
    }

    public async Task<Result<ReservationHistory>> Handle(GetReservationsHistoryQuery request, CancellationToken cancellationToken)
    {
        Result<List<ReservationHistory>> result = await _reservationHistoryRepository.FindAsync(rh => 
            rh.Token == request.Token,
            cancellationToken: cancellationToken
        );
        ReservationHistory? history = result.Value.FirstOrDefault();
        
        return history == null 
            ? Result.Fail<ReservationHistory>(ErrorMessages.GenericErrorWithRetryPrompt) 
            : Result.Ok(history);
    }
}

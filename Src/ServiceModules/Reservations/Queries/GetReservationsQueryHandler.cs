using Contracts.DataAccess.Base;
using Contracts.ServiceModules.Reservations;
using DomainModules.Reservations.Entities;
using FluentResults;
using MediatR;
using ServiceModules.Errors;

namespace ServiceModules.Reservations.Queries;

public class GetReservationsQueryHandler : IRequestHandler<GetReservationsQuery, Result<List<Reservation>>>
{
    private readonly IRepository<Reservation> _reservationRepository;

    public GetReservationsQueryHandler(IRepository<Reservation> reservationRepository)
    {
        _reservationRepository = reservationRepository;
    }

    public async Task<Result<List<Reservation>>> Handle(GetReservationsQuery request, CancellationToken cancellationToken)
    {
        DateTime endTime = request.StartTime.Add(request.TimeSpan);
        Result<List<Reservation>> result = await _reservationRepository.FindAsync(r => 
            r.StartTime >= request.StartTime && r.StartTime <= endTime, 
            cancellationToken: cancellationToken
        );
        
        return result.IsFailed 
            ? Result.Fail<List<Reservation>>(ErrorMessages.GenericErrorWithRetryPrompt) 
            : Result.Ok(result.Value);
    }
}

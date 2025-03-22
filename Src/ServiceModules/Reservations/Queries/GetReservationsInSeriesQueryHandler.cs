using Contracts.DataAccess.Base;
using Contracts.ServiceModules.Reservations;
using DomainModules.Reservations.Entities;
using FluentResults;
using MediatR;
using ServiceModules.Errors;

namespace ServiceModules.Reservations.Queries;

public class GetReservationsInSeriesQueryHandler : IRequestHandler<GetReservationsInSeriesQuery, Result<List<Reservation>>>
{
    private readonly IRepository<ReservationSeries> _seriesRepository;
        
    public GetReservationsInSeriesQueryHandler(IRepository<ReservationSeries> seriesRepository)
    {
        _seriesRepository = seriesRepository;
    }

    public async Task<Result<List<Reservation>>> Handle(GetReservationsInSeriesQuery request, CancellationToken cancellationToken)
    {
        Result<ReservationSeries> seriesResult = await _seriesRepository.GetByIdAsync(request.SeriesId, readOnly: true, cancellationToken);
        if (seriesResult.IsFailed || seriesResult.Value == null)
        {
            return Result.Fail<List<Reservation>>(ErrorMessages.Reservations_SeriesNotFound);
        }
            
        return Result.Ok(seriesResult.Value.Reservations);
    }
}

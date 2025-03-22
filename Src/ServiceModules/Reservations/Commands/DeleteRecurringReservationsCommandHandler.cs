using Contracts.DataAccess.Base;
using Contracts.ServiceModules.Reservations;
using DomainModules.Reservations.Entities;
using FluentResults;
using MediatR;
using ServiceModules.Errors;

namespace ServiceModules.Reservations.Commands;

public class DeleteRecurringReservationsCommandHandler : IRequestHandler<DeleteRecurringReservationsCommand, Result<List<Reservation>>>
{
    private readonly IRepository<ReservationSeries> _seriesRepository;
    private readonly IRepository<Reservation> _reservationRepository;
    private readonly IRepository<ReservationHistory> _reservationHistoryRepository;

    public DeleteRecurringReservationsCommandHandler(
        IRepository<ReservationSeries> seriesRepository,
        IRepository<Reservation> reservationRepository,
        IRepository<ReservationHistory> reservationHistoryRepository)
    {
        _seriesRepository = seriesRepository;
        _reservationRepository = reservationRepository;
        _reservationHistoryRepository = reservationHistoryRepository;
    }

    public async Task<Result<List<Reservation>>> Handle(DeleteRecurringReservationsCommand request, CancellationToken cancellationToken)
    {
        // Retrieve the reservation series using the provided SeriesId
        Result<ReservationSeries> seriesResult = await _seriesRepository.GetByIdAsync(request.SeriesId, readOnly: false, cancellationToken);
        if (seriesResult.IsFailed || seriesResult.Value == null)
        {
            return Result.Fail<List<Reservation>>(ErrorMessages.Reservations_SeriesNotFound);
        }
        ReservationSeries? series = seriesResult.Value;

        // Validate token ownership by checking one of the reservations in the series
        if (series.Reservations.Count == 0)
        {
            return Result.Fail<List<Reservation>>(ErrorMessages.Reservations_SeriesNotFound);
        }
        Reservation sampleReservation = series.Reservations.First();
        Result<List<ReservationHistory>> historyResult = await _reservationHistoryRepository.FindAsync(rh => rh.Token == request.Token, cancellationToken: cancellationToken);
        if (historyResult.Value.Count == 0)
        {
            return Result.Fail<List<Reservation>>(ErrorMessages.Reservations_TokenMismatch);
        }
        ReservationHistory history = historyResult.Value.First();
        if (!string.Equals(history.Email, sampleReservation.Email, StringComparison.OrdinalIgnoreCase))
        {
            return Result.Fail<List<Reservation>>(ErrorMessages.Reservations_TokenMismatch);
        }

        // Determine the cutoff date: use request.FromDate if provided, otherwise DateTime.UtcNow
        DateTime cutoff = request.FromDate ?? DateTime.UtcNow;

        // Filter the reservations in the series that occur from the cutoff date onward
        List<Reservation> reservationsToDelete = series.Reservations.Where(r => r.StartTime >= cutoff).ToList();
        if (reservationsToDelete.Count == 0)
        {
            return Result.Ok(new List<Reservation>()); // Nothing to delete
        }

        // Delete the filtered reservations
        Result deleteResult = await _reservationRepository.DeleteAsync(reservationsToDelete, cancellationToken);
        if (deleteResult.IsFailed)
        {
            return Result.Fail<List<Reservation>>(ErrorMessages.GenericErrorWithRetryPrompt);
        }

        // Update the series to remove the deleted reservations
        series.Reservations = series.Reservations.Except(reservationsToDelete).ToList();
        await _seriesRepository.UpdateAsync(series, cancellationToken);

        return Result.Ok(reservationsToDelete);
    }
}

using Contracts.DataAccess.Base;
using Contracts.ServiceModules.Reservations;
using DomainModules.Reservations.Entities;
using FluentResults;
using MediatR;
using ServiceModules.Errors;

namespace ServiceModules.Reservations.Commands;

public class DeleteReservationCommandHandler : IRequestHandler<DeleteReservationCommand, Result>
{
    private readonly IRepository<Reservation> _reservationRepository;
    private readonly IRepository<ReservationHistory> _reservationHistoryRepository;

    public DeleteReservationCommandHandler(
        IRepository<Reservation> reservationRepository,
        IRepository<ReservationHistory> reservationHistoryRepository)
    {
        _reservationRepository = reservationRepository;
        _reservationHistoryRepository = reservationHistoryRepository;
    }

    public async Task<Result> Handle(DeleteReservationCommand request, CancellationToken cancellationToken)
    {
        Result<Reservation> reservationResult = await _reservationRepository.GetByIdAsync(request.Id, readOnly: false, cancellationToken);
        if (reservationResult.IsFailed || reservationResult.Value == null)
        {
            return Result.Fail(ErrorMessages.Reservations_ReservationNotFound);
        }

        Reservation? reservation = reservationResult.Value;

        // Validate token by ensuring the reservation belongs to a ReservationHistory with the provided token
        Result<List<ReservationHistory>> historyResult = await _reservationHistoryRepository.FindAsync(rh => 
            rh.Token == request.Token, 
            cancellationToken: cancellationToken
        );
        if (historyResult.Value.Count == 0)
        {
            return Result.Fail(ErrorMessages.Reservations_TokenMismatch);
        }

        ReservationHistory history = historyResult.Value.First();
        if (!string.Equals(history.Email, reservation.Email, StringComparison.OrdinalIgnoreCase))
        {
            return Result.Fail(ErrorMessages.Reservations_TokenMismatch);
        }

        Result deleteResult = await _reservationRepository.DeleteAsync(reservation, cancellationToken);
        
        return deleteResult.IsFailed 
            ? Result.Fail(ErrorMessages.GenericErrorWithRetryPrompt) 
            : Result.Ok();
    }
}

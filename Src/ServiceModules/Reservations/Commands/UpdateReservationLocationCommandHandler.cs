using Contracts.DataAccess.Base;
using Contracts.ServiceModules.Reservations;
using DomainModules.Reservations.Entities;
using FluentResults;
using FluentValidation;
using FluentValidation.Results;
using JetBrains.Annotations;
using MediatR;
using ServiceModules.Errors;

namespace ServiceModules.Reservations.Commands;

[UsedImplicitly]
public class UpdateReservationLocationCommandValidator : AbstractValidator<UpdateReservationLocationCommand>
{
    public UpdateReservationLocationCommandValidator(IValidator<ReservationLocation> reservationLocationValidator)
    {
        RuleFor(x => x.ReservationLocation).SetValidator(reservationLocationValidator);
    }
}

public class UpdateReservationLocationCommandHandler : IRequestHandler<UpdateReservationLocationCommand, Result<ReservationLocation>>
{
    private readonly IRepository<ReservationLocation> _locationRepository;

    public UpdateReservationLocationCommandHandler(IRepository<ReservationLocation> locationRepository)
    {
        _locationRepository = locationRepository;
    }

    public async Task<Result<ReservationLocation>> Handle(UpdateReservationLocationCommand request, CancellationToken cancellationToken)
    {
        Result<ReservationLocation> updateResult = await _locationRepository.UpdateAsync(request.ReservationLocation, cancellationToken);
        
        return updateResult.IsFailed 
            ? Result.Fail<ReservationLocation>(ErrorMessages.GenericErrorWithRetryPrompt) 
            : Result.Ok(updateResult.Value);
    }
}

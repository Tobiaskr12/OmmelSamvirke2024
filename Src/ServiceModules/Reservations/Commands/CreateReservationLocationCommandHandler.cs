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
public class CreateReservationLocationCommandValidator : AbstractValidator<CreateReservationLocationCommand>
{
    public CreateReservationLocationCommandValidator(IValidator<ReservationLocation> reservationLocationValidator)
    {
        RuleFor(x => x.ReservationLocation).SetValidator(reservationLocationValidator);
    }
}

public class CreateReservationLocationCommandHandler : IRequestHandler<CreateReservationLocationCommand, Result<ReservationLocation>>
{
    private readonly IRepository<ReservationLocation> _locationRepository;

    public CreateReservationLocationCommandHandler(IRepository<ReservationLocation> locationRepository)
    {
        _locationRepository = locationRepository;
    }

    public async Task<Result<ReservationLocation>> Handle(CreateReservationLocationCommand request, CancellationToken cancellationToken)
    {
        Result<ReservationLocation> addResult = await _locationRepository.AddAsync(request.ReservationLocation, cancellationToken);
        return addResult.IsFailed 
            ? Result.Fail<ReservationLocation>(ErrorMessages.GenericErrorWithRetryPrompt) 
            : Result.Ok(addResult.Value);
    }
}

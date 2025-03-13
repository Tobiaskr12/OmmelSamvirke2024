using DomainModules.Errors;
using DomainModules.Reservations.Entities;
using FluentValidation;

namespace DomainModules.Reservations.Validators;

public class ReservationLocationValidator : AbstractValidator<ReservationLocation>
{
    public ReservationLocationValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage(ErrorMessages.ReservationLocation_Name_InvalidLength)
            .Length(3, 75)
            .WithMessage(ErrorMessages.ReservationLocation_Name_InvalidLength);
    }
}

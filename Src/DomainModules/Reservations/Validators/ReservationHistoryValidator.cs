using DomainModules.Emails.Validators;
using DomainModules.Errors;
using DomainModules.Reservations.Entities;
using FluentValidation;

namespace DomainModules.Reservations.Validators;

public class ReservationHistoryValidator : AbstractValidator<ReservationHistory>
{
    public ReservationHistoryValidator(IValidator<Reservation> reservationValidator)
    {
        RuleFor(x => x.Email)
            .Must(RecipientValidator.IsEmailStructureValid)
            .WithMessage(ErrorMessages.ReservationHistory_Email_InvalidStructure);

        RuleFor(x => x.Token)
            .NotEmpty()
            .WithMessage(ErrorMessages.ReservationHistory_Token_Empty);
        
        RuleForEach(x => x.Reservations).SetValidator(reservationValidator);
    }
}

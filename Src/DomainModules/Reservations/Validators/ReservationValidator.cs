using DomainModules.Emails.Validators;
using DomainModules.Errors;
using DomainModules.Reservations.Entities;
using FluentValidation;

namespace DomainModules.Reservations.Validators;

public class ReservationValidator : AbstractValidator<Reservation>
{
    public ReservationValidator()
    {
        RuleFor(x => x.Email)
            .Must(RecipientValidator.IsEmailStructureValid)
            .WithMessage(ErrorMessages.Reservation_Email_InvalidStructure);

        RuleFor(x => x.PhoneNumber)
            .NotEmpty()
            .WithMessage(ErrorMessages.Reservation_PhoneNumber_Empty)
            .Length(5, 20)
            .WithMessage(ErrorMessages.Reservation_PhoneNumber_InvalidLength);
        
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage(ErrorMessages.Reservation_Name_Empty)
            .Length(3, 100)
            .WithMessage(ErrorMessages.Reservation_Name_InvalidLength);
        
        RuleFor(x => x.CommunityName)
            .Length(3, 75)
            .When(x => x.CommunityName is not null)
            .WithMessage(ErrorMessages.Reservation_CommunityName_InvalidLength);
        
        RuleFor(x => x.StartTime)
            .GreaterThanOrEqualTo(_ => DateTime.UtcNow)
            .WithMessage(ErrorMessages.Reservation_StartTIme_InPast);

        RuleFor(x => x.EndTime)
            .GreaterThanOrEqualTo(x => x.StartTime.AddHours(1))
            .WithMessage(ErrorMessages.Reservation_EndTime_MustBeAfterStart);
    }
}

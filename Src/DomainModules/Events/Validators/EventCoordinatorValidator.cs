using DomainModules.Common;
using DomainModules.Errors;
using DomainModules.Events.Entities;
using FluentValidation;

namespace DomainModules.Events.Validators;

public class EventCoordinatorValidator : AbstractValidator<EventCoordinator>
{
    public EventCoordinatorValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage(ErrorMessages.EventCoordinator_Name_NotEmpty)
            .Length(2, 100)
            .WithMessage(ErrorMessages.EventCoordinator_Name_InvalidLength);
            
        When(x => !string.IsNullOrWhiteSpace(x.EmailAddress), () =>
        {
            RuleFor(x => x.EmailAddress)
                .Must(ValidationUtils.IsEmailStructureValid!)
                .WithMessage(ErrorMessages.EventCoordinator_EmailAddress_Invalid);
        });
            
        When(x => !string.IsNullOrWhiteSpace(x.PhoneNumber), () =>
        {
            RuleFor(x => x.PhoneNumber)
                .Must(ValidationUtils.IsValidPhoneNumber!)
                .WithMessage(ErrorMessages.EventCoordinator_PhoneNumber_Invalid);
        });
    }
}

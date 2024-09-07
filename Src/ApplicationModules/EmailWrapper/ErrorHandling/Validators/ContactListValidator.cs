using EmailWrapper.ErrorHandling.ErrorMessages;
using EmailWrapper.Models;
using FluentValidation;

namespace EmailWrapper.ErrorHandling.Validators;

public class ContactListValidator : AbstractValidator<ContactList>
{
    public ContactListValidator()
    {
        RuleFor(x => x.Name)
            .NotNull()
            .WithMessage(EmailWrapperErrorMessages.ContactList_InvalidNameLength)
            .Length(3, 200)
            .WithMessage(EmailWrapperErrorMessages.ContactList_InvalidNameLength);

        RuleFor(x => x.Description)
            .NotNull()
            .WithMessage(EmailWrapperErrorMessages.ContactList_InvalidDescriptionLength)
            .Length(5, 2000)
            .WithMessage(EmailWrapperErrorMessages.ContactList_InvalidDescriptionLength);
    }
}

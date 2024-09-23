using Emails.Domain.Entities;
using Emails.Domain.Errors;
using FluentValidation;

namespace Emails.Domain.Validators;

public class ContactListValidator : AbstractValidator<ContactList>
{
    public ContactListValidator()
    {
        RuleFor(x => x.Name)
            .NotNull()
            .WithMessage(ErrorMessages.ContactList_InvalidNameLength)
            .Length(3, 200)
            .WithMessage(ErrorMessages.ContactList_InvalidNameLength);

        RuleFor(x => x.Description)
            .NotNull()
            .WithMessage(ErrorMessages.ContactList_InvalidDescriptionLength)
            .Length(5, 2000)
            .WithMessage(ErrorMessages.ContactList_InvalidDescriptionLength);
    }
}

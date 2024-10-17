using Emails.Domain.Entities;
using Emails.Domain.Errors;
using FluentValidation;

namespace Emails.Domain.Validators;

public class ContactListValidator : AbstractValidator<ContactList>
{
    public ContactListValidator(IValidator<Recipient> recipientValidator)
    {
        RuleFor(x => x.Name)
            .NotNull()
            .WithMessage(ErrorMessages.ContactList_Name_InvalidLength)
            .Length(3, 200)
            .WithMessage(ErrorMessages.ContactList_Name_InvalidLength);

        RuleFor(x => x.Description)
            .NotNull()
            .WithMessage(ErrorMessages.ContactList_Description_InvalidLength)
            .Length(5, 2000)
            .WithMessage(ErrorMessages.ContactList_Description_InvalidLength);

        RuleForEach(x => x.Contacts).SetValidator(recipientValidator);
    }
}

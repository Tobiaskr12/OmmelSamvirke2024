using FluentValidation;
using OmmelSamvirke.DomainModules.Emails.Entities;
using OmmelSamvirke.DomainModules.Errors;

namespace OmmelSamvirke.DomainModules.Emails.Validators;

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

        // No error messages, as this can't be corrected by the user
        RuleFor(x => x.UnsubscribeToken)
            .NotNull()
            .NotEqual(Guid.Empty);

        RuleForEach(x => x.Contacts).SetValidator(recipientValidator);
    }
}

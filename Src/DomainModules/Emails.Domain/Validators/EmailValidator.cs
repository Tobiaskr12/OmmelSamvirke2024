using Emails.Domain.Entities;
using Emails.Domain.Errors;
using FluentValidation;

namespace Emails.Domain.Validators;

public class EmailValidator : AbstractValidator<Email>
{
    public EmailValidator()
    {
        RuleFor(x => x.Subject)
            .NotNull()
            .WithMessage(ErrorMessages.ContactList_InvalidNameLength);
    }
}

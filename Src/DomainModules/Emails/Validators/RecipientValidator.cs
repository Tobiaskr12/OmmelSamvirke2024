using System.Net.Mail;
using DomainModules.Common;
using FluentValidation;
using DomainModules.Emails.Entities;
using DomainModules.Errors;

namespace DomainModules.Emails.Validators;

public class RecipientValidator : AbstractValidator<Recipient>
{
    public RecipientValidator()
    {
        RuleFor(x => x.EmailAddress)
            .Must(ValidationUtils.IsEmailStructureValid)
            .WithMessage(ErrorMessages.Recipient_EmailAddress_MustBeValid);
    }
}

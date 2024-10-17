using System.Net.Mail;
using Emails.Domain.Entities;
using Emails.Domain.Errors;
using FluentValidation;

namespace Emails.Domain.Validators;

public class RecipientValidator : AbstractValidator<Recipient>
{
    public RecipientValidator()
    {
        RuleFor(x => x.EmailAddress)
            .Must(IsEmailStructureValid)
            .WithMessage(ErrorMessages.Recipient_EmailAddress_MustBeValid);
    }
    
    private static bool IsEmailStructureValid(string emailAddress)
    {
        if (string.IsNullOrWhiteSpace(emailAddress)) return false;
        try
        {
            var addr = new MailAddress(emailAddress);

            // Ensure the address matches exactly (handles cases like extra spaces)
            if (addr.Address != emailAddress)
                return false;

            // Split the email address into local and domain parts
            int index = emailAddress.LastIndexOf('@');

            string localPart = emailAddress[..index];
            string domainPart = emailAddress[(index + 1)..];

            // Check for consecutive dots in local part
            if (localPart.Contains(".."))
                return false;

            // Check for consecutive dots in domain part
            return !domainPart.Contains("..");
        }
        catch (FormatException)
        {
            return false;
        }
    }
}

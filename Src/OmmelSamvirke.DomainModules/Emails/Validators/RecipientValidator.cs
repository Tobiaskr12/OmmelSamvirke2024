using System.Net.Mail;
using FluentValidation;
using OmmelSamvirke.DomainModules.Emails.Entities;
using OmmelSamvirke.DomainModules.Errors;

namespace OmmelSamvirke.DomainModules.Emails.Validators;

public class RecipientValidator : AbstractValidator<Recipient>
{
    public RecipientValidator()
    {
        RuleFor(x => x.EmailAddress)
            .Must(IsEmailStructureValid)
            .WithMessage(ErrorMessages.Recipient_EmailAddress_MustBeValid);
    }
    
    public static bool IsEmailStructureValid(string emailAddress)
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

            // Check for consecutive dots in local and domain parts
            return !localPart.Contains("..") && !domainPart.Contains("..");
        }
        catch (FormatException)
        {
            return false;
        }
    }
}

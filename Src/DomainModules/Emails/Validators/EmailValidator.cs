using FluentValidation;
using DomainModules.Emails.Constants;
using DomainModules.Emails.Entities;
using DomainModules.Errors;

namespace DomainModules.Emails.Validators;

public class EmailValidator : AbstractValidator<Email>
{
    private const int OneMb = 1024 * 1024;
    
    public EmailValidator(IValidator<Recipient> recipientValidator, IValidator<Attachment> attachmentValidator)
    {
        RuleFor(x => x.SenderEmailAddress)
            .NotEmpty()
            .WithMessage(ErrorMessages.Email_SenderAddress_MustNotBeEmpty)
            .Must(SenderEmailAddressMustBeInValidSenderEmailAddresses)
            .WithMessage(ErrorMessages.Email_SenderAddress_MustBeApproved);
        
        RuleFor(x => x.Subject)
            .NotNull()
            .WithMessage(ErrorMessages.Email_Subject_InvalidLength)
            .Length(3, 80)
            .WithMessage(ErrorMessages.Email_Subject_InvalidLength);

        RuleFor(x => x.HtmlBody)
            .NotEmpty()
            .WithMessage(ErrorMessages.Email_Body_InvalidLength)
            .Length(20, 7 * OneMb / 2)
            .WithMessage(ErrorMessages.Email_Body_InvalidLength);
        
        RuleFor(x => x.PlainTextBody)
            .NotEmpty()
            .WithMessage(ErrorMessages.Email_Body_InvalidLength)
            .Length(20, 7 * OneMb / 2)
            .WithMessage(ErrorMessages.Email_Body_InvalidLength);

        RuleFor(x => x.Recipients)
            .NotNull()
            .WithMessage(ErrorMessages.Email_Recipient_InvalidSize)
            .Must(x => x.Count is >= 1 and <= ServiceLimits.RecipientsPerEmail)
            .WithMessage(ErrorMessages.Email_Recipient_InvalidSize)
            .Must(MustBeUnique)
            .WithMessage(ErrorMessages.Email_Recipients_MustBeUnique);
        
        RuleFor(x => x.Attachments)
            .NotNull()
            .WithMessage(ErrorMessages.Email_Attachments_InvalidSize)
            .Must(x => x.Count <= 10)
            .WithMessage(ErrorMessages.Email_Attachments_InvalidSize)
            .Must(MustBeUnique)
            .WithMessage(ErrorMessages.Email_Attachments_MustBeUnique);

        RuleFor(x => x)
            .Must(HaveValidContentSize)
            .WithMessage(ErrorMessages.Email_ContentSize_TooLarge);
        
        RuleForEach(x => x.Recipients).SetValidator(recipientValidator);
        RuleForEach(x => x.Attachments).SetValidator(attachmentValidator);
    }

    private static bool MustBeUnique(List<Recipient> recipients)
    {
        return !recipients.GroupBy(x => x.EmailAddress).Any(x => x.Count() > 1);
    }
    
    private static bool MustBeUnique(List<Attachment> attachments)
    {
        return !attachments.GroupBy(x => x.Name).Any(x => x.Count() > 1);
    }

    private static bool SenderEmailAddressMustBeInValidSenderEmailAddresses(string emailAddress)
    {
        return ValidSenderEmailAddresses.AcceptedEmailAddresses.Contains(emailAddress);
    }
    
    private static bool HaveValidContentSize(Email email)
    {
        int subjectSize = email.Subject.Length * sizeof(char);
        int bodySize = email.HtmlBody.Length * sizeof(char) + email.PlainTextBody.Length * sizeof(char);
        long attachmentsSize = email.Attachments.Sum(attachment => attachment.ContentSize);
        
        long totalSize = subjectSize + bodySize + attachmentsSize;

        return totalSize <= ServiceLimits.MaxEmailRequestSizeInBytes;
    }
}

using System.Net.Mail;
using System.Text;
using EmailWrapper.Constants;
using FluentResults;
using OmmelSamvirke2024.Domain;

namespace EmailWrapper.Models;

public class Email : BaseEntity
{
    public required string SenderEmailAddress { get; set; }
    public required string Subject { get; set; }
    public required string Body { get; set; }
    public required List<Recipient> Recipients { get; set; }
    public required List<Attachment> Attachments { get; set; }
    
    public static Result<Email> Create(
        string fromAddress,
        string subject,
        string body,
        Recipient recipient,
        List<Attachment>? attachments = null)
    {
        return Create(fromAddress, subject, body, new List<Recipient> { recipient }, attachments);
    }

    public static Result<Email> Create(
        string fromAddress,
        string subject,
        string body,
        List<Recipient>? recipients,
        List<Attachment>? attachments = null)
    {
        const int twentyMb = 20 * 1024 * 1024;

        if (!AcceptedSenderEmails().Contains(fromAddress))
            return Result.Fail("The 'from' email address is not on the list of accepted sender email address");
        if (string.IsNullOrWhiteSpace(subject))
            return Result.Fail("Subject is required.");
        if (subject.Length > 80)
            return Result.Fail("Subject must not be longer than 80 characters");
        if (string.IsNullOrWhiteSpace(body))
            return Result.Fail("Body is required.");
        if (recipients == null || recipients.Count == 0)
            return Result.Fail("At least one recipient is required.");
        if (attachments == null && Encoding.Unicode.GetByteCount(subject + body) > twentyMb)
            return Result.Fail("The total size of the email cannot exceed 20MB.");
        if (attachments != null && attachments.Sum(att => att.ContentStream.Length) + Encoding.Unicode.GetByteCount(subject + body) > twentyMb)
            return Result.Fail("The total size of the email cannot exceed 20MB.");

        var email = new Email
        {
            SenderEmailAddress = fromAddress,
            Subject = subject,
            Body = body,
            Recipients = recipients,
            Attachments = attachments ?? []
        };

        return Result.Ok(email);
    }

    private static List<string> AcceptedSenderEmails()
    {
        return
        [
            SenderEmailAddresses.Admins,
            SenderEmailAddresses.Auth,
            SenderEmailAddresses.Auto,
            SenderEmailAddresses.Newsletter
        ];
    }
}

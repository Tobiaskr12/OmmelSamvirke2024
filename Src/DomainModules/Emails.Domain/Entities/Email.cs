using Domain.Common;

namespace Emails.Domain.Entities;

public class Email : BaseEntity
{
    public required string SenderEmailAddress { get; init; }
    public required string Subject { get; init; }
    public required string Body { get; set; }
    public required List<Recipient> Recipients { get; init; }
    public required List<Attachment> Attachments { get; init; }
}

// TODO - Implement as validators!
// if (!AcceptedSenderEmails().Contains(fromAddress))
//     return Result.Fail("The 'from' email address is not on the list of accepted sender email address");
// if (string.IsNullOrWhiteSpace(subject))
//     return Result.Fail("Subject is required.");
// if (subject.Length > 80)
//     return Result.Fail("Subject must not be longer than 80 characters");
// if (string.IsNullOrWhiteSpace(body))
//     return Result.Fail("Body is required.");
// if (recipients.Count == 0)
//     return Result.Fail("At least one recipient is required.");
// if (attachments == null && Encoding.Unicode.GetByteCount(subject + body) > twentyMb)
//     return Result.Fail("The total size of the email cannot exceed 20MB.");
// if (attachments != null && attachments.Sum(att => att.ContentSize) + Encoding.Unicode.GetByteCount(subject + body) > twentyMb)
//     return Result.Fail("The total size of the email cannot exceed 20MB.");
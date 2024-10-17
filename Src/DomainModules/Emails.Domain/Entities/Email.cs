using Domain.Common;

namespace Emails.Domain.Entities;

public class Email : BaseEntity
{
    public required string SenderEmailAddress { get; set; }
    public required string Subject { get; set; }
    public required string Body { get; set; }
    public required List<Recipient> Recipients { get; set; }
    public required List<Attachment> Attachments { get; set; }
}

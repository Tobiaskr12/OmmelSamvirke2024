using DomainModules.Common;

namespace DomainModules.Emails.Entities;

public class Email : BaseEntity
{
    public required string SenderEmailAddress { get; set; }
    public required string Subject { get; set; }
    public required string HtmlBody { get; set; }
    public required string PlainTextBody { get; set; }
    public required List<Recipient> Recipients { get; set; } = [];
    public required List<Attachment> Attachments { get; set; } = [];

    public bool IsNewsletter { get; set; } = false;
}

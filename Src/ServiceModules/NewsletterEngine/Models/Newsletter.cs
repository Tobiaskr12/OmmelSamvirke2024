using Domain.Common;
using Emails.Domain.Entities;

namespace NewsletterEngine.Models;

public class Newsletter : BaseEntity
{
    public required string Title { get; set; }
    public required string Body { get; set; }
    public required string PlainTextBody { get; set; }
    public required List<PdfAttachment> PdfAttachments { get; set; }
    public required int AdminId { get; set; }
    public required List<ContactList> EmailAudience { get; set; }

    public Email ToEmail()
    {
        // TODO - Implement
        throw new NotImplementedException();
    }
}
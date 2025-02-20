using DomainModules.Common;
using DomainModules.Emails.Entities;

namespace DomainModules.Newsletters.Entities;

public class NewsletterSubscriptionConfirmation : BaseEntity
{
    public Guid ConfirmationToken { get; init; } = Guid.NewGuid();
    public DateTime ConfirmationExpiry { get; set; } = DateTime.UtcNow.AddDays(7);
    public bool IsConfirmed { get; set; } = false;
    public DateTime? ConfirmationTime { get; set; }
    
    public required Recipient Recipient { get; set; }
    public List<NewsletterGroup> NewsletterGroups { get; set; } = [];
}

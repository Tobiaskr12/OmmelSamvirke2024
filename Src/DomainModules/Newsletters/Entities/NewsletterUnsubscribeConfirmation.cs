using DomainModules.Common;

namespace DomainModules.Newsletters.Entities;

public class NewsletterUnsubscribeConfirmation : BaseEntity
{
    public Guid ConfirmationToken { get; } = Guid.NewGuid();
    public required DateTime ConfirmationExpiry { get; set; }
    public bool IsConfirmed { get; set; } = false;
    public DateTime? ConfirmationTime { get; set; }
}

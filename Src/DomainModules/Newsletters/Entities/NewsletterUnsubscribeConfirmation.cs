using DomainModules.Common;

namespace DomainModules.Newsletters.Entities;

public class NewsletterUnsubscribeConfirmation : BaseEntity
{
    public Guid ConfirmationToken { get; } = Guid.NewGuid();
    public DateTime ConfirmationExpiry { get; set; } = DateTime.UtcNow.AddDays(7);
    public bool IsConfirmed { get; set; } = false;
    public DateTime? ConfirmationTime { get; set; }
}

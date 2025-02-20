using DomainModules.Common;
using DomainModules.Newsletters.Entities;

namespace DomainModules.Emails.Entities;

public class Recipient : BaseEntity
{
    public required string EmailAddress { get; set; }
    public Guid Token { get; init; } = Guid.NewGuid();
    
    public List<NewsletterSubscriptionConfirmation> NewsletterSubscriptionConfirmations { get; set; } = [];
}

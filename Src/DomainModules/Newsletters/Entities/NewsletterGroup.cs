using DomainModules.Common;
using DomainModules.Emails.Entities;

namespace DomainModules.Newsletters.Entities;

public class NewsletterGroup : BaseEntity
{
    public required string Name { get; set; }
    public required string Description { get; set; }
    public required ContactList ContactList { get; set; }
    
    public List<NewsletterSubscriptionConfirmation> NewsletterSubscriptionConfirmations { get; set; } = [];
    public List<NewsletterUnsubscribeConfirmation> NewsletterUnsubscribeConfirmations { get; set; } = [];
}

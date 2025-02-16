using DomainModules.Common;
using DomainModules.Emails.Entities;

namespace DomainModules.Newsletters.Entities;

public class Newsletter : BaseEntity
{
    public required Email Email { get; set; }
    public required List<NewsletterGroup> NewsletterGroups { get; set; } = [];
}

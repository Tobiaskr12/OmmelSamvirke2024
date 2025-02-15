using DomainModules.Common;

namespace DomainModules.Emails.Entities;

public class ContactList : BaseEntity
{
    public required string Name { get; set; }
    public required string Description { get; set; }
    public Guid UnsubscribeToken { get; } = Guid.NewGuid();
    public List<Recipient> Contacts { get; set; } = [];
}

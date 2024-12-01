using OmmelSamvirke.DomainModules.Common;

namespace OmmelSamvirke.DomainModules.Emails.Entities;

public class ContactList : BaseEntity
{
    public required string Name { get; set; }
    public required string Description { get; set; }
    public List<Recipient> Contacts { get; set; } = [];
}

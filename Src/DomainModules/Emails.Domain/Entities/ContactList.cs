using Domain.Common;

namespace Emails.Domain.Entities;

public class ContactList : BaseEntity
{
    public required string Name { get; set; }
    public required string Description { get; set; }
    public List<Recipient> Contacts { get; set; } = [];
}

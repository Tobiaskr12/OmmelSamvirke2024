using OmmelSamvirke2024.Domain;

namespace EmailWrapper.Models;

public class ContactList : BaseEntity
{
    public required string Name { get; set; }
    public required string Description { get; set; }
    
    public List<Recipient> Contacts { get; set; } = [];
}

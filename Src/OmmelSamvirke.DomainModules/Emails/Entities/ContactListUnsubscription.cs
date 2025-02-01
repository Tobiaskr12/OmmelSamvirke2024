using OmmelSamvirke.DomainModules.Common;

namespace OmmelSamvirke.DomainModules.Emails.Entities;

public class ContactListUnsubscription : BaseEntity
{
    public Guid UndoToken { get; } = Guid.NewGuid();
    public required string EmailAddress { get; init; }
    public required int ContactListId { get; init; }
}

using OmmelSamvirke.DomainModules.Common;

namespace OmmelSamvirke.DomainModules.Emails.Entities;

public class Recipient : BaseEntity
{
    public required string EmailAddress { get; set; }
}

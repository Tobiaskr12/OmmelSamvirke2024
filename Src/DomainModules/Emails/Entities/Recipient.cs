using DomainModules.Common;

namespace DomainModules.Emails.Entities;

public class Recipient : BaseEntity
{
    public required string EmailAddress { get; set; }
}

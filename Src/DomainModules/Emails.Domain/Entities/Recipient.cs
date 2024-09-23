using Domain.Common;

namespace Emails.Domain.Entities;

public class Recipient : BaseEntity
{
    public required string Email { get; set; }
}

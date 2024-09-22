using OmmelSamvirke2024.Domain;

namespace EmailWrapper.Models;

public class Recipient : BaseEntity
{
    public required string Email { get; set; }
}

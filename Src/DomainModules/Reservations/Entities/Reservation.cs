using DomainModules.Common;

namespace DomainModules.Reservations.Entities;

public class Reservation : BaseEntity
{
    public required string Email { get; set; }
    public required string PhoneNumber { get; set; }
    public required string Name { get; set; }
    public required DateTime StartTime { get; set; }
    public required DateTime EndTime { get; set; }
    public string? CommunityName { get; set; }
    
    // TODO - Add optional link to Event
}

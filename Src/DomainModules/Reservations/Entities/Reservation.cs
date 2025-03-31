using DomainModules.Common;
using DomainModules.Events.Entities;
using DomainModules.Reservations.Enums;

namespace DomainModules.Reservations.Entities;

public class Reservation : BaseEntity
{
    public required string Email { get; set; }
    public required string PhoneNumber { get; set; }
    public required string Name { get; set; }
    public required DateTime StartTime { get; set; }
    public required DateTime EndTime { get; set; }
    public string? CommunityName { get; set; }
    public ReservationState State { get; set; } = ReservationState.Pending;
    
    public required ReservationLocation Location { get; set; }
    public int? ReservationSeriesId { get; set; }
    public Event? Event { get; set; }
}

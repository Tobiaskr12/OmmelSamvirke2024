using DomainModules.Common;

namespace DomainModules.Reservations.Entities;

public class BlockedReservationTimeSlot : BaseEntity
{
    public required DateTime StartTime { get; set; }
    public required DateTime EndTime { get; set; }
}

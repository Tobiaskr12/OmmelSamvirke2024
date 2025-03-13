using DomainModules.Common;

namespace DomainModules.Reservations.Entities;

public class ReservationLocation : BaseEntity
{
    public required string Name { get; set; }
}

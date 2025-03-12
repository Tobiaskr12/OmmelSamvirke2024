using DomainModules.Common;

namespace DomainModules.Reservations.Entities;

public class ReservationHistory : BaseEntity
{
    public required string Email { get; set; }
    public Guid Token { get; set; } = Guid.NewGuid();
    public List<Reservation> Reservations { get; set; } = [];
}

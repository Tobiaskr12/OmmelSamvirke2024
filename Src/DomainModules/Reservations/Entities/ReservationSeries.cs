using DomainModules.Common;
using DomainModules.Reservations.Enums;

namespace DomainModules.Reservations.Entities;

public class ReservationSeries : BaseEntity
{
    public RecurrenceType RecurrenceType { get; set; } = RecurrenceType.None;
    public int Interval { get; set; } = 1;
    public DateTime RecurrenceStartDate { get; set; }
    public DateTime RecurrenceEndDate { get; set; }
    public List<Reservation> Reservations { get; set; } = [];
}

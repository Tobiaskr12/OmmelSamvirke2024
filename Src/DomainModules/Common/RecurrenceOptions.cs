namespace DomainModules.Common;

public enum RecurrenceType
{
    None,
    Daily,
    Weekly,
    Monthly,
    Custom
}

public class RecurrenceOptions
{
    public RecurrenceType RecurrenceType { get; set; } = RecurrenceType.None;

    // For Daily/Weekly/Monthly types: e.g. every 2 days, 3 weeks, etc.
    public int Interval { get; set; } = 1;
    
    public DateTime RecurrenceStartDate { get; set; }
    public DateTime RecurrenceEndDate { get; set; }

    // For Custom recurrence: list the specific dates on which an entity should occur
    public List<DateTime>? CustomDates { get; set; }
}

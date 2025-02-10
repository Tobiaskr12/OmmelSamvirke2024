namespace OmmelSamvirke.SupportModules.Logging.Models;

public abstract class TimestampedEntry
{
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
}

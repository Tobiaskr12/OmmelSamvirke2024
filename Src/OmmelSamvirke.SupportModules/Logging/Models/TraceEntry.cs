namespace OmmelSamvirke.SupportModules.Logging.Models;

public class TraceEntry : TimestampedEntry
{
    public required string OperationType { get; init; } // "Command" or "Query"
    public required bool IsSuccess { get; init; }
    public required long ExecutionTimeMs { get; init; }
    public required string RequestName { get; init; }
    
    // Correlation
    public string SessionId { get; set; } = string.Empty;
    public string OperationId { get; set; } = string.Empty;
}

using Contracts.SupportModules.Logging.Enums;

namespace Contracts.SupportModules.Logging.Models;

public class LogEntry : TimestampedEntry
{
    public required LogLevel Level { get; init; }
    public required string Message { get; init; }
    public ExceptionInfo? Exception { get; init; }

    // Correlation
    public string SessionId { get; set; } = string.Empty;
    public string OperationId { get; set; } = string.Empty;

    // Caller metadata
    public required int CallerLineNumber { get; init; }
    public required string CallerMemberName { get; init; }
    public required string CallerServiceName { get; init; }
    public required string CallerAssemblyName { get; init; }
}

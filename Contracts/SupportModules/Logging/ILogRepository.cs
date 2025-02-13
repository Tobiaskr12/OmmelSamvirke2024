using Contracts.SupportModules.Logging.Enums;
using Contracts.SupportModules.Logging.Models;

namespace Contracts.SupportModules.Logging;

public interface ILogRepository
{
    /// <summary>
    /// Returns all log entries for a specific time interval.
    /// All optional parameters act as filters.
    /// </summary>
    /// <param name="start">The start time of the query interval.</param>
    /// <param name="interval">The duration (timespan) of the query interval.</param>
    /// <param name="sessionId">Filter on SessionId.</param>
    /// <param name="operationId">Filter on OperationId.</param>
    /// <param name="level">Filter on LogLevel.</param>
    /// <param name="assemblyName">Filter on the CallerAssemblyName.</param>
    /// <param name="serviceName">Filter on the CallerServiceName.</param>
    /// <param name="memberName">Filter on the CallerMemberName.</param>
    IEnumerable<LogEntry> QueryLogs(
        DateTime start,
        TimeSpan interval,
        string? sessionId = null,
        string? operationId = null,
        LogLevel? level = null,
        string? assemblyName = null,
        string? serviceName = null,
        string? memberName = null);

    /// <summary>
    /// Deletes all logs more than 7 days old
    /// </summary>
    bool DeleteOldLogs();
}

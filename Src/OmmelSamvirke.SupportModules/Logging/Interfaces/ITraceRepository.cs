using OmmelSamvirke.SupportModules.Logging.Models;

namespace OmmelSamvirke.SupportModules.Logging.Interfaces;

public interface ITraceRepository
{
    /// <summary>
    /// Returns the distinct RequestName values for traces logged within the specified time interval.
    /// </summary>
    /// <param name="start">The start time of the query interval.</param>
    /// <param name="interval">The duration (timespan) of the query interval.</param>
    IEnumerable<string> ListDistinctRequestNames(DateTime start, TimeSpan interval);

    /// <summary>
    /// Returns all trace entries for a specific time interval.
    /// All optional parameters act as filters.
    /// </summary>
    /// <param name="start">The start time of the query interval.</param>
    /// <param name="interval">The duration (timespan) of the query interval.</param>
    /// <param name="requestName">Filter on RequestName.</param>
    /// <param name="operationType">Filter on OperationType (e.g. "Command" or "Query").</param>
    /// <param name="isSuccess">Filter on success status.</param>
    /// <param name="minExecutionTimeMs">Minimum execution time (ms).</param>
    /// <param name="maxExecutionTimeMs">Maximum execution time (ms).</param>
    /// <param name="sessionId">Filter on SessionId.</param>
    /// <param name="operationId">Filter on OperationId.</param>
    IEnumerable<TraceEntry> QueryTracesForRequest(
        DateTime start,
        TimeSpan interval,
        string? requestName = null,
        string? operationType = null,
        bool? isSuccess = null,
        long? minExecutionTimeMs = null,
        long? maxExecutionTimeMs = null,
        string? sessionId = null,
        string? operationId = null);
}

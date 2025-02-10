using System.Globalization;
using OmmelSamvirke.SupportModules.Logging.Interfaces;
using OmmelSamvirke.SupportModules.Logging.Models;

namespace OmmelSamvirke.SupportModules.Logging;

public class TraceRepository : ITraceRepository
{
    private readonly string _traceDirectory;
    
    public TraceRepository(ILoggingLocationInfo loggingLocationInfo)
    {
        _traceDirectory = loggingLocationInfo.GetLoggingDirectoryPath();
    }

    public IEnumerable<string> ListDistinctRequestNames(DateTime start, TimeSpan interval)
    {
        DateTime end = start + interval;
        var distinct = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        // Process all trace files in the directory.
        foreach (string file in Directory.GetFiles(_traceDirectory, "*-Traces.csv"))
        {
            // Parse the date from the file name (expected format: ddMMyy-Traces.csv).
            string fileName = Path.GetFileName(file);
            if (fileName.Length < 10) continue;
            
            string datePart = fileName.Substring(0, 10).Replace("-", "");
            if (!DateTime.TryParseExact(
                    datePart,
                    "yyyyMMdd",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out DateTime fileDate)
            ) continue;
            
            // Only include files whose date might fall within the query interval.
            if (fileDate < start.Date || fileDate > end.Date) continue;

            foreach (string line in File.ReadLines(file).Skip(1)) // skip header
            {
                string[] parts = line.Split(',');
                if (parts.Length < 7) continue;
                
                // Column 0 is the timestamp
                if (!DateTime.TryParse(parts[0], out DateTime timestamp)) continue;
                if (timestamp < start || timestamp > end) continue;
                
                // Column 4 is RequestName.
                distinct.Add(parts[4]);
            }
        }
        return distinct;
    }

    public IEnumerable<TraceEntry> QueryTracesForRequest(
        DateTime start,
        TimeSpan interval,
        string? requestName = null,
        string? operationType = null,
        bool? isSuccess = null,
        long? minExecutionTimeMs = null,
        long? maxExecutionTimeMs = null,
        string? sessionId = null,
        string? operationId = null)
    {
        DateTime end = start + interval;

        foreach (string file in Directory.GetFiles(_traceDirectory, "*-Traces.csv"))
        {
            // Parse date from file name.
            string fileName = Path.GetFileName(file);
            if (fileName.Length < 10) continue;
            
            string datePart = fileName.Substring(0, 10).Replace("-", "");
            if (!DateTime.TryParseExact(
                    datePart,
                    "yyyyMMdd",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out DateTime fileDate)
            ) continue;
            
            if (fileDate < start.Date || fileDate > end.Date) continue;

            foreach (string line in File.ReadLines(file).Skip(1)) // skip header
            {
                string[] parts = line.Split(',');
                if (parts.Length < 7) continue;
                
                TraceEntry entry;
                try
                {
                    entry = new TraceEntry
                    {
                        Timestamp = DateTime.Parse(parts[0]).ToUniversalTime(),
                        OperationType = parts[1],
                        IsSuccess = bool.Parse(parts[2]),
                        ExecutionTimeMs = long.Parse(parts[3]),
                        RequestName = parts[4],
                        SessionId = parts[5],
                        OperationId = parts[6]
                    };
                }
                catch
                {
                    // Skip malformed lines.
                    continue;
                }
                if (entry.Timestamp < start || entry.Timestamp > end) continue;
                if (!string.IsNullOrEmpty(requestName) && !entry.RequestName.Equals(requestName, StringComparison.OrdinalIgnoreCase)) continue;
                if (!string.IsNullOrEmpty(operationType) && !entry.OperationType.Equals(operationType, StringComparison.OrdinalIgnoreCase)) continue;
                if (isSuccess.HasValue && entry.IsSuccess != isSuccess.Value) continue;
                if (minExecutionTimeMs.HasValue && entry.ExecutionTimeMs < minExecutionTimeMs.Value) continue;
                if (maxExecutionTimeMs.HasValue && entry.ExecutionTimeMs > maxExecutionTimeMs.Value) continue;
                if (!string.IsNullOrEmpty(sessionId) && !entry.SessionId.Equals(sessionId, StringComparison.OrdinalIgnoreCase)) continue;
                if (!string.IsNullOrEmpty(operationId) && !entry.OperationId.Equals(operationId, StringComparison.OrdinalIgnoreCase)) continue;

                yield return entry;
            }
        }
    }
}

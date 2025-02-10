using System.Globalization;
using OmmelSamvirke.SupportModules.Logging.Enums;
using OmmelSamvirke.SupportModules.Logging.Interfaces;
using OmmelSamvirke.SupportModules.Logging.Models;

namespace OmmelSamvirke.SupportModules.Logging;

public class LogRepository : ILogRepository
{
    private readonly string _logDirectory;
    public LogRepository(ILoggingLocationInfo loggingLocationInfo)
    {
        _logDirectory = loggingLocationInfo.GetLoggingDirectoryPath();
    }

    public IEnumerable<LogEntry> QueryLogs(
        DateTime start,
        TimeSpan interval,
        string? sessionId = null,
        string? operationId = null,
        LogLevel? level = null,
        string? assemblyName = null,
        string? serviceName = null,
        string? memberName = null)
    {
        DateTime end = start + interval;

        foreach (string file in Directory.GetFiles(_logDirectory, "*-Logs.csv"))
        {
            // Parse date from file name (expected format: ddMMyy).
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
                if (parts.Length < 10) continue;
                
                LogEntry entry;
                try
                {
                    entry = new LogEntry
                    {
                        Timestamp = DateTime.Parse(parts[0]).ToUniversalTime(),
                        Level = Enum.Parse<LogLevel>(parts[1]),
                        Message = parts[2],
                        Exception = string.IsNullOrWhiteSpace(parts[3]) ? null : new Exception(parts[3]),
                        SessionId = parts[4],
                        OperationId = parts[5],
                        CallerLineNumber = int.Parse(parts[6]),
                        CallerMemberName = parts[7],
                        CallerServiceName = parts[8],
                        CallerAssemblyName = parts[9]
                    };
                }
                catch
                {
                    // Skip malformed lines
                    continue;
                }
                
                if (entry.Timestamp < start || entry.Timestamp > end) continue;
                if (!string.IsNullOrEmpty(sessionId) && !entry.SessionId.Equals(sessionId, StringComparison.OrdinalIgnoreCase)) continue;
                if (!string.IsNullOrEmpty(operationId) && !entry.OperationId.Equals(operationId, StringComparison.OrdinalIgnoreCase)) continue;
                if (level.HasValue && entry.Level != level.Value) continue;
                if (!string.IsNullOrEmpty(assemblyName) && !entry.CallerAssemblyName.Equals(assemblyName, StringComparison.OrdinalIgnoreCase)) continue;
                if (!string.IsNullOrEmpty(serviceName) && !entry.CallerServiceName.Equals(serviceName, StringComparison.OrdinalIgnoreCase)) continue;
                if (!string.IsNullOrEmpty(memberName) && !entry.CallerMemberName.Equals(memberName, StringComparison.OrdinalIgnoreCase)) continue;

                yield return entry;
            }
        }
    }
}

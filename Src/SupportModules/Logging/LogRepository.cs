using System.Globalization;
using System.Text;
using System.Text.Json;
using Contracts.SupportModules.Logging;
using Contracts.SupportModules.Logging.Enums;
using Contracts.SupportModules.Logging.Models;
using Microsoft.VisualBasic.FileIO;
using SupportModules.Logging.Interfaces;

namespace SupportModules.Logging;

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
            // Parse date from file name (expected format: yyyy-MM-dd-Logs.csv).
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

            using var parser = new TextFieldParser(file)
            {
                TextFieldType = FieldType.Delimited,
                HasFieldsEnclosedInQuotes = true
            };
            parser.SetDelimiters(",");

            // Skip the header line (if not empty):
            if (!parser.EndOfData)
            {
                _ = parser.ReadLine();
            }

            while (!parser.EndOfData)
            {
                string[]? parts = parser.ReadFields();
                if (parts == null || parts.Length < 10) continue;

                LogEntry entry;
                try
                {
                    ExceptionInfo? logException = null;
                    if (!string.IsNullOrEmpty(parts[3]))
                    {
                        string exceptionJson = Encoding.UTF8.GetString(Convert.FromBase64String(parts[3]));
                        if (!string.IsNullOrEmpty(exceptionJson))
                        {
                            logException = JsonSerializer.Deserialize<ExceptionInfo>(exceptionJson);
                        }
                    }

                    entry = new LogEntry
                    {
                        Timestamp = DateTime.Parse(parts[0]).ToUniversalTime(),
                        Level = Enum.Parse<LogLevel>(parts[1]),
                        Message = parts[2],
                        Exception = logException,
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
                    // Skip any malformed lines
                    continue;
                }

                if (entry.Timestamp < start || entry.Timestamp > end) continue;
                if (!string.IsNullOrEmpty(sessionId)
                    && !entry.SessionId.Equals(sessionId, StringComparison.OrdinalIgnoreCase)) continue;
                if (!string.IsNullOrEmpty(operationId)
                    && !entry.OperationId.Equals(operationId, StringComparison.OrdinalIgnoreCase)) continue;
                if (level.HasValue && entry.Level != level.Value) continue;
                if (!string.IsNullOrEmpty(assemblyName)
                    && !entry.CallerAssemblyName.Equals(assemblyName, StringComparison.OrdinalIgnoreCase)) continue;
                if (!string.IsNullOrEmpty(serviceName)
                    && !entry.CallerServiceName.Equals(serviceName, StringComparison.OrdinalIgnoreCase)) continue;
                if (!string.IsNullOrEmpty(memberName)
                    && !entry.CallerMemberName.Equals(memberName, StringComparison.OrdinalIgnoreCase)) continue;

                yield return entry;
            }
        }
    }

    public bool DeleteOldLogs()
    {
        try
        {
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

                if (fileDate.AddDays(7) < DateTime.Now.Date)
                {
                    File.Delete(file);
                }
            }

            return true;
        } catch
        {
            return false;
        }
    }
}

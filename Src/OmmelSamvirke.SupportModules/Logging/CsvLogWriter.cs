using System.Reflection;
using System.Runtime.CompilerServices;
using OmmelSamvirke.SupportModules.Logging.Enums;
using OmmelSamvirke.SupportModules.Logging.Interfaces;
using OmmelSamvirke.SupportModules.Logging.Models;

namespace OmmelSamvirke.SupportModules.Logging;

public class CsvLogWriter : CsvBufferedWriter<LogEntry>, ILoggingHandler
{
    public CsvLogWriter(ICorrelationContext correlationContext, ILoggingLocationInfo loggingLocationInfo)
        : base(correlationContext, loggingLocationInfo) {}

    public void LogInformation(
        string message,
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0,
        [CallerMemberName] string memberName = ""
    ) => EnqueueLog(LogLevel.Information, message, null, filePath, lineNumber, memberName);

    public void LogWarning(
        string message,
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0,
        [CallerMemberName] string memberName = ""
    ) => EnqueueLog(LogLevel.Warning, message, null, filePath, lineNumber, memberName);

    public void LogDebug(
        string message,
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0,
        [CallerMemberName] string memberName = ""
    ) => EnqueueLog(LogLevel.Debug, message, null, filePath, lineNumber, memberName);

    public void LogError(
        Exception? ex,
        string? customMessage = null,
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0,
        [CallerMemberName] string memberName = ""
    )
    {
        string message = customMessage ?? ex?.Message ?? "N/A";
        EnqueueLog(LogLevel.Error, message, ex, filePath, lineNumber, memberName);
    }

    public void LogCritical(
        Exception ex,
        string? customMessage = null,
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0,
        [CallerMemberName] string memberName = ""
    )
    {
        string message = customMessage ?? ex.Message;
        EnqueueLog(LogLevel.Critical, message, ex, filePath, lineNumber, memberName);
        // TODO: Send an email to the developer
    }

    private void EnqueueLog(
        LogLevel level,
        string message,
        Exception? ex,
        string filePath,
        int lineNumber,
        string memberName
    )
    {
        string assemblyName = InferAssemblyName();
        string serviceName = InferServiceName(filePath);

        var entry = new LogEntry
        {
            Level = level,
            Message = message,
            Exception = ex,
            SessionId = CorrelationContext.SessionId,
            OperationId = CorrelationContext.OperationId,
            CallerLineNumber = lineNumber,
            CallerMemberName = memberName,
            CallerServiceName = serviceName,
            CallerAssemblyName = assemblyName
        };

        AddEntry(entry);
    }

    private static string InferAssemblyName()
    {
        string assemblyName = Assembly.GetCallingAssembly().GetName().Name ?? "UnknownAssembly";

        if (assemblyName.Contains('.'))
        {
            assemblyName = assemblyName.Split('.')[^1];
        }
        
        return assemblyName;
    }

    private static string InferServiceName(string filePath) =>
        string.IsNullOrWhiteSpace(filePath)
            ? "UnknownService"
            : Path.GetFileName(filePath);

    protected override string GetFileName(DateTime now) =>
        Path.Combine(Directory, $"{now:yyyy-MM-dd}-Logs.csv");

    protected override string GetHeaderLine() =>
        "Timestamp,Level,Message,Exception,SessionId,OperationId,CallerLineNumber,CallerMemberName,CallerServiceName,CallerAssemblyName";

    protected override string FormatEntry(LogEntry e)
    {
        return string.Join(",",
            e.Timestamp.ToString("o"),
            e.Level,
            EscapeCsv(e.Message),
            EscapeCsv(e.Exception?.ToString() ?? ""),
            EscapeCsv(e.SessionId),
            EscapeCsv(e.OperationId),
            e.CallerLineNumber,
            EscapeCsv(e.CallerMemberName),
            EscapeCsv(e.CallerServiceName),
            EscapeCsv(e.CallerAssemblyName)
        );
    }
}

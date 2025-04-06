using Contracts.SupportModules.Logging;
using Contracts.SupportModules.Logging.Enums;
using Serilog.Events;
using Serilog.Context;
using System.Runtime.CompilerServices;
using System.Reflection;
using Serilog;

namespace SupportModules.Logging;

public class SerilogLoggingHandler : ILoggingHandler
{
    private readonly ICorrelationContext _correlationContext;
    
    public SerilogLoggingHandler(ICorrelationContext correlationContext)
    {
        _correlationContext = correlationContext;
    }

    public void LogInformation(string message, [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string memberName = "")
    {
        string assemblyName = SanitizeAssemblyName(Assembly.GetCallingAssembly().GetName().Name ?? "UnknownAssembly");
        
        LogWithStaticLogger(LogLevel.Information, message, null, filePath, lineNumber, memberName, assemblyName);
    }

    public void LogWarning(string message, [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string memberName = "")
    {
        string assemblyName = SanitizeAssemblyName(Assembly.GetCallingAssembly().GetName().Name ?? "UnknownAssembly");
        
        LogWithStaticLogger(LogLevel.Warning, message, null, filePath, lineNumber, memberName, assemblyName);
    }

    public void LogDebug(string message, [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string memberName = "")
    {
        string assemblyName = SanitizeAssemblyName(Assembly.GetCallingAssembly().GetName().Name ?? "UnknownAssembly");
        LogWithStaticLogger(LogLevel.Debug, message, null, filePath, lineNumber, memberName, assemblyName);
    }

    public void LogError(Exception? ex, string? customMessage = null, [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string memberName = "")
    {
        string message = customMessage ?? ex?.Message ?? "An error occurred";
        string assemblyName = SanitizeAssemblyName(Assembly.GetCallingAssembly().GetName().Name ?? "UnknownAssembly");
        
        LogWithStaticLogger(LogLevel.Error, message, ex, filePath, lineNumber, memberName, assemblyName);
    }

    public void LogCritical(Exception ex, string? customMessage = null, [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string memberName = "")
    {
        string message = customMessage ?? ex.Message;
        string assemblyName = SanitizeAssemblyName(Assembly.GetCallingAssembly().GetName().Name ?? "UnknownAssembly");
        
        LogWithStaticLogger(LogLevel.Critical, message, ex, filePath, lineNumber, memberName, assemblyName);
    }

    private void LogWithStaticLogger(LogLevel level, string message, Exception? ex, string filePath, int lineNumber, string memberName, string assemblyName)
    {
        LogEventLevel serilogLevel = MapLogLevel(level);
        
        string serviceName = InferServiceName(filePath);
        
        using (LogContext.PushProperty("SessionId", _correlationContext.SessionId))
        using (LogContext.PushProperty("OperationId", _correlationContext.OperationId))
        using (LogContext.PushProperty("CallerFilePath", filePath))
        using (LogContext.PushProperty("CallerLineNumber", lineNumber))
        using (LogContext.PushProperty("CallerMemberName", memberName))
        using (LogContext.PushProperty("CallerServiceName", serviceName))
        using (LogContext.PushProperty("CallerAssemblyName", assemblyName))
        {
            if (ex != null)
            {
                Log.Write(serilogLevel, ex, "{Message}", message);
            }
            else
            {
                Log.Write(serilogLevel, "{Message}", message); 
            }
        }
    }

    private static LogEventLevel MapLogLevel(LogLevel level) => level switch
    {
        LogLevel.Debug => LogEventLevel.Debug,
        LogLevel.Information => LogEventLevel.Information,
        LogLevel.Warning => LogEventLevel.Warning,
        LogLevel.Error => LogEventLevel.Error,
        LogLevel.Critical => LogEventLevel.Fatal,
        _ => LogEventLevel.Verbose
    };

     private static string SanitizeAssemblyName(string assemblyName)
    {
        if (assemblyName.Contains('.'))
        {
            assemblyName = assemblyName.Split('.')[^1];
        }
        return assemblyName;
    }

    private static string InferServiceName(string filePath) => string.IsNullOrWhiteSpace(filePath) 
        ? "UnknownService" 
        : Path.GetFileNameWithoutExtension(filePath);
}

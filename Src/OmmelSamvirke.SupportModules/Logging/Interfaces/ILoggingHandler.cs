using System.Runtime.CompilerServices;

namespace OmmelSamvirke.SupportModules.Logging.Interfaces;

public interface ILoggingHandler
{
    void LogInformation(
        string message,
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0,
        [CallerMemberName] string memberName = ""
    );

    void LogWarning(
        string message,
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0,
        [CallerMemberName] string memberName = ""
    );

    void LogDebug(
        string message,
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0,
        [CallerMemberName] string memberName = ""
    );

    void LogError(
        Exception? ex,
        string? customMessage = null,
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0,
        [CallerMemberName] string memberName = ""
    );

    void LogCritical(
        Exception ex,
        string? customMessage = null,
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0,
        [CallerMemberName] string memberName = ""
    );
}

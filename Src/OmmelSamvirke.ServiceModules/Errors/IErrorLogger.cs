using System.Runtime.CompilerServices;

namespace OmmelSamvirke.ServiceModules.Errors;

public interface IErrorLogger
{
    Task LogException(
        Exception exception,
        CancellationToken cancellationToken = default,
        [CallerMemberName] string callingMethodName = "",
        [CallerFilePath] string callingFilePath = "",
        [CallerLineNumber] int callerLineNumber = 0);
    
    Task LogValidationError(
        object invalidValue,
        string validationErrorMessage,
        CancellationToken cancellationToken = default,
        [CallerMemberName] string callingMethodName = "",
        [CallerFilePath] string callingFilePath = "",
        [CallerLineNumber] int callerLineNumber = 0);
}

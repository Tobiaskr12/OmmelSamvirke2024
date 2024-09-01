using OmmelSamvirke.ErrorHandling.Models;

namespace OmmelSamvirke.ErrorHandling.Interfaces;

public interface IErrorHandler
{
    Error CreateError(string message, int statusCode);
    Error CreateError(Exception exception);
}

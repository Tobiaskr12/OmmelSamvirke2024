using ErrorHandling.Models;

namespace ErrorHandling.Interfaces;

public interface IErrorHandler
{
    Error CreateError(string message, int statusCode);
    Error CreateError(Exception exception);
}

using ErrorHandling.Models;

namespace ErrorHandling.Interfaces;

public interface IErrorFactory
{
    Error CreateError(string message, int statusCode);
    Error CreateError(Exception exception);
}

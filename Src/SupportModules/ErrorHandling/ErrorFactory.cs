using ErrorHandling.Interfaces;
using ErrorHandling.Models;
using Microsoft.Extensions.Logging;

namespace ErrorHandling;

public class ErrorFactory : IErrorFactory
{
    private readonly ILogger _logger;
    
    public ErrorFactory(ILogger logger)
    {
        _logger = logger;
    }
    
    public Error CreateError(string message, int statusCode)
    {
        var error = new Error(message, statusCode);
        _logger.LogError("{errorMessage}", error.ToString());
        return error;
    }

    public Error CreateError(Exception exception)
    {
        string stackTrace = exception.StackTrace ?? Environment.StackTrace;

        var error = new Error(exception.Message, 500, stackTrace);
        _logger.LogError("{errorMessage}", error.ToString());
        return error;
    }
}

using Microsoft.Extensions.Logging;
using OmmelSamvirke.ErrorHandling.Interfaces;
using OmmelSamvirke.ErrorHandling.Models;

namespace OmmelSamvirke.ErrorHandling.Services;

public class ErrorHandler : IErrorHandler
{
    private readonly ILogger _logger;
    
    public ErrorHandler(ILogger logger)
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
        string? stackTrace = exception.StackTrace;

        var error = new Error(exception.Message, 500, stackTrace);
        _logger.LogError("{errorMessage}", error.ToString());
        return error;
    }
}
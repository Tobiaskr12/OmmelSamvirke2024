using System.Diagnostics;
using MediatR;
using Microsoft.Extensions.Logging;

namespace OmmelSamvirke.SupportModules.MediatorConfig.PipelineBehaviors;

public sealed class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : notnull
{
    private readonly ILogger _logger;
    
    public LoggingBehavior(ILogger logger)
    {
        _logger = logger; 
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        DateTime startTime = DateTime.UtcNow;
        var requestId = startTime.ToString("mm-ss.fffff");

        _logger.LogInformation("({requestId}) Started handling \"{requestName}\".", requestId, typeof(TRequest).Name); 
        
        var stopwatch = Stopwatch.StartNew();

        try
        {
            TResponse response = await next();
            stopwatch.Stop();

            _logger.LogInformation("({requestId}) Finished handling \"{requestName}\". Request took {executionTime}ms", requestId, typeof(TRequest).Name, stopwatch.Elapsed.TotalMilliseconds);
            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "(requestId{}) Error handling \"{requestName}\". Request failed after {executionTime}ms", requestId, typeof(TRequest).Name, stopwatch.Elapsed.TotalMilliseconds);

            throw;
        }
    }
}

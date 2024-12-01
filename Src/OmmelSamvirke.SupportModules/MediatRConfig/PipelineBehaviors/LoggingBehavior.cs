using System.Diagnostics;
using MediatR;
using Microsoft.Extensions.Logging;

namespace OmmelSamvirke.SupportModules.MediatRConfig.PipelineBehaviors;

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

        _logger.LogInformation($"({requestId}) Started handling \"{typeof(TRequest).Name}\"."); 
        
        var stopwatch = Stopwatch.StartNew();

        try
        {
            var response = await next();
            stopwatch.Stop();

            _logger.LogInformation($"({requestId}) Finished handling \"{typeof(TResponse).Name}\". Request took {stopwatch.Elapsed.TotalMilliseconds}ms");
            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, $"({requestId}) Error handling \"{typeof(TRequest).Name}\". Request failed after {stopwatch.Elapsed.TotalMilliseconds}ms");

            throw;
        }
    }
}

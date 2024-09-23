using MediatR;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace MediatrConfig.PipelineBehaviors;

public sealed class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : notnull
{
    private readonly ILogger _logger;


    public LoggingBehavior(ILogger logger)
    {
        _logger = logger; 
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var startTime = DateTime.UtcNow;
        var requestID = startTime.ToString("mm-ss.fffff");

        _logger.LogInformation($"({requestID}) Started handling \"{typeof(TRequest).Name}\"."); 
        
        var stopwatch = Stopwatch.StartNew();

        try
        {
            var response = await next();
            stopwatch.Stop();

            _logger.LogInformation($"({requestID}) Finished handling \"{typeof(TResponse).Name}\". Request took {stopwatch.Elapsed.TotalMilliseconds}ms");
            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, $"({requestID}) Error handling \"{typeof(TRequest).Name}\". Request failed after {stopwatch.Elapsed.TotalMilliseconds}ms");

            throw;
        }
    }
}

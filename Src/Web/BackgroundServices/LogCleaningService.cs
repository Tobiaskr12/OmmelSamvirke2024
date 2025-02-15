using Contracts.SupportModules.Logging;
using System.Diagnostics;

namespace Web.BackgroundServices;

public class LogCleaningService : BackgroundService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogRepository _logRepository;
    private readonly ITraceRepository _traceRepository;
    private readonly Stopwatch _stopwatch;

    public LogCleaningService(IServiceScopeFactory serviceScopeFactory, ILogRepository logRepository, ITraceRepository traceRepository)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _logRepository = logRepository;
        _traceRepository = traceRepository;

        _stopwatch = new Stopwatch();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                _stopwatch.Restart();
                DeleteOldData();
            }
            catch (Exception ex)
            {
                try
                {
                    _stopwatch.Stop();

                    using var scope = _serviceScopeFactory.CreateScope();
                    var loggingHandler = scope.ServiceProvider.GetRequiredService<ILoggingHandler>();
                    var traceHandler = scope.ServiceProvider.GetRequiredService<ITraceHandler>();

                    loggingHandler.LogError(ex, "Logs and Traces were not deleted");
                    traceHandler.Trace("Scheduled Background Service", isSuccess: false, _stopwatch.ElapsedMilliseconds, nameof(LogCleaningService));
                } catch (Exception)
                {
                    // If logging also fails, that sucks, but the service must survive!
                }
            }

            var now = DateTime.UtcNow;
            var nextRun = now.Date.AddDays(1).AddHours(4); // 4AM UTC
            var delay = nextRun - now;

            await Task.Delay(delay, stoppingToken);
        }
    }

    private void DeleteOldData()
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var traceHandler = scope.ServiceProvider.GetRequiredService<ITraceHandler>();

        _logRepository.DeleteOldLogs();
        _traceRepository.DeleteOldTraces();

        _stopwatch.Stop();
        traceHandler.Trace("Scheduled Background Service", isSuccess: true, _stopwatch.ElapsedMilliseconds, nameof(LogCleaningService));
    }
}

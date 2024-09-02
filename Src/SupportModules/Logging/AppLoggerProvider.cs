using Microsoft.Extensions.Logging;

namespace Logging;

public class AppLoggerProvider : ILoggerProvider
{
    private readonly ILogger _logger;

    public AppLoggerProvider(ILogger logger)
    {
        _logger = logger;
    }

    public ILogger CreateLogger(string categoryName)
    {
        return _logger;
    }

    public void Dispose()
    {
        // Nothing to dispose
        GC.SuppressFinalize(this);
    }
}

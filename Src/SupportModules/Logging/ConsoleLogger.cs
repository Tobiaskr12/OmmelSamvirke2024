using Serilog;
using Serilog.Core;
using Serilog.Extensions.Logging;

namespace Logging;

public static class ConsoleLogger
{
    public static Microsoft.Extensions.Logging.ILogger CreateLogger()
    {
        Logger logger = new LoggerConfiguration()
            .WriteTo.Console()
            .CreateLogger();

        return new SerilogLoggerFactory(logger).CreateLogger("default");
    } 
}

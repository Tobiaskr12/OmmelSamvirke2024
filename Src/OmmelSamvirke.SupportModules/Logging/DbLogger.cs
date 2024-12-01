using Serilog;
using Serilog.Core;
using Serilog.Extensions.Logging;
using Serilog.Sinks.MSSqlServer;

namespace OmmelSamvirke.SupportModules.Logging;

public static class DbLogger
{
    public static Microsoft.Extensions.Logging.ILogger CreateLogger(string connectionString)
    {
        Logger logger = new LoggerConfiguration()
            .WriteTo.MSSqlServer(
                connectionString: connectionString,
                sinkOptions: new MSSqlServerSinkOptions()
                {
                    TableName = "Logs",
                    AutoCreateSqlDatabase = false,
                    AutoCreateSqlTable = true
                },
                columnOptions: new ColumnOptions()
            )
            .CreateLogger();

        return new SerilogLoggerFactory(logger).CreateLogger("default");
    }
}

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace OmmelSamvirke.SupportModules.Logging;

public static class AppLoggerFactory
{
    public static ILogger CreateLogger(IConfigurationRoot configRoot)
    {
        bool isProduction = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Production";

        // Case: Test and Development
        if (!isProduction) return ConsoleLogger.CreateLogger();
        
        // Case: Production
        var connectionString = configRoot.GetValue<string>("SqlServerConnectionString");

        if (string.IsNullOrEmpty(connectionString))
            throw new ArgumentException("A valid connection string to the database could not be found");

        return DbLogger.CreateLogger(connectionString);
    }
}

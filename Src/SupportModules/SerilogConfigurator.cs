using Contracts.SupportModules.SecretsManager;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace SupportModules;

public static class SerilogConfigurator
{
    /// <summary>
    /// Configures the static Serilog logger. Call this WebApplication.CreateBuilder(). Ideally as early as possible.
    /// </summary>
    /// <param name="configuration">An IConfiguration instance containing Serilog settings and the AppInsights connection string.</param>
    /// <param name="environment">The current execution environment.</param>
    public static void ConfigureStaticLogger(IConfiguration configuration, ExecutionEnvironment environment)
    {
        string? appInsightsConnectionString = configuration["ApplicationInsightsConnectionString"];

        if (string.IsNullOrEmpty(appInsightsConnectionString))
        {
            throw new Exception("ApplicationInsightsConnectionString not found during logger setup.");
        }

        LoggerConfiguration loggerConfiguration = 
            new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .Enrich.FromLogContext();

        try
        {
            var telemetryConfiguration = TelemetryConfiguration.CreateDefault();
            telemetryConfiguration.ConnectionString = appInsightsConnectionString;
            loggerConfiguration.WriteTo.ApplicationInsights(telemetryConfiguration, TelemetryConverter.Traces);
        }
        catch (Exception ex)
        {
             throw new Exception($"Failed to configure Application Insights sink: {ex.Message}");
        }

        if (environment == ExecutionEnvironment.Development)
        {
            loggerConfiguration.WriteTo.Console();
        }

        Log.Logger = loggerConfiguration.CreateLogger();
    }

    /// <summary>
    /// Integrates the globally configured Serilog logger with the ASP.NET Core host.
    /// Call this on builder.Host in Program.cs.
    /// </summary>
    /// <param name="hostBuilder">The host builder.</param>
    /// <returns>The host builder for chaining.</returns>
    public static IHostBuilder UseSharedSerilogConfiguration(this IHostBuilder hostBuilder)
    {
        hostBuilder.UseSerilog();
        return hostBuilder;
    }
}

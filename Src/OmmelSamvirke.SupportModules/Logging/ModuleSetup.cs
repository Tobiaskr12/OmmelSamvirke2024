using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using OmmelSamvirke.Interfaces.Emails;
using OmmelSamvirke.SupportModules.Logging.Interfaces;
using OmmelSamvirke.SupportModules.SecretsManager;

namespace OmmelSamvirke.SupportModules.Logging;

public static class ModuleSetup
{
    public static IServiceCollection InitializeLoggingModule(
        this IServiceCollection services,
        ExecutionEnvironment executionEnvironment)
    {
        // Register services
        services.AddSingleton<ILoggingLocationInfo>(_ => new LoggingLocationInfo(executionEnvironment));
        services.AddSingleton<ILogRepository, LogRepository>();
        services.AddSingleton<ITraceRepository, TraceRepository>();
        
        services.AddScoped<ICorrelationContext, CorrelationContext>();
        services.AddScoped<ILoggingHandler, CsvLogWriter>();
        services.AddScoped<ITraceHandler, CsvTraceWriter>();

        // Inject IEmailTemplateEngine as a factory to break circular dependency
        services.AddTransient<Func<IEmailTemplateEngine>>(sp => () => sp.GetRequiredService<IEmailTemplateEngine>());
        
        return services;
    }
}

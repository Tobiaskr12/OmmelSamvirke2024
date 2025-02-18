using Contracts.ServiceModules.Emails.EmailTemplateEngine;
using Contracts.SupportModules.Logging;
using Contracts.SupportModules.Logging.Util;
using Contracts.SupportModules.SecretsManager;
using Microsoft.Extensions.DependencyInjection;
using SupportModules.Logging.Interfaces;
using SupportModules.Logging.Util;

namespace SupportModules.Logging;

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
        services.AddScoped<IShortIdGenerator, ShortIdGenerator>();
        services.AddScoped<ILoggingHandler, CsvLogWriter>();
        services.AddScoped<ITraceHandler, CsvTraceWriter>();

        // Inject IEmailTemplateEngine as a factory to break circular dependency
        services.AddTransient<Func<IEmailTemplateEngine>>(sp => () => sp.GetRequiredService<IEmailTemplateEngine>());
        
        return services;
    }
}

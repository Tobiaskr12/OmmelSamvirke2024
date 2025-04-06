using Contracts.SupportModules.Logging;
using Contracts.SupportModules.Logging.Util;
using Contracts.SupportModules.SecretsManager;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using SupportModules.Logging.Util;

namespace SupportModules.Logging;

public static class ModuleSetup
{
    public static IServiceCollection InitializeLoggingModule(
        this IServiceCollection services, 
        IConfigurationRoot configuration, 
        ExecutionEnvironment executionEnvironment)
    {
        
        services.AddScoped<ICorrelationContext, CorrelationContext>();
        services.AddScoped<IShortIdGenerator, ShortIdGenerator>();
        services.AddScoped<ILoggingHandler, SerilogLoggingHandler>();
        services.AddScoped<ITraceHandler, SerilogTraceHandler>();
        
        return services;
    }
}

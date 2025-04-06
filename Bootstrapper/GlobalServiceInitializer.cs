using Contracts.SupportModules.SecretsManager;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using DomainModules;
using DataAccess;
using Infrastructure;
using ServiceModules;
using SupportModules.Logging;

namespace Bootstrapper;

public static class GlobalServiceInitializer
{
    public static IServiceCollection InitializeAllServices(
        this IServiceCollection services,
        IConfigurationRoot configuration,
        ExecutionEnvironment executionEnvironment)
    {
        services
            .InitializeLoggingModule(configuration, executionEnvironment)
            .InitializeDataAccessModule(configuration).Result
            .InitializeInfrastructureModule()
            .InitializeDomainModule()
            .InitializeServicesModule();
        
        return services;
    }
}

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OmmelSamvirke.DataAccess;
using OmmelSamvirke.DomainModules;
using OmmelSamvirke.Infrastructure;
using OmmelSamvirke.SupportModules.Logging;
using OmmelSamvirke.SupportModules.SecretsManager;

namespace OmmelSamvirke.ServiceModules;

public static class GlobalServiceInitializer
{
    public static IServiceCollection InitializeAllServices(
        this IServiceCollection services,
        IConfigurationRoot configuration,
        ExecutionEnvironment executionEnvironment)
    {

        services
            .InitializeLoggingModule(executionEnvironment)
            .InitializeDataAccessModule(configuration).Result
            .InitializeInfrastructureModule()
            .InitializeDomainModule()
            .InitializeServicesModule();
        
        return services;
    }
}

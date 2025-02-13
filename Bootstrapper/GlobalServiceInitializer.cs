using Contracts.SupportModules.SecretsManager;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OmmelSamvirke.DomainModules;
using OmmelSamvirke.DataAccess;
using OmmelSamvirke.Infrastructure;
using OmmelSamvirke.ServiceModules;
using OmmelSamvirke.SupportModules.Logging;

namespace OmmelSamvirke.Bootstrapper;

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

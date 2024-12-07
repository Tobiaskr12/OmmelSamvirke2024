using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using OmmelSamvirke.DomainModules;
using OmmelSamvirke.ServiceModules;
using OmmelSamvirke.SupportModules.Logging;
using OmmelSamvirke.SupportModules.MediatorConfig;
using OmmelSamvirke.SupportModules.SecretsManager;

namespace OmmelSamvirke.SupportModules.Tests.MediatorConfig;

public static class ServiceRegistry
{
    private static ServiceProvider? _serviceProvider;

    public static T GetService<T>()
    {
        if (_serviceProvider == null)
        {
            RegisterServices();
        }

        if (_serviceProvider is null)
        {
            throw new Exception("Could not success fully initialized the service registry.");
        }

        var foundService = _serviceProvider.GetService<T>();

        if (foundService == null)
        {
            throw new Exception($"Could not find service of type {typeof(T).Name}");
        }
        
        return foundService;
    }
    
    private static void RegisterServices()
    {
        var services = new ServiceCollection();
        
        var mockLogger = Substitute.For<ILogger>();
        services.AddSingleton(mockLogger);
        
        services.AddValidatorsFromAssembly(typeof(ServiceRegistry).Assembly);
        MediatrConfigSetup.Setup(services, typeof(ServiceRegistry).Assembly);
        
        _serviceProvider = services.BuildServiceProvider();
    }
}

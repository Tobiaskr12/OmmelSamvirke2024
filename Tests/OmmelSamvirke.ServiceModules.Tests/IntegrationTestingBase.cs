using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OmmelSamvirke.DataAccess;
using OmmelSamvirke.DomainModules;
using OmmelSamvirke.Infrastructure;
using OmmelSamvirke.SupportModules.Logging;
using OmmelSamvirke.SupportModules.SecretsManager;

namespace OmmelSamvirke.ServiceModules.Tests;

public abstract class IntegrationTestingBase
{
    protected IMediator Mediator;
    protected IConfigurationRoot Configuration;
    
    [SetUp]
    public virtual void Setup()
    {
        var services = new ServiceCollection();
        Configuration = new ConfigurationBuilder()
            .AddKeyVaultSecrets(ExecutionEnvironment.Testing)
            .Build();
        
        services.AddSingleton<IConfiguration>(Configuration);
        services.AddSingleton(Configuration); // Adds as IConfigurationRoot
        
        ILogger appLogger = AppLoggerFactory.CreateLogger(Configuration);
        services.AddSingleton(appLogger);
        services.AddLogging(loggingBuilder => loggingBuilder.AddProvider(new AppLoggerProvider(appLogger)));
        
        services
            .InitializeDataAccessModule(Configuration).Result
            .InitializeInfrastructureModule()
            .InitializeDomainModule()
            .InitializeServicesModule();
        
        ServiceProvider serviceProvider = services.BuildServiceProvider();
        
        Mediator = serviceProvider.GetService<IMediator>() ?? throw new Exception("Mediator service not found");
    }
}
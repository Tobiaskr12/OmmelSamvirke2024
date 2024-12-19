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

public class IntegrationTestingHelper
{
    private IMediator? _mediator;
    private IConfigurationRoot? _configuration;
    private ServiceProvider? _serviceProvider;
    
    public IMediator Mediator
    {
        get
        {
            if (_mediator is null)
            {
                Setup();
            }
            
            return _mediator!;
        }
        private set => _mediator = value;
    }

    public IConfigurationRoot Configuration
    {
        get
        {
            if (_configuration is null)
            {
                Setup();
            }
            
            return _configuration!;
        }
        private set => _configuration = value;
    }

    public ServiceProvider ServiceProvider
    {
        get
        {
            if (_serviceProvider is null)
            {
                Setup();
            }
            
            return _serviceProvider!;
        }
        private set => _serviceProvider = value;
    }

    private void Setup()
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
        
        ServiceProvider = services.BuildServiceProvider();
        
        Mediator = ServiceProvider.GetService<IMediator>() ?? throw new Exception("Mediator service not found");
    }
}

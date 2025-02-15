using Bootstrapper;
using Contracts.SupportModules.SecretsManager;
using DataAccess.Base;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SupportModules.SecretsManager;

namespace TestDatabaseFixtures;

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
    
    public T GetService<T>() where T : class => ServiceProvider.GetService<T>() ?? throw new Exception($"Service of type {typeof(T).Name} not found");
    
    public async Task ResetDatabase()
    {
        var dbContext = ServiceProvider.GetService<OmmelSamvirkeDbContext>();
        if (dbContext == null) throw new ArgumentNullException(nameof(dbContext));
        
        if (_configuration?.GetSection("ExecutionEnvironment").Value == "Test")
        {
            await dbContext.Database.EnsureDeletedAsync();
            await dbContext.Database.MigrateAsync();
            dbContext.ChangeTracker.Clear();
        }
    }

    private ServiceProvider ServiceProvider
    {
        get
        {
            if (_serviceProvider is null)
            {
                Setup();
            }
            
            return _serviceProvider!;
        }
        set => _serviceProvider = value;
    }

    private void Setup()
    {
        var services = new ServiceCollection();
        Configuration = new ConfigurationBuilder()
            .AddKeyVaultSecrets(ExecutionEnvironment.Testing)
            .Build();
        
        services.AddSingleton<IConfiguration>(Configuration);
        services.AddSingleton(Configuration); // Adds as IConfigurationRoot
        
        services.InitializeAllServices(Configuration, ExecutionEnvironment.Testing);
        
        ServiceProvider = services.BuildServiceProvider();
        
        Mediator = ServiceProvider.GetService<IMediator>() ?? throw new Exception("Mediator service not found");
    }
}

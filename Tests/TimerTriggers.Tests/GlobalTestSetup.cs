using Bootstrapper;
using Contracts.SupportModules.SecretsManager;
using DataAccess.Base;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SupportModules.SecretsManager;

namespace TimerTriggers.Tests;

[SetUpFixture]
public class GlobalTestSetup
{
    private static IConfigurationRoot _configuration = null!;
    private static ServiceProvider _serviceProvider = null!;
    
    public static T GetService<T>() where T : class => _serviceProvider.GetService<T>() 
        ?? throw new Exception($"Service of type {typeof(T).Name} not found");
    
    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        var services = new ServiceCollection();
        _configuration = new ConfigurationBuilder()
            .AddKeyVaultSecrets(ExecutionEnvironment.Testing)
            .Build();
        
        services.AddSingleton<IConfiguration>(_configuration);
        services.AddSingleton(_configuration); // Adds as IConfigurationRoot
        
        services.InitializeAllServices(_configuration, ExecutionEnvironment.Testing);
        
        _serviceProvider = services.BuildServiceProvider();
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        _serviceProvider.Dispose();
    }
    
    public static async Task ResetDatabase()
    {
        var dbContext = _serviceProvider.GetService<OmmelSamvirkeDbContext>();
        if (dbContext == null) throw new ArgumentNullException(nameof(dbContext));
        
        if (_configuration.GetSection("ExecutionEnvironment").Value == "Test")
        {
            await dbContext.Database.EnsureDeletedAsync();
            await dbContext.Database.MigrateAsync();
            dbContext.ChangeTracker.Clear();
        }
    }
}

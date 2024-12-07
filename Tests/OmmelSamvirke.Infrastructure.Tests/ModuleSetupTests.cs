using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using NSubstitute;
using OmmelSamvirke.Infrastructure.Emails;
using OmmelSamvirke.SupportModules.SecretsManager;

namespace OmmelSamvirke.Infrastructure.Tests;

[TestFixture, Category("IntegrationTests")]
public class ModuleSetupTests
{
    private IServiceCollection _services;

    [SetUp]
    public void Setup()
    {
        var mockLogger = Substitute.For<ILogger>();
        var mockLoggerFactory = Substitute.For<ILoggerFactory>();
        
        IConfigurationBuilder configurationBuilder = new ConfigurationBuilder().AddKeyVaultSecrets(ExecutionEnvironment.Testing);

        _services = new ServiceCollection();
        _services.AddSingleton<IConfiguration>(configurationBuilder.Build());
        _services.AddSingleton(mockLogger);
        _services.AddSingleton(mockLoggerFactory);

        _services.InitializeInfrastructureModule();
    }

    [Test]
    public void InitializeInfrastructureModule_Should_RegisterLocalizationOptions()
    {
        ServiceProvider provider = _services.BuildServiceProvider();
        
        var localizationFactory = provider.GetService<IStringLocalizerFactory>();
        Assert.That(localizationFactory, Is.Not.Null);
    }

    [Test]
    public void InitializeInfrastructureModule_Should_RegisterExternalEmailServiceWrapper()
    {
        ServiceProvider provider = _services.BuildServiceProvider();
        
        var emailService = provider.GetService<IExternalEmailServiceWrapper>();
        Assert.That(emailService, Is.Not.Null);
        Assert.That(emailService, Is.InstanceOf<AzureEmailServiceWrapper>());
    }
}
using Contracts.Infrastructure.Emails;
using Contracts.SupportModules.Logging;
using Contracts.SupportModules.SecretsManager;
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
        IConfigurationBuilder configurationBuilder = new ConfigurationBuilder().AddKeyVaultSecrets(ExecutionEnvironment.Testing);

        _services = new ServiceCollection();
        _services.AddSingleton<IConfiguration>(configurationBuilder.Build());

        _services.InitializeInfrastructureModule();
    }

    [Test]
    public void InitializeInfrastructureModule_Should_RegisterLocalizationOptions()
    {
        _services.AddSingleton(Substitute.For<ILoggerFactory>());
        ServiceProvider provider = _services.BuildServiceProvider();
        
        var localizationFactory = provider.GetService<IStringLocalizerFactory>();
        Assert.That(localizationFactory, Is.Not.Null);
    }

    [Test]
    public void InitializeInfrastructureModule_Should_RegisterExternalEmailServiceWrapper()
    {
        _services.AddSingleton(Substitute.For<ILoggingHandler>());
        ServiceProvider provider = _services.BuildServiceProvider();
        
        var emailService = provider.GetService<IExternalEmailServiceWrapper>();
        Assert.That(emailService, Is.Not.Null);
        Assert.That(emailService, Is.InstanceOf<AzureEmailServiceWrapper>());
    }
}

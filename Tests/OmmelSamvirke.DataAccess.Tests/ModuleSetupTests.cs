using Contracts.DataAccess.Base;
using Contracts.DataAccess.Emails;
using Contracts.SupportModules.SecretsManager;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OmmelSamvirke.SupportModules.SecretsManager;
using OmmelSamvirke.DataAccess.Base;
using OmmelSamvirke.DataAccess.Emails.Repositories;
using OmmelSamvirke.DomainModules.Emails.Entities;
using OmmelSamvirke.Bootstrapper;

namespace OmmelSamvirke.DataAccess.Tests;

[TestFixture, Category("IntegrationTests")]
public class ModuleSetupTests
{
    private IServiceCollection _services;

    [SetUp]
    public void Setup()
    {
        _services = new ServiceCollection();

        IConfigurationBuilder configurationBuilder = new ConfigurationBuilder().AddKeyVaultSecrets(ExecutionEnvironment.Testing);
        _services.InitializeAllServices(configurationBuilder.Build(), ExecutionEnvironment.Testing);
    }

    [Test]
    public void InitializeDataAccessModule_Should_RegisterGenericRepository()
    {
        ServiceProvider provider = _services.BuildServiceProvider();
        
        var genericRepo = provider.GetService<IRepository<Email>>();
        Assert.That(genericRepo, Is.Not.Null);
        Assert.That(genericRepo, Is.InstanceOf<GenericRepository<Email>>());
    }

    [Test]
    public void InitializeDataAccessModule_Should_RegisterEmailSendingRepository()
    {
        ServiceProvider provider = _services.BuildServiceProvider();
        
        var emailSendingRepo = provider.GetService<IEmailSendingRepository>();
        Assert.That(emailSendingRepo, Is.Not.Null, "IEmailSendingRepository should be registered.");
        Assert.That(emailSendingRepo, Is.InstanceOf<EmailSendingRepository>());
    }
}

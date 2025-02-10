using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using OmmelSamvirke.DomainModules.Emails.Validators;

namespace OmmelSamvirke.DomainModules.Tests;

[TestFixture, Category("IntegrationTests")]
public class ModuleSetupTests
{
    private IServiceCollection _services;

    [SetUp]
    public void Setup()
    {
        _services = new ServiceCollection();
        _services.InitializeDomainModule();
    }

    [Test]
    public void InitializeDomainModule_Should_RegisterLocalizationOptions()
    {
        using ServiceProvider provider = _services.BuildServiceProvider();
        var localizationFactory = provider.GetService<IStringLocalizerFactory>();
        Assert.That(localizationFactory, Is.Not.Null);
    }

    [Test]
    public void InitializeDomainModule_Should_RegisterValidatorsFromAssembly()
    {
        using ServiceProvider provider = _services.BuildServiceProvider();
        var validator = provider.GetService<EmailValidator>();
        Assert.That(validator, Is.Not.Null);
    }
}

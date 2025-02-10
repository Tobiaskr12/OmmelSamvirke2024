using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using OmmelSamvirke.DomainModules;
using OmmelSamvirke.ServiceModules.Emails.Sending.Commands;

namespace OmmelSamvirke.ServiceModules.Tests;

[TestFixture, Category("IntegrationTests")]
public class ModuleSetupTests
{
    private IServiceCollection _services;

    [SetUp]
    public void Setup()
    {
        _services = new ServiceCollection();
        
        _services.InitializeDomainModule(); // Load in validator dependencies
        _services.InitializeServicesModule();
    }

    [Test]
    public void InitializeServicesModule_Should_RegisterLocalizationOptions()
    {
        ServiceProvider provider = _services.BuildServiceProvider();
        
        var localizationOptions = provider.GetService<Microsoft.Extensions.Localization.IStringLocalizerFactory>();
        Assert.That(localizationOptions, Is.Not.Null);
    }

    [Test]
    public void InitializeServicesModule_Should_RegisterValidatorsFromAssembly()
    {
        ServiceProvider provider = _services.BuildServiceProvider();
        
        var validator = provider.GetService<IValidator<SendEmailCommand>>();
        Assert.That(validator, Is.Not.Null);
    }

    [Test]
    public void InitializeServicesModule_Should_RegisterMediatorServices()
    {
        ServiceProvider provider = _services.BuildServiceProvider();
        
        var mediator = provider.GetService<MediatR.IMediator>();
        Assert.That(mediator, Is.Not.Null);
    }
}

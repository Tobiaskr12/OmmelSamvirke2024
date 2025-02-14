using Contracts.ServiceModules.Emails;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using OmmelSamvirke.ServiceModules.Emails.EmailTemplateEngine;
using OmmelSamvirke.ServiceModules.MediatorConfig.PipelineBehaviors;

namespace OmmelSamvirke.ServiceModules;

public static class ModuleSetup
{
    public static IServiceCollection InitializeServicesModule(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddLocalization(options => options.ResourcesPath = "ErrorMessages");
        serviceCollection.AddValidatorsFromAssembly(typeof(ModuleSetup).Assembly);
        serviceCollection.AddScoped<IEmailTemplateEngine, TemplateEngine>();

        serviceCollection.AddMediatR(config =>
        {
            config.RegisterServicesFromAssembly(typeof(ModuleSetup).Assembly);
            config.AddOpenBehavior(typeof(ResponseHandlingBehavior<,>));
            config.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });

        return serviceCollection;
    }
}

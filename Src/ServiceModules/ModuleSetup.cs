using Contracts.Emails.EmailTemplateEngine;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using ServiceModules.Emails.EmailTemplateEngine;
using ServiceModules.MediatorConfig.PipelineBehaviors;

namespace ServiceModules;

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

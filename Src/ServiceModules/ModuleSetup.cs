using Contracts.ServiceModules.AlbumImages;
using Contracts.ServiceModules.Emails.EmailTemplateEngine;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using ServiceModules.Emails.EmailTemplateEngine;
using ServiceModules.Events.IcsFeed;
using ServiceModules.ImageAlbums.Services;
using ServiceModules.MediatorConfig.PipelineBehaviors;

namespace ServiceModules;

public static class ModuleSetup
{
    public static IServiceCollection InitializeServicesModule(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddLocalization(options => options.ResourcesPath = "ErrorMessages");
        serviceCollection.AddValidatorsFromAssembly(typeof(ModuleSetup).Assembly);
        serviceCollection.AddScoped<IEmailTemplateEngine, TemplateEngine>();
        
        serviceCollection.AddScoped<IcsFeedService>();
        serviceCollection.AddSingleton<IImageProcessingService, ImageProcessingService>();

        serviceCollection.AddMediatR(config =>
        {
            config.RegisterServicesFromAssembly(typeof(ModuleSetup).Assembly);
            config.AddOpenBehavior(typeof(ResponseHandlingBehavior<,>));
            config.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });

        return serviceCollection;
    }
}

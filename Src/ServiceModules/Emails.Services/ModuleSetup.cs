using Microsoft.Extensions.DependencyInjection;
using MediatrConfig.PipelineBehaviors;
using FluentValidation;

namespace Emails.Services;

public static class ModuleSetup
{
    public static IServiceCollection InitializeEmailServicesModule(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddLocalization(options => options.ResourcesPath = "ErrorMessages");
        serviceCollection.AddValidatorsFromAssembly(typeof(ModuleSetup).Assembly);
        
        serviceCollection.AddMediatR(config =>
        {
            config.RegisterServicesFromAssembly(typeof(ModuleSetup).Assembly);
            config.AddOpenBehavior(typeof(LoggingBehavior<,>));
            config.AddOpenBehavior(typeof(ValidationBehavior<,>));
            config.AddOpenBehavior(typeof(ResultExceptionThrowingBehavior<,>));
        });

        return serviceCollection;
    }
}

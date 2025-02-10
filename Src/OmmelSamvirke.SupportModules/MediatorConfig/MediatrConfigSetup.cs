using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using OmmelSamvirke.SupportModules.MediatorConfig.PipelineBehaviors;

namespace OmmelSamvirke.SupportModules.MediatorConfig;

public static class MediatrConfigSetup
{
    public static void Setup(IServiceCollection serviceCollection, Assembly callingAssembly)
    {
        serviceCollection.AddMediatR(config =>
        {
            config.RegisterServicesFromAssembly(callingAssembly);
            // config.AddOpenBehavior(typeof(LoggingBehavior<,>));
            config.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });
    }
}

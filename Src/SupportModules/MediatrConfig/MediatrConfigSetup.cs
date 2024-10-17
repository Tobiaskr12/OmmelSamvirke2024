using System.Reflection;
using MediatrConfig.PipelineBehaviors;
using Microsoft.Extensions.DependencyInjection;

namespace MediatrConfig;

public static class MediatrConfigSetup
{
    public static void Setup(IServiceCollection serviceCollection, Assembly callingAssembly)
    {
        serviceCollection.AddMediatR(config =>
        {
            config.RegisterServicesFromAssembly(callingAssembly);
            config.AddOpenBehavior(typeof(LoggingBehavior<,>));
            config.AddOpenBehavior(typeof(ValidationBehavior<,>));
            config.AddOpenBehavior(typeof(ResultExceptionThrowingBehavior<,>));
        });
    }
}

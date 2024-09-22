using Microsoft.Extensions.DependencyInjection;

namespace EmailWrapper;

public static class ModuleSetup
{
    
    public static IServiceCollection InitializeEmailWrapperModule(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddLocalization(options => options.ResourcesPath = "ErrorHandling/ErrorMessages");

        return serviceCollection;
    }
}

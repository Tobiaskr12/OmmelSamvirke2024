using Microsoft.Extensions.DependencyInjection;
using FluentValidation;
using MediatrConfig;

namespace Emails.Services;

public static class ModuleSetup
{
    public static IServiceCollection InitializeEmailServicesModule(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddLocalization(options => options.ResourcesPath = "ErrorMessages");
        serviceCollection.AddValidatorsFromAssembly(typeof(ModuleSetup).Assembly);
        
        MediatrConfigSetup.Setup(serviceCollection, typeof(ModuleSetup).Assembly);

        return serviceCollection;
    }
}

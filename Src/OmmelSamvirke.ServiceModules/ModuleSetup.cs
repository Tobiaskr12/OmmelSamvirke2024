using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using OmmelSamvirke.SupportModules.MediatRConfig;

namespace OmmelSamvirke.ServiceModules;

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

using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using OmmelSamvirke.ServiceModules.Errors;
using OmmelSamvirke.SupportModules.MediatorConfig;

namespace OmmelSamvirke.ServiceModules;

public static class ModuleSetup
{
    public static IServiceCollection InitializeServicesModule(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddLocalization(options => options.ResourcesPath = "ErrorMessages");
        serviceCollection.AddValidatorsFromAssembly(typeof(ModuleSetup).Assembly);
        serviceCollection.AddSingleton<IErrorLogger, ErrorLogger>();
        
        MediatrConfigSetup.Setup(serviceCollection, typeof(ModuleSetup).Assembly);

        return serviceCollection;
    }
}

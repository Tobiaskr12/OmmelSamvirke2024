using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace OmmelSamvirke.DomainModules;

public static class ModuleSetup
{
    public static IServiceCollection InitializeDomainModule(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddLocalization(options => options.ResourcesPath = "ErrorMessages");
        serviceCollection.AddValidatorsFromAssembly(typeof(ModuleSetup).Assembly);

        return serviceCollection;
    }
}

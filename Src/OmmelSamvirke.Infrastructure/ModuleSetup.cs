using Contracts.Infrastructure.Emails;
using Microsoft.Extensions.DependencyInjection;
using OmmelSamvirke.Infrastructure.Emails;

namespace OmmelSamvirke.Infrastructure;

public static class ModuleSetup
{
    public static IServiceCollection InitializeInfrastructureModule(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddLocalization(options => options.ResourcesPath = "ErrorMessages");
        serviceCollection.AddScoped<IExternalEmailServiceWrapper, AzureEmailServiceWrapper>();

        return serviceCollection;
    }
}
using Contracts.Infrastructure.Emails;
using Microsoft.Extensions.DependencyInjection;
using Infrastructure.Emails;

namespace Infrastructure;

public static class ModuleSetup
{
    public static IServiceCollection InitializeInfrastructureModule(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddLocalization(options => options.ResourcesPath = "ErrorMessages");
        serviceCollection.AddScoped<IExternalEmailServiceWrapper, AzureEmailServiceWrapper>();

        return serviceCollection;
    }
}
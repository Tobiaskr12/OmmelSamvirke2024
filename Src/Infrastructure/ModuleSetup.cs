using Contracts.Infrastructure.BlobStorage;
using Contracts.Infrastructure.Emails;
using Infrastructure.BlobStorage;
using Microsoft.Extensions.DependencyInjection;
using Infrastructure.Emails;

namespace Infrastructure;

public static class ModuleSetup
{
    public static IServiceCollection InitializeInfrastructureModule(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddLocalization(options => options.ResourcesPath = "ErrorMessages");
        serviceCollection.AddScoped<IExternalEmailServiceWrapper, AzureEmailServiceWrapper>();
        serviceCollection.AddScoped<IBlobStorageService, AzureBlobStorageService>();

        return serviceCollection;
    }
}

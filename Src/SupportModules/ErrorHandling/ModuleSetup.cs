using ErrorHandling.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace ErrorHandling;

public static class ModuleSetup
{
    public static void InitializeErrorHandlingModule(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddSingleton<IErrorFactory, ErrorFactory>();
    }
}

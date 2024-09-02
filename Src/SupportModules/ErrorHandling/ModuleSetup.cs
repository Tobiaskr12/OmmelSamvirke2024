using ErrorHandling.Interfaces;
using ErrorHandling.Services;
using Microsoft.Extensions.DependencyInjection;

namespace ErrorHandling;

public static class ModuleSetup
{
    public static void InitializeErrorHandlingModule(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddSingleton<IErrorHandler, ErrorHandler>();
        serviceCollection.AddSingleton<IErrorTranslationService, ErrorTranslationService>();
        serviceCollection.AddTransient<IValidator, Validator>();
    }
}

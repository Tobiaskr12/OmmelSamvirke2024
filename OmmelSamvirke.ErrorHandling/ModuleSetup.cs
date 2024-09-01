using Microsoft.Extensions.DependencyInjection;
using OmmelSamvirke.ErrorHandling.Interfaces;
using OmmelSamvirke.ErrorHandling.Services;

namespace OmmelSamvirke.ErrorHandling;

public static class ModuleSetup
{
    public static void InitializeErrorHandlingModule(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddSingleton<IErrorHandler, ErrorHandler>();
        serviceCollection.AddSingleton<IErrorTranslationService, ErrorTranslationService>();
        serviceCollection.AddTransient<IValidator, Validator>();
    }
}

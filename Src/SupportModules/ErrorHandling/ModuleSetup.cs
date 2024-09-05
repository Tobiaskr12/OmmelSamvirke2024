using ErrorHandling.Interfaces;
using ErrorHandling.Interfaces.Contracts;
using ErrorHandling.Services;
using ErrorHandling.Services.Errors;
using ErrorHandling.Services.Validation;
using Microsoft.Extensions.DependencyInjection;

namespace ErrorHandling;

public static class ModuleSetup
{
    public static void InitializeErrorHandlingModule(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddSingleton<IErrorFactory, ErrorFactory>();
        serviceCollection.AddSingleton<IErrorTranslationService, ErrorTranslationService>();
        serviceCollection.AddTransient<IValidator, Validator>();
    }
}

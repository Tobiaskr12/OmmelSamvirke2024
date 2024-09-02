using EmailWrapper.Errors;
using EmailWrapper.Interfaces;
using EmailWrapper.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using ErrorHandling.Interfaces;

namespace EmailWrapper;

public static class ModuleSetup
{
    
    public static void InitializeEmailWrapperModule(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddSingleton<IEmailSender, EmailSender>();
    }

    public static void ConfigureEmailWrapperModule(this WebApplication application)
    {
        var errorTranslationService = application.Services.GetService<IErrorTranslationService>();
        if (errorTranslationService is null)
            throw new ApplicationException("IErrorTranslationService has not been registered correctly");
        
        ContactListErrors.RegisterErrorMessages(errorTranslationService);
    }
}

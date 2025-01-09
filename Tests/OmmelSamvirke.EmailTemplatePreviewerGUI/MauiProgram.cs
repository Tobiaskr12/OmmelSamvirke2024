using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OmmelSamvirke.DataAccess;
using OmmelSamvirke.DomainModules;
using OmmelSamvirke.EmailTemplatePreviewGUI;
using OmmelSamvirke.EmailTemplatePreviewGUI.Services;
using OmmelSamvirke.Infrastructure;
using OmmelSamvirke.ServiceModules;
using OmmelSamvirke.SupportModules.Logging;
using OmmelSamvirke.SupportModules.SecretsManager;

namespace OmmelSamvirke.EmailTemplatePreviewerGUI;
public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();

        // Setup Configuration
        var configurationRoot = builder.Configuration
               .AddKeyVaultSecrets(ExecutionEnvironment.Development)
               .AddEnvironmentVariables()
               .Build();

        builder.Services.AddSingleton(configurationRoot);

        // Register custom logger
        ILogger appLogger = AppLoggerFactory.CreateLogger(builder.Configuration);
        builder.Logging.ClearProviders();
        builder.Logging.AddProvider(new AppLoggerProvider(appLogger));

        // Register services
        builder.Services.AddSingleton(appLogger)
               .InitializeDataAccessModule(builder.Configuration).Result
               .InitializeInfrastructureModule()
               .InitializeDomainModule()
               .InitializeServicesModule();

        builder.Services.AddSingleton<FileWatcherService>();
        builder.Services.AddHostedService(provider => provider.GetService<FileWatcherService>());

        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            });

        builder.Services.AddMauiBlazorWebView();

#if DEBUG
		builder.Services.AddBlazorWebViewDeveloperTools();
		builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}

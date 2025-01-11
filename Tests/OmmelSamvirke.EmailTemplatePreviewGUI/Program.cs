using MudBlazor.Services;
using OmmelSamvirke.DataAccess;
using OmmelSamvirke.DomainModules;
using OmmelSamvirke.EmailTemplatePreviewGUI.Components;
using OmmelSamvirke.EmailTemplatePreviewGUI.ViewModels;
using OmmelSamvirke.Infrastructure;
using OmmelSamvirke.ServiceModules;
using OmmelSamvirke.SupportModules.Logging;
using OmmelSamvirke.SupportModules.SecretsManager;

namespace OmmelSamvirke.EmailTemplatePreviewGUI;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddRazorComponents()
               .AddInteractiveServerComponents();
        
        // Setup Configuration
        var configuration = builder.Configuration
               .AddKeyVaultSecrets(builder.Environment.IsDevelopment() ? ExecutionEnvironment.Development : ExecutionEnvironment.Production)
               .AddEnvironmentVariables()
               .Build();
        
        // Register custom logger
        ILogger appLogger = AppLoggerFactory.CreateLogger(builder.Configuration);
        builder.Logging.ClearProviders();
        builder.Logging.AddProvider(new AppLoggerProvider(appLogger));
        
        builder.Services.AddSingleton(configuration);
        builder.Services.AddSingleton<FileWatcherService>();
        
        // Register services
        builder.Services
            .AddSingleton(appLogger)
            .InitializeDataAccessModule(builder.Configuration).Result
            .InitializeInfrastructureModule()
            .InitializeDomainModule()
            .InitializeServicesModule();

        // Register ViewModels
        builder.Services.AddScoped<TargetDeviceViewModel>();
        builder.Services.AddScoped<ThemeViewModel>();

        // Add MudBlazor
        builder.Services.AddMudServices();

        builder.Services.AddSingleton(configuration);
        builder.Services.AddSingleton<FileWatcherService>();

        var app = builder.Build();

        app.MapHub<FileChangeHub>("/fileChangeHub");

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        app.UseHttpsRedirection();

        app.UseStaticFiles();
        app.UseAntiforgery();

        app.MapRazorComponents<App>()
           .AddInteractiveServerRenderMode();

        app.Run();
    }
}
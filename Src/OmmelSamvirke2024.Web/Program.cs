using ApexCharts;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Options;
using MudBlazor.Services;
using OmmelSamvirke.SupportModules.SecretsManager;
using OmmelSamvirke.Bootstrapper;
using OmmelSamvirke2024.Web.BackgroundServices;
using OmmelSamvirke2024.Web.Components;
using OmmelSamvirke2024.Web.Components.Pages.TechnicalData;
using OmmelSamvirke2024.Web.Components.ViewModels;
using System.Globalization;
using Contracts.SupportModules.SecretsManager;

namespace OmmelSamvirke2024.Web;

public class Program
{
    public static void Main(string[] args)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddRazorComponents()
               .AddInteractiveServerComponents();
        
        // Setup Configuration
        ExecutionEnvironment executionEnvironment = builder.Environment.IsDevelopment()
            ? ExecutionEnvironment.Development
            : ExecutionEnvironment.Production;
        IConfigurationRoot configuration = 
            builder.Configuration
                   .AddKeyVaultSecrets(executionEnvironment)
                   .AddEnvironmentVariables()
                   .Build();
        
        builder.Services.AddSingleton(configuration);
        
        // Register all service-layers
        builder.Services.InitializeAllServices(configuration, executionEnvironment);

        // Register pages
        builder.Services.InitializeTechnicalDataPage();

        // Add ViewModels
        builder.Services.AddScoped<ThemeViewModel>();
        
        // Add third-party libraries
        builder.Services.AddMudServices();
        builder.Services.AddApexCharts();

        // Setup localization
        var supportedCultureCodes = new[] { "da", "en" };
        var supportedCultures = supportedCultureCodes.Select(code => new CultureInfo(code)).ToList();

        builder.Services.AddLocalization();
        builder.Services.Configure<RequestLocalizationOptions>(options =>
        {
            options.DefaultRequestCulture = new RequestCulture("da");
            options.SupportedCultures = supportedCultures;
            options.SupportedUICultures = supportedCultures;

            options.FallBackToParentCultures = true;
            options.FallBackToParentUICultures = true;

            options.RequestCultureProviders.Clear();

            options.RequestCultureProviders.Insert(0, new CustomRequestCultureProvider(async context =>
            {
                var acceptLangs = context.Request.Headers["Accept-Language"].ToString();
                string primaryLanguage = acceptLangs.Split(',').FirstOrDefault() ?? "da";

                if (primaryLanguage.Contains("en", StringComparison.OrdinalIgnoreCase))
                {
                    primaryLanguage = "en";
                }
                else if (primaryLanguage.Contains("da", StringComparison.OrdinalIgnoreCase))
                {
                    primaryLanguage = "da";
                }
                else
                {
                    primaryLanguage = "da";
                }

                return await Task.FromResult(new ProviderCultureResult(primaryLanguage, primaryLanguage));
            }));
        });

        // Register background services
        builder.Services.AddHostedService<LogCleaningService>();

        WebApplication app = builder.Build();

        var localizationOptions = app.Services.GetRequiredService<IOptions<RequestLocalizationOptions>>().Value;
        app.UseRequestLocalization(localizationOptions);

        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Error", createScopeForErrors: true);
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

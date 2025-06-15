using ApexCharts;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Options;
using MudBlazor.Services;
using SupportModules.SecretsManager;
using Bootstrapper;
using Web.Components;
using Web.Components.ViewModels;
using System.Globalization;
using Contracts.SupportModules.SecretsManager;
using MudBlazor.Translations;
using SupportModules;

namespace Web;

public class Program
{
    public static void Main(string[] args)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
        
        // Set ICS-feed output path
        string webRootPath = builder.Environment.WebRootPath;
        string calendarFilePath = Path.Combine(webRootPath, "calendar.ics");
        
        Dictionary<string, string> inMemoryConfig = new()
        {
            { "CalendarFilePath", calendarFilePath }
        };
        
        // Setup Configuration
        ExecutionEnvironment executionEnvironment = builder.Environment.IsDevelopment()
            ? ExecutionEnvironment.Development
            : ExecutionEnvironment.Production;

        IConfigurationRoot configuration = 
            builder.Configuration
                   .AddKeyVaultSecrets(executionEnvironment)
                   .AddEnvironmentVariables()
                   .AddInMemoryCollection(inMemoryConfig!)
                   .Build();
        
        SerilogConfigurator.ConfigureStaticLogger(configuration, executionEnvironment);
        builder.Host.UseSharedSerilogConfiguration();
        
        // Add services to the container.
        builder.Services.AddRazorComponents()
               .AddInteractiveServerComponents();
        
        builder.Services.AddSingleton(configuration);
        
        // Register all service-layers
        builder.Services.InitializeAllServices(configuration, executionEnvironment);

        // Add ViewModels
        builder.Services.AddScoped<ThemeViewModel>();
        
        // Add third-party libraries
        builder.Services.AddMudServices().AddMudTranslations();
        builder.Services.AddApexCharts();

        // Setup localization
        string[] supportedCultureCodes = ["da", "en"];
        List<CultureInfo>? supportedCultures = supportedCultureCodes.Select(code => new CultureInfo(code)).ToList();

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
                string acceptLangs = context.Request.Headers.AcceptLanguage.ToString();
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

                return await Task.FromResult(new ProviderCultureResult("da", "da"));
                
                // Enable to use the browsers language
                //return await Task.FromResult(new ProviderCultureResult(primaryLanguage, primaryLanguage));
            }));
        });

        WebApplication app = builder.Build();

        RequestLocalizationOptions? localizationOptions = app.Services.GetRequiredService<IOptions<RequestLocalizationOptions>>().Value;
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
        
        // Expose ICS-file (Calendar-events)
        app.MapGet("/calendar.ics", (IWebHostEnvironment env) =>
        {
            string filePath = Path.Combine(env.WebRootPath, "calendar.ics");
            return !File.Exists(filePath) 
                ? Results.NotFound("ICS file not found. Please try again later.") 
                : Results.File(filePath, "text/calendar");
        });

        app.Run();
    }
}

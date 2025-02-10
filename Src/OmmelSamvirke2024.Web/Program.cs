using ApexCharts;
using MudBlazor.Services;
using OmmelSamvirke.ServiceModules;
using OmmelSamvirke.SupportModules.SecretsManager;
using OmmelSamvirke2024.ServiceDefaults;
using OmmelSamvirke2024.Web.Components;
using OmmelSamvirke2024.Web.Components.Pages.TechnicalData;
using OmmelSamvirke2024.Web.Components.ViewModels;

namespace OmmelSamvirke2024.Web;

public class Program
{
    public static void Main(string[] args)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

        // Add service defaults & Aspire components.
        builder.AddServiceDefaults();

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
        
        builder.Services.AddOutputCache();
        builder.Services.AddHttpClient<WeatherApiClient>(client =>
        {
            // This URL uses "https+http://" to indicate HTTPS is preferred over HTTP.
            // Learn more about service discovery scheme resolution at https://aka.ms/dotnet/sdschemes.
            client.BaseAddress = new("https+http://apiservice");
        });

        WebApplication app = builder.Build();

        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Error", createScopeForErrors: true);
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        app.UseHttpsRedirection();

        app.UseStaticFiles();
        app.UseAntiforgery();

        app.UseOutputCache();

        app.MapRazorComponents<App>()
           .AddInteractiveServerRenderMode();

        app.MapDefaultEndpoints();

        app.Run();
    }
}
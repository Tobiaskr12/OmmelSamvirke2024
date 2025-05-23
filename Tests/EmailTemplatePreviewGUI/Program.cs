using MudBlazor.Services;
using EmailTemplatePreviewGUI.ViewModels;
using Bootstrapper;
using SupportModules.SecretsManager;
using Contracts.SupportModules.SecretsManager;
using EmailTemplatePreviewGUI.Components;

namespace EmailTemplatePreviewGUI;

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
        builder.Services.AddScoped<FileWatcherService>();
        
        // Register services
        builder.Services.InitializeAllServices(configuration, executionEnvironment);

        // Register ViewModels
        builder.Services.AddScoped<TargetDeviceViewModel>();
        builder.Services.AddScoped<ThemeViewModel>();
        builder.Services.AddScoped<EmailTemplatesViewModel>();

        // Add MudBlazor
        builder.Services.AddMudServices();
        
        WebApplication app = builder.Build();

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
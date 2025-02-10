using CommunityToolkit.Mvvm.Messaging;
using OmmelSamvirke2024.Web.Components.Pages.TechnicalData.Components;

namespace OmmelSamvirke2024.Web.Components.Pages.TechnicalData;

public static class PageSetup
{
    public static IServiceCollection InitializeTechnicalDataPage(this IServiceCollection services)
    {
        services.AddScoped<DashboardViewModel>();
        services.AddScoped<TimelineViewModel>();

        return services;
    }
}

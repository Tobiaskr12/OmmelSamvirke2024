using CommunityToolkit.Mvvm.Messaging;
using Web.Components.Pages.TechnicalData.Components;

namespace Web.Components.Pages.TechnicalData;

public static class PageSetup
{
    public static IServiceCollection InitializeTechnicalDataPage(this IServiceCollection services)
    {
        services.AddScoped<DashboardViewModel>();
        services.AddScoped<TimelineViewModel>();

        return services;
    }
}

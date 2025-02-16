namespace Web.Components.Pages.TechnicalData;

public static class PageSetup
{
    public static IServiceCollection InitializeTechnicalDataPage(this IServiceCollection services)
    {
        services.AddScoped<DashboardViewModel>();
        services.AddScoped<Components.Timeline.TimelineViewModel>();

        return services;
    }
}

namespace OmmelSamvirke2024.Web.Components.Pages.TechnicalData.Components.Timeline.Models;

public class TimelinePointSeries<T>
{
    public required Type Type { get; set; }
    public required string Name { get; set; }
    public required List<T> Items { get; set; }
}

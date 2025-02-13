using ApexCharts;
using CommunityToolkit.Mvvm.ComponentModel;
using Contracts.SupportModules.Logging.Models;
using OmmelSamvirke2024.Web.Components.Pages.TechnicalData.Components;
using OmmelSamvirke2024.Web.Components.Pages.TechnicalData.Components.Timeline;
using OmmelSamvirke2024.Web.Components.Pages.TechnicalData.Components.Timeline.Models;
using System.ComponentModel;

namespace OmmelSamvirke2024.Web.Components.Pages.TechnicalData;

public partial class TimelineViewModel : ObservableObject
{
    private readonly DashboardViewModel _dashboardViewModel;
    private ApexChart<BucketedEntry>? _chart;

    [ObservableProperty] private TimelinePointSeries<BucketedEntry>? _timeline;
    [ObservableProperty] private string _xAxisFormat = "dd/MM/yyyy";

    public TimelineViewModel(DashboardViewModel dashboardViewModel)
    {
        _dashboardViewModel = dashboardViewModel;
        _dashboardViewModel.PropertyChanged += OnDashboardChanged;

        OnDashboardChanged(null, null!);
    }

    public void SetChart(ApexChart<BucketedEntry> chart) 
    { 
        _chart = chart;
    }
    
    private async void OnDashboardChanged(object? sender, PropertyChangedEventArgs args)
    {
        if (_dashboardViewModel.CurrentView != DashboardView.Minute) 
        { 
            switch (_dashboardViewModel.CurrentlyActiveTab)
            {
                case DashboardTab.Logs:
                    Timeline = BuildLogsTimeline(_dashboardViewModel.FilteredLogs);
                    break;
                case DashboardTab.Traces:
                    Timeline = BuildTracesTimeline(_dashboardViewModel.FilteredTraces);
                    break;
            }
        }

        if (_chart is null) return;
        if (_dashboardViewModel.CurrentView == DashboardView.Minute)
        {
            _chart.Options.States = new States()
            {
                Active = new()
                {
                    AllowMultipleDataPointsSelection = false,
                    Filter = new()
                    {
                        Type = StatesFilterType.darken
                    }
                }
            };
        }
        else
        {
            _chart.Options.States = new States()
            {
                Active = new()
                {
                    AllowMultipleDataPointsSelection = false,
                    Filter = new()
                    {
                        Type = StatesFilterType.none
                    }
                }
            };
        }

        await _chart.UpdateOptionsAsync(redrawPaths: false, animate: false, updateSyncedCharts: false);
    }

    private TimelinePointSeries<BucketedEntry> BuildLogsTimeline(IEnumerable<LogEntry> logs)
    {
        var grouped = TimelineGroupingHelper.GroupByTimeInterval(logs);
        return new TimelinePointSeries<BucketedEntry>()
        {
            Type = typeof(LogEntry),
            Name = "Logs",
            Items = grouped.ToList()
        };
    }

    private TimelinePointSeries<BucketedEntry> BuildTracesTimeline(IEnumerable<TraceEntry> traces)
    {
        var grouped = TimelineGroupingHelper.GroupByTimeInterval(traces);
        return new TimelinePointSeries<BucketedEntry>()
        {
            Type = typeof(TraceEntry),
            Name = "Traces",
            Items = grouped.ToList()
        };
    }
}

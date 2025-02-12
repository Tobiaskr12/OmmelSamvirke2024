using ApexCharts;
using CommunityToolkit.Mvvm.ComponentModel;
using MudBlazor;
using OmmelSamvirke.SupportModules.Logging.Interfaces;
using OmmelSamvirke.SupportModules.Logging.Models;
using OmmelSamvirke2024.Web.Components.Pages.TechnicalData.Components.Timeline.Models;

namespace OmmelSamvirke2024.Web.Components.Pages.TechnicalData.Components;

public enum DashboardTab
{
    Logs,
    Traces
}

public enum DashboardView
{
    MultipleDays,
    SingleDay,
    Hour,
    Minute
}

public partial class DashboardViewModel : ObservableObject
{
    private readonly ILogRepository _logRepository;
    private readonly ITraceRepository _traceRepository;
    private DateTime? _selectedDate = null;
    private string? _selectedMinute = null;

    public DashboardViewModel(ILogRepository logRepository, ITraceRepository traceRepository)
    {
        _logRepository = logRepository;
        _traceRepository = traceRepository;
    }

    [ObservableProperty] private DashboardTab _currentlyActiveTab = DashboardTab.Logs;
    [ObservableProperty] private DateRange _dateRange = new(DateTime.UtcNow.AddDays(-7).Date, DateTime.UtcNow.Date);
    [ObservableProperty] private DashboardView _currentView = DashboardView.MultipleDays;
    [ObservableProperty] private string _searchString = string.Empty;

    private IEnumerable<LogEntry> _logs = [];
    public IEnumerable<LogEntry> Logs 
    { 
        get => _logs;
        private set => SetProperty(ref _logs, value);
    }

    private IEnumerable<TraceEntry> _traces = [];
    public IEnumerable<TraceEntry> Traces 
    {
        get => _traces;
        private set => SetProperty(ref _traces, value);
    }

    public IEnumerable<LogEntry> FilteredLogs 
    { 
        get => string.IsNullOrWhiteSpace(SearchString) 
            ? Logs 
            : Logs.Where(log => LogsFilterFunc(log, SearchString));
    }

    public IEnumerable<TraceEntry> FilteredTraces
    {
        get => string.IsNullOrWhiteSpace(SearchString)
            ? Traces
            : Traces.Where(trace => TracesFilterFunc(trace, SearchString));
    }

    public void ReloadData(DateRange dateRange)
    {
        DateTime? start = dateRange.Start;
        DateTime? end = dateRange.End;
        if (start is null || end is null) return;

        ReloadData(start.Value.Date, end.Value.Date.AddDays(1).AddTicks(-1));
    }

    public void ReloadData(DateTime startTime,  DateTime endTime)
    {
        TimeSpan interval = endTime - startTime;

        Logs = _logRepository.QueryLogs(startTime.ToUniversalTime(), interval).OrderByDescending(x => x.Timestamp);
        Traces = _traceRepository.QueryTracesForRequest(startTime.ToUniversalTime(), interval).OrderByDescending(x => x.Timestamp);
    }

    public string IncreaseDashboardScope()
    {
        switch (CurrentView)
        {
            case DashboardView.SingleDay:
                CurrentView = DashboardView.MultipleDays;
                _selectedDate = null;

                ReloadData(DateRange);
                break;
            case DashboardView.Hour:
            case DashboardView.Minute:
                CurrentView = DashboardView.SingleDay;
                GetLogsForADay();
                break;

        }

        _selectedMinute = null;
        return GetCurrentViewDateTimeFormat();
    }

    public string DecreaseDashboardScope(IDataPoint<BucketedEntry> selectedDataPoint) {
        switch (CurrentView)
        {
            case DashboardView.MultipleDays:
                CurrentView = DashboardView.SingleDay;
                bool dateParseResult = DateTime.TryParse(selectedDataPoint.X.ToString(), out DateTime selectedDate);
                if (dateParseResult)
                {
                    _selectedDate = selectedDate;
                    if (_selectedDate is not null)
                    {
                        ReloadData(new DateRange(_selectedDate.Value.Date, _selectedDate.Value.Date));
                    }
                }

                break;
            case DashboardView.SingleDay:
                bool hourParseResult = TimeSpan.TryParse(selectedDataPoint.X.ToString(), out TimeSpan selectedHour);
                if (!hourParseResult) break;

                CurrentView = DashboardView.Hour;
                GetLogsForAnHour(selectedHour);

                break;

            case DashboardView.Hour:
                {
                    bool minuteParseResult = TimeSpan.TryParse(selectedDataPoint.X.ToString(), out TimeSpan selectedMinute);
                    if (!minuteParseResult) break;

                    CurrentView = DashboardView.Minute;
                    GetLogsForAMinute(selectedMinute);

                    _selectedMinute = new DateTime().Add(selectedMinute).ToString("HH:mm");

                    break;
                }

            case DashboardView.Minute:
                if (_selectedMinute == selectedDataPoint.X.ToString())
                {
                    bool hourWithMinutesParseResult = TimeSpan.TryParse(selectedDataPoint.X.ToString(), out TimeSpan selectedHourWithMinutes);
                    if (!hourWithMinutesParseResult) break;

                    CurrentView = DashboardView.Hour;
                    GetLogsForAnHour(TimeSpan.FromHours(selectedHourWithMinutes.Hours));

                    _selectedMinute = null;
                } else
                {
                    bool minuteParseResult = TimeSpan.TryParse(selectedDataPoint.X.ToString(), out TimeSpan selectedMinute);
                    if (!minuteParseResult) break;

                    CurrentView = DashboardView.Minute;
                    GetLogsForAMinute(selectedMinute);

                    _selectedMinute = new DateTime().Add(selectedMinute).ToString("HH:mm");
                }

                break;
        }

        return GetCurrentViewDateTimeFormat();
    }

    private void GetLogsForAMinute(TimeSpan selectedTime)
    {
        if (_selectedDate is not null)
        {
            var start = _selectedDate.Value.Date.Add(selectedTime);
            var end = start.AddMinutes(1);
            ReloadData(start, end);
        }
    }

    private void GetLogsForAnHour(TimeSpan selectedTime)
    {
        if (_selectedDate is not null)
        {
            var start = _selectedDate.Value.Date.Add(selectedTime);
            var end = start.AddHours(1);
            ReloadData(start, end);
        }
    }

    private void GetLogsForADay()
    {
        if (_selectedDate is not null)
        {
            ReloadData(new DateRange(_selectedDate.Value.Date, _selectedDate.Value.Date));
        }
        else
        {
            CurrentView = DashboardView.MultipleDays;
            ReloadData(DateRange);
        }
    }

    private string GetCurrentViewDateTimeFormat()
    {
        switch (CurrentView) 
        {
            case DashboardView.MultipleDays:
                return "dd/MM/yyyy";
            case DashboardView.SingleDay:
                return "HH:00";
            case DashboardView.Hour:
            case DashboardView.Minute:
                return "HH:mm";
        }

        return "dd/MM/yyyy";
    }

    public bool LogsFilterFunc(LogEntry log) => LogsFilterFunc(log, SearchString);
    private bool LogsFilterFunc(LogEntry log, string searchString)
    {
        if (string.IsNullOrWhiteSpace(searchString)) return true;
        if (log.Timestamp.ToLocalTime().ToString("dd/MM/yyyy HH:mm:ss.fff").Contains(searchString, StringComparison.Ordinal)) return true;
        if (log.Message.Contains(searchString, StringComparison.OrdinalIgnoreCase)) return true;
        if (log.SessionId.Contains(searchString, StringComparison.OrdinalIgnoreCase)) return true;
        if (log.OperationId.ToString().Contains(searchString, StringComparison.OrdinalIgnoreCase)) return true;
        if (log.CallerAssemblyName.Contains(searchString, StringComparison.OrdinalIgnoreCase)) return true;
        if (log.Exception?.Message.Contains(searchString, StringComparison.OrdinalIgnoreCase) ?? false) return true;
        if ($"{log.CallerServiceName} | {log.CallerMemberName}:{log.CallerLineNumber}".Contains(searchString, StringComparison.OrdinalIgnoreCase)) return true;
        return false;
    }

    public bool TracesFilterFunc(TraceEntry trace) => TracesFilterFunc(trace, SearchString);
    private bool TracesFilterFunc(TraceEntry trace, string searchString)
    {
        if (string.IsNullOrWhiteSpace(searchString)) return true;
        if (trace.Timestamp.ToLocalTime().ToString("dd/MM/yyyy HH:mm:ss.fff").Contains(searchString, StringComparison.Ordinal)) return true;
        if (trace.SessionId.Contains(searchString, StringComparison.OrdinalIgnoreCase)) return true;
        if (trace.OperationId.ToString().Contains(searchString, StringComparison.OrdinalIgnoreCase)) return true;
        if (trace.RequestName.Contains(searchString, StringComparison.OrdinalIgnoreCase)) return true;
        if (trace.OperationType.Contains(searchString, StringComparison.OrdinalIgnoreCase)) return true;
        return false;
    }
}

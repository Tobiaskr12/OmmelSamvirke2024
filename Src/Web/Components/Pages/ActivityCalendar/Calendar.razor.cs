using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Web.Components.Pages.ActivityCalendar;

public partial class Calendar : ComponentBase
{
    [CascadingParameter(Name = "CurrentBreakpoint")]
    public Breakpoint CurrentBreakpoint { get; set; }

    private ElementReference DesktopCalendar;
    private ElementReference MobileCalendar;
}
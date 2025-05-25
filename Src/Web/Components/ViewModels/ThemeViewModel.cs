using CommunityToolkit.Mvvm.ComponentModel;
using MudBlazor;

namespace Web.Components.ViewModels;

public enum AppTheme
{
    Light,
    Dark
}

public partial class ThemeViewModel : ObservableObject
{
    [ObservableProperty] private AppTheme _theme;

    public PaletteLight PaletteLight { get; init; }
    public PaletteDark PaletteDark { get; init; }

    public ThemeViewModel()
    {
        PaletteLight = new PaletteLight
        {
            Primary = "#284844",
            PrimaryContrastText = "#FFF",
            Background = "#F4F4F4"
        };
    }
}

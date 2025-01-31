using CommunityToolkit.Mvvm.ComponentModel;

namespace OmmelSamvirke.EmailTemplatePreviewGUI.ViewModels;

public enum AppTheme
{
    Light,
    Dark
}

public partial class ThemeViewModel : ObservableObject
{
    [ObservableProperty] private AppTheme _theme;
}

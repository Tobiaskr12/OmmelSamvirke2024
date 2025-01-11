using OmmelSamvirke.EmailTemplatePreviewGUI.ViewModels.BaseClasses;

namespace OmmelSamvirke.EmailTemplatePreviewGUI.ViewModels;

public enum AppTheme
{
    Light,
    Dark
}

public class ThemeViewModel : ViewModelBase
{
    private AppTheme _theme;

    public AppTheme Theme 
    { 
        get => _theme;
        set
        {
            if (value != _theme)
            {
                _theme = value;
                OnPropertyChanged();
            }
        }
    }
}

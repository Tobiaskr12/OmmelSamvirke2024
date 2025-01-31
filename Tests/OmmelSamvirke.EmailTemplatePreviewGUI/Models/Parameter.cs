using CommunityToolkit.Mvvm.ComponentModel;

namespace OmmelSamvirke.EmailTemplatePreviewGUI.Models;

public partial class Parameter : ObservableObject
{
    [ObservableProperty] private string _name = string.Empty;
    [ObservableProperty] private string _value = string.Empty;
}

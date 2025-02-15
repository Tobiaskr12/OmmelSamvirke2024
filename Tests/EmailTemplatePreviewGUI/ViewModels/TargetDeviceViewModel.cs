using CommunityToolkit.Mvvm.ComponentModel;

namespace EmailTemplatePreviewGUI.ViewModels;

public enum TargetDevice
{
    Desktop,
    Tablet,
    Mobile
}

public partial class TargetDeviceViewModel : ObservableObject
{
    [ObservableProperty] private TargetDevice _targetDevice = TargetDevice.Tablet;
}

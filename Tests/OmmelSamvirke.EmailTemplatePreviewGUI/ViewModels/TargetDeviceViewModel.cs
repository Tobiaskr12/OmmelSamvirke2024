using OmmelSamvirke.EmailTemplatePreviewGUI.ViewModels.BaseClasses;

namespace OmmelSamvirke.EmailTemplatePreviewGUI.ViewModels;

public enum TargetDevice
{
    Desktop,
    Tablet,
    Mobile
}

public class TargetDeviceViewModel : ViewModelBase
{
    private TargetDevice _targetDevice = TargetDevice.Tablet;
    public TargetDevice TargetDevice
    {
        get => _targetDevice;
        private set
        {
            if (_targetDevice != value)
            {
                _targetDevice = value;
                OnPropertyChanged();
            }
        }
    }

    public void UpdateTargetDevice(TargetDevice device)
    {
        TargetDevice = device;
    }
}

using BlackBoxControl.Models;

namespace BlackBoxControl.ViewModels
{
    public class DeviceItemViewModel : TreeNodeViewModel
    {
        public SelectableDevice Device { get; }

        public DeviceItemViewModel(SelectableDevice device)
        {
            Device = device;

            DisplayName = $"{device.Type} (Addr {device.Address})";
            Icon = "•"; // or use device.ImagePath
            NodeType = TreeNodeType.Device;
        }
    }

}

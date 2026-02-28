using BlackBoxControl.Models;

namespace BlackBoxControl.ViewModels
{
    public class SelectableDeviceNodeViewModel : TreeNodeViewModel
    {
        public SelectableDevice Device { get; }

        public SelectableDeviceNodeViewModel(SelectableDevice device)
        {
            Device = device;
            DisplayName = $"{device.Type} (Addr {device.Address})";
            NodeType = TreeNodeType.CauseDeviceNode;
            // Use icon path if available, else fallback
            Icon = device.ImagePath ?? "🔸";
        }
    }
}

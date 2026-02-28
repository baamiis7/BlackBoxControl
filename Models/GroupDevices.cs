using System.Collections.ObjectModel;

namespace BlackBoxControl.Models
{
    public class GroupedDevice
    {
        public string GroupName { get; set; }
        public ObservableCollection<SelectableDevice> Devices { get; set; }

        public GroupedDevice(string name)
        {
            GroupName = name;
            Devices = new ObservableCollection<SelectableDevice>();
        }
    }
}

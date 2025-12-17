using BlackBoxControl.ViewModels;
using System.Collections.ObjectModel;

public class GroupedDevice
{
    public string GroupName { get; set; }   // “Loop 1”, “Bus 2”
    public ObservableCollection<SelectableDevice> Devices { get; set; }

    public GroupedDevice(string name)
    {
        GroupName = name;
        Devices = new ObservableCollection<SelectableDevice>();
    }
}

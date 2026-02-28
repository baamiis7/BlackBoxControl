using BlackBoxControl.Models;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace BlackBoxControl.ViewModels
{
    public class LoopDeviceViewModel : TreeNodeViewModel
    {
        private LoopDevice _device = null!;

        public LoopDevice Device
        {
            get => _device;
            set
            {
                _device = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(NodeName));
                OnPropertyChanged(nameof(IconPath));
            }
        }

        // WHAT THE TREEVIEW DISPLAYS
        public string NodeName
        {
            get
            {
                // Simply return the device type
                return Device?.Type ?? "Unknown Device";
            }
        }


        // ICON DISPLAYED IN TREEVIEW
        public string IconPath =>
            string.IsNullOrWhiteSpace(Device?.ImagePath)
                ? "/Assets/Icons/default-device.png"
                : Device.ImagePath;

        public LoopDeviceViewModel(LoopDevice device)
        {
            Device = device;
            NodeType = TreeNodeType.Device;  // IMPORTANT!
            SaveCommand = new RelayCommand(SaveChanges);
            CancelCommand = new RelayCommand(CancelChanges);
        }

        // Available input actions for ComboBox
        public ObservableCollection<string> InputActions { get; set; } =
            new ObservableCollection<string>
            {
                "None",
                "Activate",
                "Deactivate",
                "Alarm",
                "Reset"
            };

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        private void SaveChanges()
        {
            MessageBox.Show(
                "Changes saved successfully!",
                "Save",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        private void CancelChanges()
        {
            MessageBox.Show(
                "Changes canceled.",
                "Cancel",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
        }

    }
}



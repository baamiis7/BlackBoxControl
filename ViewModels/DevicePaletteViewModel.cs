using System.Collections.ObjectModel;
using System.IO;
using Newtonsoft.Json;
using BlackBoxControl.Models;

namespace BlackBoxControl.ViewModels
{
    public class DevicePaletteViewModel : ViewModelBase
    {
        public ObservableCollection<LoopDevice> AvailableDevices { get; } = new();
        public ObservableCollection<BusNode> AvailableBusNodes { get; } = new();

        private ObservableCollection<LoopDevice> _allowedDevices = new();
        public ObservableCollection<LoopDevice> AllowedDevices
        {
            get { return _allowedDevices; }
            private set
            {
                _allowedDevices = value;
                OnPropertyChanged(nameof(AllowedDevices));
            }
        }

        private LoopDevice? _selectedDevice;
        public LoopDevice? SelectedDevice
        {
            get { return _selectedDevice; }
            set
            {
                _selectedDevice = value;
                OnPropertyChanged(nameof(SelectedDevice));
            }
        }

        public DevicePaletteViewModel()
        {
            LoadAvailableDevices();
            LoadAvailableBusNodes();
        }

        public void Update(object? selectedNode)
        {
            if (selectedNode is LoopViewModel || selectedNode is Loop)
            {
                AllowedDevices = new ObservableCollection<LoopDevice>(AvailableDevices);
            }
            else if (selectedNode is BusViewModel || selectedNode is Bus)
            {
                AllowedDevices.Clear();
            }
            else if (selectedNode is BlackBoxControlPanelViewModel || selectedNode is BlackBoxControlPanel)
            {
                AllowedDevices = new ObservableCollection<LoopDevice>(AvailableDevices);
            }
            else
            {
                AllowedDevices.Clear();
            }

            OnPropertyChanged(nameof(AllowedDevices));
        }

        private void LoadAvailableDevices()
        {
            string imagesDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Images");
            string configDirectory = Path.Combine(Directory.GetCurrentDirectory(), "DeviceConfigurations");

            if (Directory.Exists(imagesDirectory) && Directory.Exists(configDirectory))
            {
                var deviceImages = Directory.GetFiles(imagesDirectory, "*.png");

                foreach (var imagePath in deviceImages)
                {
                    string deviceName = Path.GetFileNameWithoutExtension(imagePath);
                    string configPath = Path.Combine(configDirectory, deviceName + ".json");

                    if (File.Exists(configPath))
                    {
                        string jsonConfig = File.ReadAllText(configPath);
                        var deviceConfig = JsonConvert.DeserializeObject<LoopDevice>(jsonConfig);
                        if (deviceConfig == null) continue;

                        deviceConfig.Type = deviceName.Replace("_", " ");
                        deviceConfig.ImagePath = imagePath;

                        AvailableDevices.Add(deviceConfig);
                    }
                    else
                    {
                        var device = new LoopDevice
                        {
                            Type = deviceName.Replace("_", " "),
                            ImagePath = imagePath
                        };
                        AvailableDevices.Add(device);
                    }
                }
            }
        }

        private void LoadAvailableBusNodes()
        {
            AvailableBusNodes.Clear();

            string imagesDirectory = Path.Combine(Directory.GetCurrentDirectory(), "BusImages");
            string configDirectory = Path.Combine(Directory.GetCurrentDirectory(), "BusNodeConfigurations");

            if (Directory.Exists(imagesDirectory))
            {
                var busImages = Directory.GetFiles(imagesDirectory, "*.png");

                foreach (var imagePath in busImages)
                {
                    string nodeName = Path.GetFileNameWithoutExtension(imagePath);
                    string configPath = Path.Combine(configDirectory, nodeName + ".json");

                    if (File.Exists(configPath))
                    {
                        string jsonConfig = File.ReadAllText(configPath);
                        var busNodeConfig = JsonConvert.DeserializeObject<BusNode>(jsonConfig);
                        if (busNodeConfig == null) continue;

                        busNodeConfig.Name = nodeName.Replace("_", " ");
                        busNodeConfig.ImagePath = imagePath;

                        AvailableBusNodes.Add(busNodeConfig);
                    }
                    else
                    {
                        var busNode = new BusNode
                        {
                            Name = nodeName.Replace("_", " "),
                            ImagePath = imagePath,
                            LocationText = "",
                            Inputs = new ObservableCollection<BusNodeIO>(),
                            Outputs = new ObservableCollection<BusNodeIO>()
                        };
                        AvailableBusNodes.Add(busNode);
                    }
                }
            }
        }
    }
}

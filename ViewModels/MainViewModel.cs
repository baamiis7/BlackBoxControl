using System;
using System.Collections.ObjectModel;
using System.Windows;
using BlackBoxControl.Models;
using BlackBoxControl.Services;
using System.Linq;
using System.Collections.Generic;
using System.Windows.Input;
using BlackBoxControl.Helpers;

namespace BlackBoxControl.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        public MenuViewModel MenuViewModel { get; }
        public DevicePaletteViewModel DevicePalette { get; } = new();

        // Changed from BlackBoxControlPanel to BlackBoxControlPanelViewModel for tree structure support
        private ObservableCollection<BlackBoxControlPanelViewModel> _BlackBoxControlPanels = new ObservableCollection<BlackBoxControlPanelViewModel>();
        public ObservableCollection<BlackBoxControlPanelViewModel> BlackBoxControlPanels
        {
            get { return _BlackBoxControlPanels; }
            set
            {
                _BlackBoxControlPanels = value;
                OnPropertyChanged(nameof(BlackBoxControlPanels));
            }
        }

        private object? _selectedForm;
        private object? _selectedNode;

        public object? SelectedForm
        {
            get { return _selectedForm; }
            set
            {
                _selectedForm = value;
                OnPropertyChanged(nameof(SelectedForm));
            }
        }

        public void RefreshCauseEffectSaveState()
        {
            if (SelectedForm is CauseAndEffectViewModel ce)
            {
                (ce.SaveCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }

        public object? SelectedNode
        {
            get { return _selectedNode; }
            set
            {
                _selectedNode = value;
                System.Diagnostics.Debug.WriteLine($"SelectedNode changed to: {value?.GetType().Name}");
                OnPropertyChanged(nameof(SelectedNode));
                OnPropertyChanged(nameof(IsBusSelected)); // ADDED
                DevicePalette.Update(value);
                DisplayDetails();
            }
        }

        // ADDED: Property to determine if a bus is selected
        public bool IsBusSelected
        {
            get
            {
                var result = SelectedNode is BusViewModel || SelectedNode is Bus;
                System.Diagnostics.Debug.WriteLine($"IsBusSelected called: {result}, SelectedNode type: {SelectedNode?.GetType().Name}");
                return result;
            }
        }

        private string? _selectedItemDetails;
        public string? SelectedItemDetails
        {
            get { return _selectedItemDetails; }
            set
            {
                _selectedItemDetails = value;
                OnPropertyChanged(nameof(SelectedItemDetails));
            }
        }

        private string? _currentProjectPath;

        public string? CurrentProjectPath
        {
            get => _currentProjectPath;
            set
            {
                _currentProjectPath = value;
                OnPropertyChanged(nameof(CurrentProjectPath));
                OnPropertyChanged(nameof(WindowTitle));
            }
        }

        public string WindowTitle
        {
            get
            {
                if (string.IsNullOrEmpty(CurrentProjectPath))
                    return "Fire Panel Simulation - New Project";
                else
                    return $"Fire Panel Simulation - {System.IO.Path.GetFileName(CurrentProjectPath)}";
            }
        }

        // Commands
        public ICommand AddDeviceCommand { get; }
        public ICommand AddBusNodeCommand { get; } // ADDED

        public MainViewModel(IProjectService projectService)
        {
            MenuViewModel = new MenuViewModel(this, projectService);

            // Initialize collections
            BlackBoxControlPanels = new ObservableCollection<BlackBoxControlPanelViewModel>();

            // Initialize commands
            AddDeviceCommand = new RelayCommand<LoopDevice>(device => AddDeviceToSelectedItem(device));
            AddBusNodeCommand = new RelayCommand<BusNode>(busNode => AddBusNodeToSelectedBus(busNode)); // ADDED

            // Don't create initial project automatically
            // CreateNewProject();
        }

        /// <summary>
        /// Creates a new project with the complete tree structure
        /// </summary>
        public void CreateNewProject()
        {
            // Create new Fire Panel
            var newPanel = new BlackBoxControlPanel
            {
                PanelName = "New Fire Panel",
                Location = "Building 1",
                PanelAddress = BlackBoxControlPanels.Count + 1,
                NumberOfLoops = 2,
                NumberOfZones = 4,
                FirmwareVersion = "1.0.0",
                Loops = new ObservableCollection<Loop>()
            };

            // Add default loops
            for (int i = 1; i <= 2; i++)
            {
                newPanel.Loops.Add(new Loop
                {
                    LoopNumber = i,
                    LoopName = "Loop " + i,
                    LoopProtocol = "Standard",
                    NumberOfDevices = 0,
                    Devices = new ObservableCollection<LoopDevice>()
                });
            }

            // Create the ViewModel with tree structure
            var panelViewModel = new BlackBoxControlPanelViewModel(newPanel);

            // Add to collection
            BlackBoxControlPanels.Add(panelViewModel);

            // Select the new panel
            SelectedForm = panelViewModel;
        }

        public void DisplayDetails()
        {
            // ✔ INPUT/OUTPUT CHILD NODES (Handle these FIRST)
            if (SelectedNode is TreeNodeViewModel treeNode && treeNode.Tag != null)
            {
                var parentCE = FindParentCauseAndEffectForNode(treeNode);

                if (parentCE != null)
                {
                    // Determine which type of input/output was selected
                    switch (treeNode.Tag)
                    {
                        case TimeOfDayInput todInput:
                            SelectedForm = new TimeOfDayInputViewModel(todInput, parentCE);
                            return;

                        case DateTimeInput dtInput:
                            SelectedForm = new DateTimeInputViewModel(dtInput, parentCE);
                            return;

                        case DeviceInput devInput:
                            SelectedForm = new DeviceInputViewModel(devInput, parentCE);
                            return;

                        case SendTextOutput textOutput:
                            SelectedForm = new SendTextOutputViewModel(textOutput, parentCE);
                            return;

                        case SendEmailOutput emailOutput:
                            SelectedForm = new SendEmailOutputViewModel(emailOutput, parentCE);
                            return;

                        case SendApiOutput apiOutput:
                            SelectedForm = new SendApiOutputViewModel(apiOutput, parentCE);
                            return;

                        case DeviceOutput devOutput:
                            SelectedForm = new DeviceOutputViewModel(devOutput, parentCE);
                            return;

                        case ReceiveApiInput raiInput:
                            SelectedForm = new ReceiveApiInputViewModel(raiInput, parentCE);
                            return;
                    }
                }
            }

            // ✔ DEVICE NODE
            if (SelectedNode is LoopDeviceViewModel deviceVM)
            {
                SelectedForm = deviceVM;
                return;
            }
            // ✔ LOOP NODE
            if (SelectedNode is LoopViewModel loopVM)
            {
                SelectedForm = loopVM;
                return;
            }
            // ✔ PANEL NODE
            if (SelectedNode is BlackBoxControlPanelViewModel panelVM)
            {
                SelectedForm = panelVM;
                return;
            }
            // ✔ BUS NODE
            if (SelectedNode is BusViewModel busVM)
            {
                SelectedForm = busVM;
                return;
            }

            if (SelectedNode is BusNodeViewModel busNodeVM)
            {
                SelectedForm = busNodeVM;
                return;
            }
            // ✔ C&E NODE
            if (SelectedNode is CauseAndEffectViewModel ceVM)
            {
                SelectedForm = ceVM;
                return;
            }
            // ✔ C&E LIST NODE
            if (SelectedNode is CauseAndEffectsListViewModel ceList)
            {
                SelectedForm = ceList;
                return;
            }
            // ✔ GENERIC TREE NODE (containers)
            if (SelectedNode is TreeNodeViewModel nodeVM)
            {
                switch (nodeVM.NodeType)
                {
                    case TreeNodeType.CauseEffectsContainer:
                    {
                        var parentPanel = FindParentPanelViewModel(nodeVM);
                        if (parentPanel != null)
                        {
                            var ceListVM = new CauseAndEffectsListViewModel(nodeVM);
                            ceListVM.RequestEditForm += (sender, ceToEdit) =>
                            {
                                var editVM = new CauseAndEffectViewModel(
                                    ceToEdit.CauseEffect,
                                    parentPanel.Panel.Loops,
                                    GetBussesFromPanel(parentPanel));
                                SelectedForm = editVM;
                            };
                            SelectedForm = ceListVM;
                        }
                        return;
                    }
                    case TreeNodeType.BussesContainer:
                        SelectedForm = null;
                        return;
                }
            }
            // ✔ DEFAULT CASE
            SelectedForm = null;
        }

        // Add this helper method to MainViewModel
        private CauseAndEffectViewModel? FindParentCauseAndEffectForNode(TreeNodeViewModel treeNode)
        {
            // Search through all fire panels for the C&E that contains this input/output
            foreach (var panel in BlackBoxControlPanels)
            {
                // Find the Cause and Effects container
                var ceContainer = panel.Children
                    .OfType<TreeNodeViewModel>()
                    .FirstOrDefault(n => n.NodeType == TreeNodeType.CauseEffectsContainer);

                if (ceContainer != null)
                {
                    // Look through all C&E entries in the container
                    foreach (var ceNode in ceContainer.Children.OfType<CauseAndEffectViewModel>())
                    {
                        // Check if this C&E contains the input/output
                        if (ceNode.CauseEffect.Inputs.Contains(treeNode.Tag) ||
                            ceNode.CauseEffect.Outputs.Contains(treeNode.Tag))
                        {
                            return ceNode;
                        }
                    }
                }
            }

            return null;
        }

        private ObservableCollection<Bus> GetBussesFromPanel(BlackBoxControlPanelViewModel panelVM)
        {
            var bussesContainer = panelVM.Children
                .OfType<TreeNodeViewModel>()
                .FirstOrDefault(n => n.NodeType == TreeNodeType.BussesContainer);

            if (bussesContainer != null)
            {
                return new ObservableCollection<Bus>(
                    bussesContainer.Children.OfType<BusViewModel>().Select(bvm => bvm.Bus)
                );
            }
            return new ObservableCollection<Bus>();
        }

        // Helper method to find the parent panel
        private BlackBoxControlPanelViewModel? FindParentPanelViewModel(TreeNodeViewModel childNode)
        {
            foreach (var panel in BlackBoxControlPanels)
            {
                if (panel.Children.Contains(childNode))
                {
                    return panel;
                }
            }
            return null;
        }

        public void AddDeviceToSelectedItem(LoopDevice device)
        {
            Loop? targetLoop = null;
            LoopViewModel? targetLoopVM = null;

            // Determine the target loop based on selected node
            if (SelectedNode is Loop loop)
            {
                targetLoop = loop;
                // Find the corresponding LoopViewModel
                targetLoopVM = FindLoopViewModel(loop);
            }
            else if (SelectedNode is LoopViewModel loopVM)
            {
                targetLoop = loopVM.Loop;
                targetLoopVM = loopVM;
            }
            else if (SelectedNode is LoopDevice existingDevice)
            {
                // Find the loop that contains this device
                targetLoop = FindLoopContainingDevice(existingDevice);
                if (targetLoop != null)
                {
                    targetLoopVM = FindLoopViewModel(targetLoop);
                }
            }

            if (targetLoop != null)
            {
                // Find the next available address
                byte nextAddress = 1;
                if (targetLoop.Devices.Any())
                {
                    var usedAddresses = targetLoop.Devices.Select(d => d.Address).ToList();
                    nextAddress = Enumerable.Range(1, byte.MaxValue)
                                            .Select(x => (byte)x)
                                            .Where(addr => !usedAddresses.Contains(addr))
                                            .FirstOrDefault();
                }

                // Create the new device with the determined address
                var newDevice = new LoopDevice
                {
                    Type = device.Type,
                    LocationText = device.LocationText,
                    Address = nextAddress,
                    SubAddresses = device.SubAddresses != null
                        ? new ObservableCollection<SubAddress>(device.SubAddresses)
                        : new ObservableCollection<SubAddress>(),
                    AnalogValue = device.AnalogValue,
                    DeviceThreshold = device.DeviceThreshold,
                    DeviceDaySensitivity = device.DeviceDaySensitivity,
                    DeviceNightSensitivity = device.DeviceNightSensitivity,
                    DeviceInputAction = device.DeviceInputAction,
                    DeviceActionMessage = device.DeviceActionMessage,
                    ImagePath = device.ImagePath
                };

                // Add to model
                targetLoop.Devices.Add(newDevice);
                targetLoop.NumberOfDevices = targetLoop.Devices.Count;

                // 🔥 ADD TO TREEVIEW HIERARCHY
                if (targetLoopVM != null)
                {
                    targetLoopVM.Children.Add(new LoopDeviceViewModel(newDevice));
                }

                // Refresh the display
                DisplayDetails();
            }
        }

        // Add this helper method
        private LoopViewModel? FindLoopViewModel(Loop loop)
        {
            foreach (var panel in BlackBoxControlPanels)
            {
                var loopsContainer = panel.Children
                    .OfType<TreeNodeViewModel>()
                    .FirstOrDefault(n => n.NodeType == TreeNodeType.LoopsContainer);

                if (loopsContainer != null)
                {
                    var loopVM = loopsContainer.Children
                        .OfType<LoopViewModel>()
                        .FirstOrDefault(lvm => lvm.Loop == loop);

                    if (loopVM != null)
                        return loopVM;
                }
            }
            return null;
        }

        // ADDED: Add bus node to selected bus
        public void AddBusNodeToSelectedBus(BusNode busNode)
        {
            Bus? targetBus = null;
            BusViewModel? targetBusVM = null;

            // Determine the target bus based on selected node
            if (SelectedNode is Bus bus)
            {
                targetBus = bus;
                // Find the BusViewModel that wraps this bus
                foreach (var panel in BlackBoxControlPanels)
                {
                    var bussesContainer = panel.Children
                        .OfType<TreeNodeViewModel>()
                        .FirstOrDefault(n => n.NodeType == TreeNodeType.BussesContainer);

                    if (bussesContainer != null)
                    {
                        targetBusVM = bussesContainer.Children
                            .OfType<BusViewModel>()
                            .FirstOrDefault(bvm => bvm.Bus == bus);

                        if (targetBusVM != null)
                            break;
                    }
                }
            }
            else if (SelectedNode is BusViewModel busVM)
            {
                targetBus = busVM.Bus;
                targetBusVM = busVM;
            }

            if (targetBus != null)
            {
                // Find the next available address
                byte nextAddress = 1;
                if (targetBus.Nodes.Any())
                {
                    var usedAddresses = targetBus.Nodes.Select(n => n.Address).ToList();
                    nextAddress = Enumerable.Range(1, byte.MaxValue)
                                            .Select(x => (byte)x)
                                            .Where(addr => !usedAddresses.Contains(addr))
                                            .FirstOrDefault();
                }

                // Create the new bus node with the determined address
                var newNode = new BusNode
                {
                    NodeNumber = targetBus.Nodes.Count + 1,
                    Name = busNode.Name,
                    LocationText = busNode.LocationText ?? "New Location",
                    Address = nextAddress,
                    ImagePath = busNode.ImagePath,
                    Inputs = new ObservableCollection<BusNodeIO>(busNode.Inputs ?? new ObservableCollection<BusNodeIO>()),
                    Outputs = new ObservableCollection<BusNodeIO>(busNode.Outputs ?? new ObservableCollection<BusNodeIO>())
                };

                // Add to bus model
                targetBus.Nodes.Add(newNode);
                targetBus.NumberOfNodes = targetBus.Nodes.Count;

                // Rebuild tree children to show the new node in the tree
                if (targetBusVM != null)
                {
                    targetBusVM.RebuildTreeChildren();
                }

                // Refresh the display
                DisplayDetails();

                System.Diagnostics.Debug.WriteLine($"Added bus node '{newNode.Name}' to bus '{targetBus.BusName}' at address {newNode.Address}");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("Could not find target bus to add node");
            }
        }

        /// <summary>
        /// Finds the loop that contains a specific device
        /// </summary>
        private Loop? FindLoopContainingDevice(LoopDevice device)
        {
            foreach (var panelVM in BlackBoxControlPanels)
            {
                if (panelVM.Panel != null && panelVM.Panel.Loops != null)
                {
                    foreach (var loop in panelVM.Panel.Loops)
                    {
                        if (loop.Devices != null && loop.Devices.Contains(device))
                        {
                            return loop;
                        }
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Adds a new loop to the selected panel
        /// </summary>
        public void AddLoop()
        {
            var panel = BlackBoxControlPanels.FirstOrDefault();
            if (panel == null)
                return;

            // Find the Loops container
            var loopsContainer = panel.Children
                .OfType<TreeNodeViewModel>()
                .FirstOrDefault(n => n.NodeType == TreeNodeType.LoopsContainer);

            if (loopsContainer != null)
            {
                int newLoopNumber = loopsContainer.Children.Count + 1;

                var newLoop = new Loop
                {
                    LoopNumber = newLoopNumber,
                    LoopName = "Loop " + newLoopNumber,
                    LoopProtocol = "Standard",
                    NumberOfDevices = 0,
                    Devices = new ObservableCollection<LoopDevice>()
                };

                var loopVM = new LoopViewModel(newLoop);
                loopsContainer.Children.Add(loopVM);

                // Also add to the BlackBoxControlPanel's Loops collection
                panel.Panel.Loops.Add(newLoop);
                panel.Panel.NumberOfLoops = panel.Panel.Loops.Count;
            }
        }

        /// <summary>
        /// Adds a new bus to the selected panel
        /// </summary>
        public void AddBus()
        {
            var panel = BlackBoxControlPanels.FirstOrDefault();
            if (panel == null)
                return;

            var bussesContainer = panel.Children
                .OfType<TreeNodeViewModel>()
                .FirstOrDefault(n => n.NodeType == TreeNodeType.BussesContainer);

            if (bussesContainer != null)
            {
                int newBusNumber = bussesContainer.Children.Count + 1;

                // Create a new Bus model
                var busModel = new Bus
                {
                    BusNumber = newBusNumber,
                    BusName = "Bus " + newBusNumber,
                    BusType = "RS485",
                    Nodes = new ObservableCollection<BusNode>()
                };

                // ✔ Correct constructor usage
                var newBusVM = new BusViewModel(busModel);

                // Add to tree
                bussesContainer.Children.Add(newBusVM);
            }
        }

        /// <summary>
        /// Adds a new Cause and Effect entry
        /// </summary>
        public void AddCauseEffect()
        {
            var panel = BlackBoxControlPanels.FirstOrDefault();
            if (panel == null)
                return;

            var ceContainer = panel.Children
                .OfType<TreeNodeViewModel>()
                .FirstOrDefault(n => n.NodeType == TreeNodeType.CauseEffectsContainer);

            if (ceContainer != null)
            {
                // Get loops and busses from the panel for the C&E configuration
                var loops = panel.Panel?.Loops ?? new ObservableCollection<Loop>();

                // Get busses from the busses container
                var bussesContainer = panel.Children
                    .OfType<TreeNodeViewModel>()
                    .FirstOrDefault(n => n.NodeType == TreeNodeType.BussesContainer);

                var busses = bussesContainer?.Children
                    .OfType<BusViewModel>()
                    .Select(bvm => bvm.Bus)
                    .ToList() ?? new List<Bus>();

                // CORRECTED: Create the CauseAndEffect model first
                var newCauseEffect = new CauseAndEffect
                {
                    Name = "New Cause & Effect",
                    IsEnabled = true,
                    LogicGate = LogicGate.AND
                };

                // Create new C&E with required parameters
                var newCE = new CauseAndEffectViewModel(newCauseEffect, loops, busses);
                ceContainer.Children.Add(newCE);
            }
        }
    }
}

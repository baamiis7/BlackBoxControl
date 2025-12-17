using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Linq;
using System;
using System.Windows.Media.Imaging;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Threading;
using System.Windows.Shapes;
using BlackBoxControl.Models;
using System.Collections.ObjectModel;
using BlackBoxControl.Properties;
using BlackBoxControl.ViewModels;

namespace BlackBoxControl
{
    public partial class MainWindow : Window
    {
        private Window _dragPreviewWindow; // Window to display the drag visual
        private DispatcherTimer _positionUpdateTimer; // Timer to update the drag visual position

        public MainWindow()
        {
            InitializeComponent();
           // LoadThemeFromSettings();
        }
        private void TreeView_Loaded(object sender, RoutedEventArgs e)
        {
            var treeView = sender as TreeView;
            if (treeView == null) return;

            System.Diagnostics.Debug.WriteLine("=== TreeView Structure ===");
            foreach (var item in treeView.Items)
            {
                PrintTreeItem(item, 0);
            }
        }

        private void PrintTreeItem(object item, int level)
        {
            var indent = new string(' ', level * 2);

            if (item is CauseAndEffectViewModel cevm)
            {
                System.Diagnostics.Debug.WriteLine($"{indent}C&E: {cevm.DisplayName}, Children: {cevm.Children?.Count ?? 0}");
                if (cevm.Children != null)
                {
                    foreach (var child in cevm.Children)
                    {
                        PrintTreeItem(child, level + 1);
                    }
                }
            }
            else if (item is TreeNodeViewModel tnvm)
            {
                System.Diagnostics.Debug.WriteLine($"{indent}TreeNode: {tnvm.DisplayName}, Children: {tnvm.Children?.Count ?? 0}");
                if (tnvm.Children != null)
                {
                    foreach (var child in tnvm.Children)
                    {
                        PrintTreeItem(child, level + 1);
                    }
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"{indent}Unknown: {item?.GetType().Name}");
            }
        }
        private void LoadThemeFromSettings()
        {
            string theme = Properties.Settings.Default.DefaultTheme;
            LoadTheme(theme);
        }

        private void LoadTheme(string themeFileName)
        {
            // Clear existing merged dictionaries
            Application.Current.Resources.MergedDictionaries.Clear();

            // Load the theme from the file
            var theme = new ResourceDictionary
            {
                Source = new Uri($"pack://application:,,,/BlackBoxControl;component/Themes/{themeFileName}", UriKind.Absolute)
            };
            Application.Current.Resources.MergedDictionaries.Add(theme);

            // Save the current theme in settings
            Settings.Default.DefaultTheme = themeFileName;
            Settings.Default.Save();
        }

        // This method can be bound to a ComboBox or any other control that allows theme selection

        private void Device_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed || e.RightButton == MouseButtonState.Pressed)
            {
                var stackPanel = sender as StackPanel;
                if (stackPanel != null)
                {
                    var device = stackPanel.DataContext as Models.LoopDevice;
                    if (device != null)
                    {
                        // Start the drag operation
                        DragDrop.DoDragDrop(stackPanel, device, DragDropEffects.All);

                    }
                }
            }
        }

        private void Device_DragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(Models.LoopDevice)))
            {
                e.Effects = DragDropEffects.Copy;
               // e.Handled = true;
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }
        }

        private void Device_Drop(object sender, DragEventArgs e)
        {
            e.Handled = true; // Prevent further propagation of the event
            // Check if the dragged data is of type LoopDevice
            if (e.Data.GetDataPresent(typeof(Models.LoopDevice)))
            {
                var droppedDevice = e.Data.GetData(typeof(Models.LoopDevice)) as Models.LoopDevice;
                var viewModel = DataContext as ViewModels.MainViewModel;

                if (viewModel != null && droppedDevice != null)
                {
                    // Add the device to the selected loop
                    viewModel.AddDeviceToSelectedItem(droppedDevice);
                }
            }

        }
        private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            var viewModel = DataContext as ViewModels.MainViewModel;
            var selectedItem = e.NewValue;

            if (viewModel == null) return;

            // Handle C&E child nodes (inputs/outputs) FIRST before handling the C&E itself
            if (selectedItem is TreeNodeViewModel treeNode && treeNode.Tag != null)
            {
                var parentCE = FindParentCauseAndEffect(treeNode);

                if (parentCE != null)
                {
                    // Determine which type of input/output was selected
                    switch (treeNode.Tag)
                    {
                        case TimeOfDayInput todInput:
                            viewModel.SelectedForm = new TimeOfDayInputViewModel(todInput, parentCE);
                            viewModel.SelectedNode = selectedItem;
                            return;

                        case DateTimeInput dtInput:
                            viewModel.SelectedForm = new DateTimeInputViewModel(dtInput, parentCE);
                            viewModel.SelectedNode = selectedItem;
                            return;

                        case DeviceInput devInput:
                            viewModel.SelectedForm = new DeviceInputViewModel(devInput, parentCE);
                            viewModel.SelectedNode = selectedItem;
                            return;

                        case SendTextOutput textOutput:
                            viewModel.SelectedForm = new SendTextOutputViewModel(textOutput, parentCE);
                            viewModel.SelectedNode = selectedItem;
                            return;

                        case SendEmailOutput emailOutput:
                            viewModel.SelectedForm = new SendEmailOutputViewModel(emailOutput, parentCE);
                            viewModel.SelectedNode = selectedItem;
                            return;

                        case SendApiOutput apiOutput:
                            viewModel.SelectedForm = new SendApiOutputViewModel(apiOutput, parentCE);
                            viewModel.SelectedNode = selectedItem;
                            return;
                        case ReceiveApiInput raiInput:
                            viewModel.SelectedForm = new ReceiveApiInputViewModel(raiInput, parentCE);
                            viewModel.SelectedNode = selectedItem;
                            return;
                        case DeviceOutput devOutput:
                            viewModel.SelectedForm = new DeviceOutputViewModel(devOutput, parentCE);
                            viewModel.SelectedNode = selectedItem;
                            return;
                    }
                }
            }

            // Handle C&E selection with diagnostic code
            if (selectedItem is CauseAndEffectViewModel cevm)
            {
                System.Diagnostics.Debug.WriteLine($"Selected C&E: {cevm.DisplayName}");
                System.Diagnostics.Debug.WriteLine($"C&E has {cevm.Children?.Count ?? 0} children");

                if (cevm.Children != null)
                {
                    foreach (var child in cevm.Children)
                    {
                        System.Diagnostics.Debug.WriteLine($"  - Child: {child.DisplayName}, Type: {child.GetType().Name}, HasChildren: {child.Children?.Count ?? 0}");
                    }
                }
            }

            // Set the selected node for all other node types
            viewModel.SelectedNode = selectedItem;
        }

        // ADD THIS HELPER METHOD
        private CauseAndEffectViewModel FindParentCauseAndEffect(TreeNodeViewModel treeNode)
        {
            var viewModel = DataContext as ViewModels.MainViewModel;
            if (viewModel == null) return null;

            // Search through all fire panels for the C&E that contains this input/output
            foreach (var panel in viewModel.BlackBoxControlPanels)
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

        private void Device_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var viewModel = DataContext as ViewModels.MainViewModel;
            if (viewModel == null) return;

            if (sender is Image image && image.DataContext is LoopDevice selectedDevice)
            {
                Loop selectedLoop = null;
                LoopViewModel selectedLoopVM = null;

                // Check if the selected node is a Loop or LoopViewModel
                if (viewModel.SelectedNode is Loop loop)
                {
                    selectedLoop = loop;
                }
                else if (viewModel.SelectedNode is LoopViewModel loopVM)
                {
                    selectedLoop = loopVM.Loop;
                    selectedLoopVM = loopVM;
                }

                if (selectedLoop != null)
                {
                    if (selectedLoop.Devices == null)
                        selectedLoop.Devices = new ObservableCollection<LoopDevice>();

                    if (!selectedLoop.Devices.Any(d => d.Address == selectedDevice.Address))
                    {
                        // NEXT ADDRESS
                        byte nextAddress = 1;
                        if (selectedLoop.Devices.Any())
                        {
                            var used = selectedLoop.Devices.Select(d => d.Address).ToList();
                            nextAddress = Enumerable.Range(1, byte.MaxValue)
                                                    .Select(x => (byte)x)
                                                    .First(addr => !used.Contains(addr));
                        }

                        // CREATE MODEL DEVICE
                        var newDevice = new LoopDevice
                        {
                            Type = selectedDevice.Type,
                            LocationText = selectedDevice.LocationText,
                            Address = nextAddress,
                            SubAddresses = selectedDevice.SubAddresses != null
                                ? new ObservableCollection<SubAddress>(selectedDevice.SubAddresses)
                                : new ObservableCollection<SubAddress>(),
                            AnalogValue = selectedDevice.AnalogValue,
                            DeviceThreshold = selectedDevice.DeviceThreshold,
                            DeviceDaySensitivity = selectedDevice.DeviceDaySensitivity,
                            DeviceNightSensitivity = selectedDevice.DeviceNightSensitivity,
                            DeviceInputAction = selectedDevice.DeviceInputAction,
                            DeviceActionMessage = selectedDevice.DeviceActionMessage,
                            ImagePath = selectedDevice.ImagePath
                        };

                        // ADD TO MODEL
                        selectedLoop.Devices.Add(newDevice);
                        selectedLoop.NumberOfDevices = selectedLoop.Devices.Count;

                        // 🔥🔥🔥 ADD TO TREEVIEW HIERARCHY (THIS WAS MISSING)
                        if (selectedLoopVM != null)
                        {
                            selectedLoopVM.Children.Add(new LoopDeviceViewModel(newDevice));
                        }

                        // REFRESH DETAILS
                        viewModel.DisplayDetails();

                        MessageBox.Show(
                            $"Device '{newDevice.Type}' added to loop '{selectedLoop.LoopName}' at address {nextAddress}.",
                            "Success",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show(
                            $"A device with address '{selectedDevice.Address}' already exists in loop '{selectedLoop.LoopName}'.",
                            "Info",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);
                    }
                }
                else
                {
                    MessageBox.Show(
                        "Please select a Loop in the tree to add the device.",
                        "No Loop Selected",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                }
            }
        }



        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                DragMove();
        }

        private void btnMinimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }
        private void btnMaximise_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Maximized;
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}

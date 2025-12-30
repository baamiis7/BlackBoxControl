using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using BlackBoxControl.Helpers;
using BlackBoxControl.Models;
using BlackBoxControl.Services;
using BlackBoxControl.Views;
using Microsoft.Win32;

namespace BlackBoxControl.ViewModels
{
    public class MenuViewModel : INotifyPropertyChanged
    {
        private readonly MainViewModel _mainViewModel;
        private ObservableCollection<RecentProjectViewModel> _recentProjects;

        public ObservableCollection<RecentProjectViewModel> RecentProjects
        {
            get => _recentProjects;
            set
            {
                _recentProjects = value;
                OnPropertyChanged(nameof(RecentProjects));
            }
        }

        public ICommand NewProjectCommand { get; }
        public ICommand OpenProjectCommand { get; }
        public ICommand OpenRecentProjectCommand { get; }
        public ICommand ClearRecentProjectsCommand { get; }
        public ICommand CloseProjectCommand { get; }
        public ICommand SaveProjectCommand { get; }
        public ICommand SaveProjectAsCommand { get; }
        public ICommand ImportProjectCommand { get; }
        public ICommand ExportProjectCommand { get; }
        public ICommand ConnectToPanelCommand { get; }
        public ICommand DownloadProjectFromPanelCommand { get; }
        public ICommand UploadProjectToPanelCommand { get; }
        public ICommand ExitCommand { get; }
        public ICommand MonitorPanelCommand { get; }
        public ICommand BlueThemeCommand { get; }
        public ICommand GreenThemeCommand { get; }
        public ICommand DarkThemeCommand { get; }
        public ICommand DocumentationCommand { get; }
        public ICommand AboutCommand { get; }
        public ICommand UploadConfigurationCommand { get; }
        public ICommand DownloadFromPanelCommand { get; }
        public ICommand ResetSimulatorCommand { get; }

        public MenuViewModel(MainViewModel mainViewModel)
        {
            _mainViewModel = mainViewModel;
            RecentProjects = new ObservableCollection<RecentProjectViewModel>();
            LoadRecentProjects();

            NewProjectCommand = new RelayCommand(NewProject);
            OpenProjectCommand = new RelayCommand(OpenProject);
            OpenRecentProjectCommand = new RelayCommand<string>(OpenRecentProject);
            ClearRecentProjectsCommand = new RelayCommand(ClearRecentProjects);
            CloseProjectCommand = new RelayCommand(CloseProject);
            SaveProjectCommand = new RelayCommand(SaveProject, CanSaveProject);
            SaveProjectAsCommand = new RelayCommand(SaveProjectAs);
            ImportProjectCommand = new RelayCommand(ImportProject);
            ExportProjectCommand = new RelayCommand(ExportProject);
            ConnectToPanelCommand = new RelayCommand(ConnectToPanel);
            DownloadProjectFromPanelCommand = new RelayCommand(DownloadProjectFromPanel);
            UploadProjectToPanelCommand = new RelayCommand(UploadProjectToPanel);
            ExitCommand = new RelayCommand(ExitApplication);
            MonitorPanelCommand = new RelayCommand(MonitorPanel);
            BlueThemeCommand = new RelayCommand(() => ThemeManager.ChangeTheme(ThemeManager.Theme.Blue));
            GreenThemeCommand = new RelayCommand(() => ThemeManager.ChangeTheme(ThemeManager.Theme.Green));
            DarkThemeCommand = new RelayCommand(() => ThemeManager.ChangeTheme(ThemeManager.Theme.Dark));
            DocumentationCommand = new RelayCommand(Documentation);
            AboutCommand = new RelayCommand(About);
            UploadConfigurationCommand = new RelayCommand(ShowUploadDialog);
            DownloadFromPanelCommand = new RelayCommand(ShowDownloadDialog);
            ResetSimulatorCommand = new RelayCommand(ResetSimulator);
        }

        private void NewProject()
        {
            var result = MessageBox.Show(
                "Create a new project? Any unsaved changes will be lost.",
                "New Project",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                _mainViewModel.BlackBoxControlPanels.Clear();
                _mainViewModel.CurrentProjectPath = null;
                _mainViewModel.CreateNewProject();
                MessageBox.Show("New project created!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void OpenProject()
        {
            var openFileDialog = new OpenFileDialog
            {
                Title = "Open Fire Panel Project",
                Filter = "BAAMIIS Project Files (*.kbb)|*.kbb|All Files (*.*)|*.*",
                DefaultExt = ".kbb"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                OpenProjectFile(openFileDialog.FileName);
            }
        }

        private void OpenRecentProject(string projectPath)
        {
            if (string.IsNullOrEmpty(projectPath))
                return;

            if (!System.IO.File.Exists(projectPath))
            {
                var result = MessageBox.Show(
                    $"The file no longer exists:\n\n{projectPath}\n\nRemove from recent projects?",
                    "File Not Found",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    RecentProjectsManager.RemoveRecentProject(projectPath);
                    LoadRecentProjects();
                }
                return;
            }

            OpenProjectFile(projectPath);
        }

        private void OpenProjectFile(string filePath)
        {
            try
            {
                _mainViewModel.BlackBoxControlPanels.Clear();

                var loadedViewModel = ProjectService.LoadProject(filePath);

                foreach (var panel in loadedViewModel.BlackBoxControlPanels)
                {
                    _mainViewModel.BlackBoxControlPanels.Add(panel);
                }

                _mainViewModel.CurrentProjectPath = filePath;

                RecentProjectsManager.AddRecentProject(filePath);
                LoadRecentProjects();

                MessageBox.Show(
                    $"Project loaded successfully!\n\nFile: {filePath}",
                    "Success",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Failed to load project:\n\n{ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void LoadRecentProjects()
        {
            RecentProjects.Clear();
            var recentPaths = RecentProjectsManager.GetRecentProjects();

            foreach (var path in recentPaths)
            {
                RecentProjects.Add(new RecentProjectViewModel
                {
                    FilePath = path,
                    FileName = System.IO.Path.GetFileName(path),
                    OpenCommand = OpenRecentProjectCommand
                });
            }

            OnPropertyChanged(nameof(RecentProjects));
        }

        private void ClearRecentProjects()
        {
            var result = MessageBox.Show(
                "Clear all recent projects?",
                "Clear Recent Projects",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                RecentProjectsManager.ClearRecentProjects();
                LoadRecentProjects();
            }
        }

        private void CloseProject()
        {
            if (_mainViewModel.BlackBoxControlPanels.Count == 0)
            {
                MessageBox.Show("No project is currently open.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var result = MessageBox.Show(
                "Close current project? Any unsaved changes will be lost.",
                "Close Project",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                _mainViewModel.BlackBoxControlPanels.Clear();
                _mainViewModel.CurrentProjectPath = null;
                _mainViewModel.SelectedForm = null;
                MessageBox.Show("Project closed.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private bool CanSaveProject()
        {
            return _mainViewModel?.BlackBoxControlPanels?.Count > 0;
        }

        private void SaveProject()
        {
            if (string.IsNullOrEmpty(_mainViewModel.CurrentProjectPath))
            {
                SaveProjectAs();
            }
            else
            {
                try
                {
                    ProjectService.SaveProject(_mainViewModel.CurrentProjectPath, _mainViewModel);

                    RecentProjectsManager.AddRecentProject(_mainViewModel.CurrentProjectPath);
                    LoadRecentProjects();

                    MessageBox.Show(
                        $"Project saved successfully!\n\nFile: {_mainViewModel.CurrentProjectPath}",
                        "Success",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        $"Failed to save project:\n\n{ex.Message}",
                        "Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            }
        }

        private void SaveProjectAs()
        {
            var saveFileDialog = new SaveFileDialog
            {
                Title = "Save Fire Panel Project As",
                Filter = "BAAMIIS Project Files (*.kbb)|*.kbb|All Files (*.*)|*.*",
                DefaultExt = ".kbb",
                FileName = "BlackBoxControlPanel_Project.kbb"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    ProjectService.SaveProject(saveFileDialog.FileName, _mainViewModel);
                    _mainViewModel.CurrentProjectPath = saveFileDialog.FileName;

                    RecentProjectsManager.AddRecentProject(saveFileDialog.FileName);
                    LoadRecentProjects();

                    MessageBox.Show(
                        $"Project saved successfully!\n\nFile: {saveFileDialog.FileName}",
                        "Success",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        $"Failed to save project:\n\n{ex.Message}",
                        "Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            }
        }

        private void ImportProject()
        {
            MessageBox.Show("Import Project - Not yet implemented", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ExportProject()
        {
            MessageBox.Show("Export Project - Not yet implemented", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ConnectToPanel()
        {
            MessageBox.Show("Connect to Panel - Not yet implemented", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void DownloadProjectFromPanel()
        {
            MessageBox.Show("Download from Panel - Not yet implemented", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void UploadProjectToPanel()
        {
            MessageBox.Show("Upload to Panel - Not yet implemented", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ExitApplication()
        {
            Application.Current.Shutdown();
        }

        private void MonitorPanel()
        {
            MessageBox.Show("Monitor Panel - Not yet implemented", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void Documentation()
        {
            MessageBox.Show("Documentation - Not yet implemented", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void About()
        {
            MessageBox.Show(
                "Fire Panel Simulation v1.0\n\n" +
                "BAAMIIS LTD\n" +
                "Professional Fire Alarm Control Panel Configuration Software\n\n" +
                "© 2025 All Rights Reserved",
                "About",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        private void ShowUploadDialog()
        {
            if (_mainViewModel.BlackBoxControlPanels == null || _mainViewModel.BlackBoxControlPanels.Count == 0)
            {
                MessageBox.Show("Please open or create a project first.", "No Project",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var projectData = new ProjectData
            {
                ProjectName = "Current Project",
                BlackBoxControlPanels = new List<BlackBoxControlPanelData>()
            };

            foreach (var panelViewModel in _mainViewModel.BlackBoxControlPanels)
            {
                var panelData = new BlackBoxControlPanelData
                {
                    PanelName = panelViewModel.Panel.PanelName,
                    Location = panelViewModel.Panel.Location,
                    PanelAddress = panelViewModel.Panel.PanelAddress,
                    NumberOfLoops = panelViewModel.Panel.NumberOfLoops,
                    NumberOfZones = panelViewModel.Panel.NumberOfZones,
                    FirmwareVersion = panelViewModel.Panel.FirmwareVersion,
                    Loops = new List<LoopData>(),
                    Busses = new List<BusData>(),
                    CauseAndEffects = new List<CauseAndEffectData>()
                };

                // CONVERT LOOPS
                if (panelViewModel.Panel.Loops != null)
                {
                    foreach (var loop in panelViewModel.Panel.Loops)
                    {
                        var loopData = new LoopData
                        {
                            LoopNumber = loop.LoopNumber,
                            LoopName = loop.LoopName,
                            Devices = new List<LoopDeviceData>()
                        };

                        if (loop.Devices != null)
                        {
                            foreach (var device in loop.Devices)
                            {
                                loopData.Devices.Add(new LoopDeviceData
                                {
                                    Address = device.Address,
                                    Type = device.Type,
                                    LocationText = device.LocationText,
                                    Zone = device.Zone,
                                    ImagePath = device.ImagePath
                                });
                            }
                        }

                        panelData.Loops.Add(loopData);
                    }
                }

                // CONVERT BUSES
                var bussesContainer = panelViewModel.Children
                    .OfType<TreeNodeViewModel>()
                    .FirstOrDefault(n => n.NodeType == TreeNodeType.BussesContainer);

                if (bussesContainer != null)
                {
                    foreach (var busVM in bussesContainer.Children.OfType<BusViewModel>())
                    {
                        var busData = new BusData
                        {
                            BusNumber = busVM.Bus.BusNumber,
                            BusName = busVM.Bus.BusName,
                            BusType = busVM.Bus.BusType,
                            Nodes = new List<BusNodeData>()
                        };

                        if (busVM.Bus.Nodes != null)
                        {
                            foreach (var node in busVM.Bus.Nodes)
                            {
                                busData.Nodes.Add(new BusNodeData
                                {
                                    Address = node.Address,
                                    Name = node.Name,
                                    LocationText = node.LocationText,
                                    ImagePath = node.ImagePath
                                });
                            }
                        }

                        panelData.Busses.Add(busData);
                    }
                }

                // 🔥 CONVERT CAUSE & EFFECTS
                var ceContainer = panelViewModel.Children
                    .OfType<TreeNodeViewModel>()
                    .FirstOrDefault(n => n.NodeType == TreeNodeType.CauseEffectsContainer);

                if (ceContainer != null)
                {
                    System.Diagnostics.Debug.WriteLine($"[MenuViewModel] Found C&E container with {ceContainer.Children.Count} children");

                    foreach (var ceVM in ceContainer.Children.OfType<CauseAndEffectViewModel>())
                    {
                        var ceData = new CauseAndEffectData
                        {
                            Name = ceVM.CauseEffect.Name,
                            LogicGate = ceVM.CauseEffect.LogicGate.ToString(),
                            IsEnabled = ceVM.CauseEffect.IsEnabled,
                            Inputs = new List<CauseInputData>(),
                            Outputs = new List<EffectOutputData>()
                        };

                        // Convert Inputs
                        if (ceVM.CauseEffect.Inputs != null)
                        {
                            foreach (var input in ceVM.CauseEffect.Inputs)
                            {
                                var inputData = ConvertCauseInput(input);
                                if (inputData != null)
                                {
                                    ceData.Inputs.Add(inputData);
                                }
                            }
                        }

                        // Convert Outputs
                        if (ceVM.CauseEffect.Outputs != null)
                        {
                            foreach (var output in ceVM.CauseEffect.Outputs)
                            {
                                var outputData = ConvertEffectOutput(output);
                                if (outputData != null)
                                {
                                    ceData.Outputs.Add(outputData);
                                }
                            }
                        }

                        panelData.CauseAndEffects.Add(ceData);

                        System.Diagnostics.Debug.WriteLine(
                            $"[MenuViewModel] Converted C&E: {ceData.Name}, Inputs={ceData.Inputs.Count}, Outputs={ceData.Outputs.Count}");
                    }
                }

                projectData.BlackBoxControlPanels.Add(panelData);
            }

            var viewModel = new UploadConfigurationViewModel(projectData);
            var dialog = new UploadConfigurationDialog(viewModel);
            dialog.Owner = Application.Current.MainWindow;
            dialog.ShowDialog();
        }

        // 🔥 NEW: Helper method to convert CauseInput to CauseInputData
        private CauseInputData ConvertCauseInput(CauseInput input)
        {
            if (input == null)
                return null;

            var inputData = new CauseInputData();

            if (input is DeviceInput deviceInput)
            {
                inputData.InputType = "Device";
                inputData.DeviceId = deviceInput.DeviceId;
                inputData.Type = deviceInput.Type;
                inputData.LocationText = deviceInput.LocationText;
                inputData.ImagePath = deviceInput.ImagePath;
            }
            else if (input is TimeOfDayInput timeInput)
            {
                inputData.InputType = "TimeOfDay";
                inputData.StartTime = timeInput.StartTime.ToString(@"hh\:mm");
                inputData.EndTime = timeInput.EndTime.ToString(@"hh\:mm");
            }
            else if (input is DateTimeInput dateTimeInput)
            {
                inputData.InputType = "DateTime";
                inputData.TriggerDateTime = dateTimeInput.TriggerDateTime;
            }
            else if (input is ReceiveApiInput apiInput)
            {
                inputData.InputType = "ReceiveApi";
                inputData.ListenUrl = apiInput.ListenUrl;
                inputData.HttpMethod = apiInput.HttpMethod;
                inputData.ExpectedPath = apiInput.ExpectedPath;
                inputData.AuthToken = apiInput.AuthToken;
            }

            return inputData;
        }

        // 🔥 NEW: Helper method to convert EffectOutput to EffectOutputData
        private EffectOutputData ConvertEffectOutput(EffectOutput output)
        {
            if (output == null)
                return null;

            var outputData = new EffectOutputData();

            if (output is DeviceOutput deviceOutput)
            {
                outputData.OutputType = "Device";
                outputData.DeviceId = deviceOutput.DeviceId;
                outputData.Type = deviceOutput.Type;
                outputData.LocationText = deviceOutput.LocationText;
                outputData.ImagePath = deviceOutput.ImagePath;
            }
            else if (output is SendTextOutput textOutput)
            {
                outputData.OutputType = "SendText";
                outputData.PhoneNumber = textOutput.PhoneNumber;
                outputData.Message = textOutput.Message;
            }
            else if (output is SendEmailOutput emailOutput)
            {
                outputData.OutputType = "SendEmail";
                outputData.EmailAddress = emailOutput.EmailAddress;
                outputData.Subject = emailOutput.Subject;
                outputData.Body = emailOutput.Body;
            }
            else if (output is SendApiOutput apiOutput)
            {
                outputData.OutputType = "SendApi";
                outputData.ApiUrl = apiOutput.ApiUrl;
                outputData.HttpMethod = apiOutput.HttpMethod;
                outputData.ContentType = apiOutput.ContentType;
                outputData.RequestBody = apiOutput.RequestBody;
            }

            return outputData;
        }

        private void ShowDownloadDialog()
        {
            var viewModel = new DownloadConfigurationViewModel();

            // Subscribe to download completed event
            viewModel.DownloadCompleted += (projectData) =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    // Clear existing panels
                    _mainViewModel.BlackBoxControlPanels.Clear();

                    // Convert downloaded data to ViewModels and add to tree
                    foreach (var panelData in projectData.BlackBoxControlPanels)
                    {
                        var panel = ConvertDataToPanel(panelData);
                        var panelViewModel = new BlackBoxControlPanelViewModel(panel);
                        panelViewModel.RebuildTree();
                        _mainViewModel.BlackBoxControlPanels.Add(panelViewModel);
                    }

                    MessageBox.Show(
                        $"Configuration downloaded successfully!\n\nPanels: {projectData.BlackBoxControlPanels.Count}\nLoops: {projectData.BlackBoxControlPanels.Sum(p => p.Loops.Count)}\nDevices: {projectData.BlackBoxControlPanels.Sum(p => p.Loops.Sum(l => l.Devices.Count))}\nBuses: {projectData.BlackBoxControlPanels.Sum(p => p.Busses.Count)}\nBus Nodes: {projectData.BlackBoxControlPanels.Sum(p => p.Busses.Sum(b => b.Nodes.Count))}",
                        "Download Complete",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                });
            };

            viewModel.RequestClose += () => Application.Current.Dispatcher.Invoke(() =>
            {
                // Dialog will close itself
            });

            var dialog = new DownloadConfigurationDialog(viewModel);
            dialog.Owner = Application.Current.MainWindow;
            dialog.ShowDialog();
        }

        private void ResetSimulator()
        {
            var result = MessageBox.Show(
                "Reset the virtual ESP32 simulator?\n\nThis will clear all stored configuration data.",
                "Reset Simulator",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                ESP32SimulatorManager.Reset();
                MessageBox.Show("Simulator reset successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private BlackBoxControlPanel ConvertDataToPanel(BlackBoxControlPanelData data)
        {
            var panel = new BlackBoxControlPanel
            {
                PanelName = data.PanelName,
                Location = data.Location,
                PanelAddress = data.PanelAddress,
                NumberOfLoops = data.NumberOfLoops,
                NumberOfZones = data.NumberOfZones,
                FirmwareVersion = data.FirmwareVersion ?? "1.0.0",
                Loops = new ObservableCollection<Loop>(),
                Busses = new ObservableCollection<Bus>()
            };

            // Convert loops
            if (data.Loops != null)
            {
                foreach (var loopData in data.Loops)
                {
                    var loop = new Loop
                    {
                        LoopNumber = loopData.LoopNumber,
                        LoopName = loopData.LoopName,
                        NumberOfDevices = loopData.Devices?.Count ?? 0,
                        Devices = new ObservableCollection<LoopDevice>()
                    };

                    // Convert devices
                    if (loopData.Devices != null)
                    {
                        foreach (var deviceData in loopData.Devices)
                        {
                            var device = new LoopDevice
                            {
                                Address = (byte)deviceData.Address,
                                Type = deviceData.Type,
                                LocationText = deviceData.LocationText,
                                Zone = deviceData.Zone,
                                ImagePath = deviceData.ImagePath ?? $"/Images/{deviceData.Type.Replace(" ", "_")}.png",
                                SubAddresses = new ObservableCollection<SubAddress>()
                            };

                            loop.Devices.Add(device);
                        }
                    }

                    panel.Loops.Add(loop);
                }
            }

            // Convert buses
            if (data.Busses != null)
            {
                foreach (var busData in data.Busses)
                {
                    var bus = new Bus
                    {
                        BusNumber = busData.BusNumber,
                        BusName = busData.BusName,
                        BusType = busData.BusType ?? "RS485",
                        NumberOfNodes = busData.Nodes?.Count ?? 0,
                        Nodes = new ObservableCollection<BusNode>()
                    };

                    // Convert bus nodes
                    if (busData.Nodes != null)
                    {
                        foreach (var nodeData in busData.Nodes)
                        {
                            var node = new BusNode
                            {
                                Address = (byte)nodeData.Address,
                                Name = nodeData.Name,
                                LocationText = nodeData.LocationText ?? "",
                                ImagePath = nodeData.ImagePath ?? $"/BusImages/{nodeData.Name.Replace(" ", "_")}.png",
                                Inputs = new ObservableCollection<BusNodeIO>(),
                                Outputs = new ObservableCollection<BusNodeIO>()
                            };

                            bus.Nodes.Add(node);
                        }
                    }

                    panel.Busses.Add(bus);
                }
            }

            // Convert cause and effects
            // Convert cause and effects
            System.Diagnostics.Debug.WriteLine($"[ConvertDataToPanel] CauseAndEffects data count: {data.CauseAndEffects?.Count ?? 0}");

            if (data.CauseAndEffects != null && data.CauseAndEffects.Count > 0)
            {
                panel.CauseAndEffects = new ObservableCollection<CauseAndEffect>();

                System.Diagnostics.Debug.WriteLine($"[ConvertDataToPanel] Converting {data.CauseAndEffects.Count} C&Es");

                foreach (var ceData in data.CauseAndEffects)
                {
                    System.Diagnostics.Debug.WriteLine($"[ConvertDataToPanel] Converting C&E: {ceData.Name}");

                    var causeEffect = new CauseAndEffect
                    {
                        Name = ceData.Name,
                        IsEnabled = ceData.IsEnabled,
                        LogicGate = Enum.Parse<LogicGate>(ceData.LogicGate),
                        Inputs = new ObservableCollection<CauseInput>(),
                        Outputs = new ObservableCollection<EffectOutput>()
                    };

                    // Convert inputs
                    if (ceData.Inputs != null)
                    {
                        System.Diagnostics.Debug.WriteLine($"[ConvertDataToPanel] Converting {ceData.Inputs.Count} inputs");
                        foreach (var inputData in ceData.Inputs)
                        {
                            var input = ConvertDataToCauseInput(inputData);
                            if (input != null)
                            {
                                causeEffect.Inputs.Add(input);
                            }
                        }
                    }

                    // Convert outputs
                    if (ceData.Outputs != null)
                    {
                        System.Diagnostics.Debug.WriteLine($"[ConvertDataToPanel] Converting {ceData.Outputs.Count} outputs");
                        foreach (var outputData in ceData.Outputs)
                        {
                            var output = ConvertDataToEffectOutput(outputData);
                            if (output != null)
                            {
                                causeEffect.Outputs.Add(output);
                            }
                        }
                    }

                    panel.CauseAndEffects.Add(causeEffect);
                    System.Diagnostics.Debug.WriteLine($"[ConvertDataToPanel] Added C&E to panel, panel now has {panel.CauseAndEffects.Count} C&Es");
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("[ConvertDataToPanel] No C&E data to convert");
            }

            return panel;

            return panel;
        }
        private CauseInput ConvertDataToCauseInput(CauseInputData data)
        {
            if (data == null)
                return null;

            switch (data.InputType)
            {
                case "Device":
                    return new DeviceInput
                    {
                        DeviceId = data.DeviceId,
                        Type = data.Type,
                        LocationText = data.LocationText,
                        ImagePath = data.ImagePath
                    };

                case "TimeOfDay":
                    return new TimeOfDayInput
                    {
                        StartTime = TimeSpan.Parse(data.StartTime ?? "00:00"),
                        EndTime = TimeSpan.Parse(data.EndTime ?? "00:00")
                    };

                case "DateTime":
                    return new DateTimeInput
                    {
                        TriggerDateTime = data.TriggerDateTime ?? DateTime.Now
                    };

                case "ReceiveApi":
                    return new ReceiveApiInput
                    {
                        ListenUrl = data.ListenUrl,
                        HttpMethod = data.HttpMethod,
                        ExpectedPath = data.ExpectedPath,
                        AuthToken = data.AuthToken
                    };

                default:
                    return null;
            }
        }

        private EffectOutput ConvertDataToEffectOutput(EffectOutputData data)
        {
            if (data == null)
                return null;

            switch (data.OutputType)
            {
                case "Device":
                    return new DeviceOutput
                    {
                        DeviceId = data.DeviceId,
                        Type = data.Type,
                        LocationText = data.LocationText,
                        ImagePath = data.ImagePath
                    };

                case "SendText":
                    return new SendTextOutput
                    {
                        PhoneNumber = data.PhoneNumber,
                        Message = data.Message
                    };

                case "SendEmail":
                    return new SendEmailOutput
                    {
                        EmailAddress = data.EmailAddress,
                        Subject = data.Subject,
                        Body = data.Body
                    };

                case "SendApi":
                    return new SendApiOutput
                    {
                        ApiUrl = data.ApiUrl,
                        HttpMethod = data.HttpMethod,
                        ContentType = data.ContentType,
                        RequestBody = data.RequestBody
                    };

                default:
                    return null;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class RecentProjectViewModel
    {
        public string FilePath { get; set; }
        public string FileName { get; set; }
        public ICommand OpenCommand { get; set; }
    }
    // 🔥 NEW: Helper methods to convert downloaded data back to ViewModels


    }

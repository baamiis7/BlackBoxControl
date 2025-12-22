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
                    Busses = new List<BusData>()
                };

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

                projectData.BlackBoxControlPanels.Add(panelData);
            }

            var viewModel = new UploadConfigurationViewModel(projectData);
            var dialog = new UploadConfigurationDialog(viewModel);
            dialog.Owner = Application.Current.MainWindow;
            dialog.ShowDialog();
        }

        private void ShowDownloadDialog()
        {
            var viewModel = new DownloadConfigurationViewModel();

            viewModel.DownloadCompleted += (projectData) =>
            {
                _mainViewModel.BlackBoxControlPanels.Clear();

                foreach (var panelData in projectData.BlackBoxControlPanels)
                {
                    var panel = ConvertDataToPanel(panelData);
                    var panelViewModel = new BlackBoxControlPanelViewModel(panel);
                    _mainViewModel.BlackBoxControlPanels.Add(panelViewModel);
                }

                MessageBox.Show(
                    $"Configuration loaded successfully!\n\nPanels: {projectData.BlackBoxControlPanels.Count}",
                    "Success",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            };

            var dialog = new DownloadConfigurationDialog(viewModel);
            dialog.Owner = Application.Current.MainWindow;
            dialog.ShowDialog();
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
                FirmwareVersion = data.FirmwareVersion,
                Loops = new ObservableCollection<Loop>()
            };

            foreach (var loopData in data.Loops)
            {
                var loop = new Loop
                {
                    LoopNumber = loopData.LoopNumber,
                    LoopName = loopData.LoopName,
                    Devices = new ObservableCollection<LoopDevice>()
                };

                foreach (var deviceData in loopData.Devices)
                {
                    loop.Devices.Add(new LoopDevice
                    {
                        Address = (byte)deviceData.Address,
                        Type = deviceData.Type,
                        LocationText = deviceData.LocationText,
                        Zone = deviceData.Zone,
                        ImagePath = deviceData.ImagePath
                    });
                }

                panel.Loops.Add(loop);
            }

            return panel;
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
}

using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
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
    public class MenuViewModel : ViewModelBase
    {
        private readonly MainViewModel _mainViewModel;
        private readonly IProjectService _projectService;
        private ObservableCollection<RecentProjectViewModel> _recentProjects = new ObservableCollection<RecentProjectViewModel>();

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

        public MenuViewModel(MainViewModel mainViewModel, IProjectService projectService)
        {
            _mainViewModel = mainViewModel;
            _projectService = projectService;
            RecentProjects = new ObservableCollection<RecentProjectViewModel>();
            LoadRecentProjects();

            NewProjectCommand = new RelayCommand(NewProject);
            OpenProjectCommand = new RelayCommand(async () => await OpenProjectAsync());
            OpenRecentProjectCommand = new RelayCommand<string>(async path => await OpenRecentProjectAsync(path));
            ClearRecentProjectsCommand = new RelayCommand(ClearRecentProjects);
            CloseProjectCommand = new RelayCommand(CloseProject);
            SaveProjectCommand = new RelayCommand(async () => await SaveProjectAsync(), CanSaveProject);
            SaveProjectAsCommand = new RelayCommand(async () => await SaveProjectAsAsync());
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

        private async Task OpenProjectAsync()
        {
            var openFileDialog = new OpenFileDialog
            {
                Title = "Open Fire Panel Project",
                Filter = "BAAMIIS Project Files (*.kbb)|*.kbb|All Files (*.*)|*.*",
                DefaultExt = ".kbb"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                await OpenProjectFileAsync(openFileDialog.FileName);
            }
        }

        private async Task OpenRecentProjectAsync(string projectPath)
        {
            if (string.IsNullOrEmpty(projectPath))
                return;

            if (!File.Exists(projectPath))
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

            await OpenProjectFileAsync(projectPath);
        }

        private async Task OpenProjectFileAsync(string filePath)
        {
            try
            {
                var projectData = await _projectService.LoadAsync(filePath)
                    ?? throw new InvalidOperationException("Failed to deserialize project file.");
                var panelVMs = ProjectMapper.ToPanelViewModels(projectData);

                _mainViewModel.BlackBoxControlPanels.Clear();
                foreach (var panel in panelVMs)
                    _mainViewModel.BlackBoxControlPanels.Add(panel);

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
                    FileName = Path.GetFileName(path),
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

        private async Task SaveProjectAsync()
        {
            if (string.IsNullOrEmpty(_mainViewModel.CurrentProjectPath))
            {
                await SaveProjectAsAsync();
            }
            else
            {
                try
                {
                    var projectData = ProjectMapper.ToProjectData(
                        Path.GetFileNameWithoutExtension(_mainViewModel.CurrentProjectPath),
                        _mainViewModel.BlackBoxControlPanels);

                    await _projectService.SaveAsync(_mainViewModel.CurrentProjectPath, projectData);
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

        private async Task SaveProjectAsAsync()
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
                    var projectData = ProjectMapper.ToProjectData(
                        Path.GetFileNameWithoutExtension(saveFileDialog.FileName),
                        _mainViewModel.BlackBoxControlPanels);

                    await _projectService.SaveAsync(saveFileDialog.FileName, projectData);
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

            var projectData = ProjectMapper.ToProjectData("Current Project", _mainViewModel.BlackBoxControlPanels);
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
                Application.Current.Dispatcher.Invoke(() =>
                {
                    var panelVMs = ProjectMapper.ToPanelViewModels(projectData);

                    _mainViewModel.BlackBoxControlPanels.Clear();
                    foreach (var panelVM in panelVMs)
                        _mainViewModel.BlackBoxControlPanels.Add(panelVM);

                    MessageBox.Show(
                        $"Configuration downloaded successfully!\n\n" +
                        $"Panels: {projectData.BlackBoxControlPanels.Count}\n" +
                        $"Loops: {projectData.BlackBoxControlPanels.Sum(p => p.Loops.Count)}\n" +
                        $"Devices: {projectData.BlackBoxControlPanels.Sum(p => p.Loops.Sum(l => l.Devices.Count))}\n" +
                        $"Buses: {projectData.BlackBoxControlPanels.Sum(p => p.Busses.Count)}\n" +
                        $"Bus Nodes: {projectData.BlackBoxControlPanels.Sum(p => p.Busses.Sum(b => b.Nodes.Count))}",
                        "Download Complete",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                });
            };

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
    }

    public class RecentProjectViewModel
    {
        public string? FilePath { get; set; }
        public string? FileName { get; set; }
        public ICommand? OpenCommand { get; set; }
    }
}

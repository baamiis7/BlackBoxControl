using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using BlackBoxControl.Models;
using BlackBoxControl.Services;

namespace BlackBoxControl.ViewModels
{
    public class DownloadConfigurationViewModel : INotifyPropertyChanged
    {
        private readonly SerialCommunicationService _serialService;
        private readonly ConfigurationDownloadService _downloadService;
        private CancellationTokenSource _cancellationTokenSource;

        private ObservableCollection<SerialPortInfo> _availablePorts;
        private SerialPortInfo _selectedPort;
        private bool _isConnected;
        private bool _isDownloading;
        private int _downloadProgress;
        private string _statusMessage;
        private string _logMessages;

        public ObservableCollection<SerialPortInfo> AvailablePorts
        {
            get => _availablePorts;
            set { _availablePorts = value; OnPropertyChanged(); }
        }

        public SerialPortInfo SelectedPort
        {
            get => _selectedPort;
            set { _selectedPort = value; OnPropertyChanged(); }
        }

        public bool IsConnected
        {
            get => _isConnected;
            set { _isConnected = value; OnPropertyChanged(); OnPropertyChanged(nameof(CanConnect)); OnPropertyChanged(nameof(CanDownload)); }
        }

        public bool IsDownloading
        {
            get => _isDownloading;
            set { _isDownloading = value; OnPropertyChanged(); OnPropertyChanged(nameof(CanConnect)); OnPropertyChanged(nameof(CanDownload)); OnPropertyChanged(nameof(CanCancel)); }
        }

        public int DownloadProgress
        {
            get => _downloadProgress;
            set { _downloadProgress = value; OnPropertyChanged(); }
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set { _statusMessage = value; OnPropertyChanged(); }
        }

        public string LogMessages
        {
            get => _logMessages;
            set { _logMessages = value; OnPropertyChanged(); }
        }

        public bool CanConnect => !IsConnected && !IsDownloading && SelectedPort != null;
        public bool CanDownload => IsConnected && !IsDownloading;
        public bool CanCancel => IsDownloading;

        public ICommand RefreshPortsCommand { get; }
        public ICommand ConnectCommand { get; }
        public ICommand DisconnectCommand { get; }
        public ICommand DownloadCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand CloseCommand { get; }

        public event Action<ProjectData> DownloadCompleted;
        public event Action RequestClose;

        public DownloadConfigurationViewModel()
        {
            _serialService = new SerialCommunicationService();
            _downloadService = new ConfigurationDownloadService(_serialService);

            // Subscribe to events
            _serialService.MessageReceived += OnSerialMessage;
            _serialService.ErrorOccurred += OnSerialError;
            _serialService.UploadProgressChanged += OnDownloadProgress;

            // Commands
            RefreshPortsCommand = new RelayCommand(RefreshPorts);
            ConnectCommand = new RelayCommand(async () => await ConnectAsync(), () => CanConnect);
            DisconnectCommand = new RelayCommand(Disconnect);
            DownloadCommand = new RelayCommand(async () => await DownloadAsync(), () => CanDownload);
            CancelCommand = new RelayCommand(CancelDownload, () => CanCancel);
            CloseCommand = new RelayCommand(Close);

            // Initialize
            StatusMessage = "Ready to download configuration";
            LogMessages = string.Empty;
            AvailablePorts = new ObservableCollection<SerialPortInfo>();

            RefreshPorts();
        }

        private void RefreshPorts()
        {
            try
            {
                var ports = SerialCommunicationService.GetAvailablePorts();
                AvailablePorts.Clear();

                foreach (var port in ports)
                {
                    AvailablePorts.Add(port);
                }

                if (AvailablePorts.Count > 0)
                {
                    SelectedPort = AvailablePorts[0];
                    StatusMessage = $"Found {AvailablePorts.Count} port(s)";
                }
                else
                {
                    StatusMessage = "No COM ports found. Is ESP32 connected?";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error refreshing ports: {ex.Message}";
            }
        }

        private async Task ConnectAsync()
        {
            if (SelectedPort == null)
            {
                StatusMessage = "Please select a COM port";
                return;
            }

            try
            {
                StatusMessage = $"Connecting to {SelectedPort.PortName}...";

                _cancellationTokenSource = new CancellationTokenSource();
                bool connected = await _serialService.ConnectAsync(SelectedPort.PortName, _cancellationTokenSource.Token);

                if (connected)
                {
                    IsConnected = true;
                    StatusMessage = $"Connected to {SelectedPort.PortName}";
                    AddLog($"Successfully connected to {SelectedPort.PortName}");
                }
                else
                {
                    StatusMessage = "Connection failed";
                    AddLog("Connection failed - check if ESP32 is running");
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Connection error: {ex.Message}";
                AddLog($"ERROR: {ex.Message}");
            }
        }

        private void Disconnect()
        {
            try
            {
                _serialService.Disconnect();
                IsConnected = false;
                StatusMessage = "Disconnected";
                AddLog("Disconnected from ESP32");
            }
            catch (Exception ex)
            {
                StatusMessage = $"Disconnect error: {ex.Message}";
            }
        }

        private async Task DownloadAsync()
        {
            if (!IsConnected)
            {
                StatusMessage = "Not connected to ESP32";
                return;
            }

            try
            {
                IsDownloading = true;
                DownloadProgress = 0;
                StatusMessage = "Starting download...";
                AddLog("=== Starting Configuration Download ===");

                _cancellationTokenSource = new CancellationTokenSource();

                var projectData = await _downloadService.DownloadConfigurationAsync(_cancellationTokenSource.Token);

                if (projectData != null)
                {
                    StatusMessage = "Download complete!";
                    AddLog("=== Download Successful ===");
                    AddLog($"Received {projectData.BlackBoxControlPanels.Count} panel(s)");

                    MessageBox.Show(
                        $"Configuration downloaded successfully!\n\n" +
                        $"Panels: {projectData.BlackBoxControlPanels.Count}",
                        "Download Complete",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);

                    // Notify that download is complete
                    DownloadCompleted?.Invoke(projectData);

                    // Close dialog
                    RequestClose?.Invoke();
                }
                else
                {
                    StatusMessage = "Download failed - no data received";
                    AddLog("=== Download Failed ===");
                }
            }
            catch (OperationCanceledException)
            {
                StatusMessage = "Download cancelled";
                AddLog("Download cancelled by user");
            }
            catch (Exception ex)
            {
                StatusMessage = $"Download error: {ex.Message}";
                AddLog($"ERROR: {ex.Message}");
                MessageBox.Show(
                    $"Download error: {ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            finally
            {
                IsDownloading = false;
                DownloadProgress = 0;
            }
        }

        private void CancelDownload()
        {
            _cancellationTokenSource?.Cancel();
            StatusMessage = "Cancelling download...";
        }

        private void Close()
        {
            if (IsDownloading)
            {
                var result = MessageBox.Show(
                    "Download is in progress. Are you sure you want to close?",
                    "Download in Progress",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result != MessageBoxResult.Yes)
                    return;

                _cancellationTokenSource?.Cancel();
            }

            Disconnect();
            RequestClose?.Invoke();
        }

        private void OnSerialMessage(object sender, string message)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                AddLog(message);
            });
        }

        private void OnSerialError(object sender, Exception ex)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                AddLog($"ERROR: {ex.Message}");
            });
        }

        private void OnDownloadProgress(object sender, UploadProgress progress)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                DownloadProgress = progress.PercentComplete;
                StatusMessage = progress.CurrentOperation;
            });
        }

        private void AddLog(string message)
        {
            string timestamp = DateTime.Now.ToString("HH:mm:ss");
            LogMessages += $"[{timestamp}] {message}\n";
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}

using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using BlackBoxControl.Helpers;
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
        private bool _useSimulator;

        public ObservableCollection<SerialPortInfo> AvailablePorts
        {
            get => _availablePorts;
            set { _availablePorts = value; OnPropertyChanged(); }
        }

        public SerialPortInfo SelectedPort
        {
            get => _selectedPort;
            set
            {
                _selectedPort = value;
                OnPropertyChanged();
                (ConnectCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }

        public bool IsConnected
        {
            get => _isConnected;
            set
            {
                _isConnected = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CanConnect));
                OnPropertyChanged(nameof(CanDownload));
                (ConnectCommand as RelayCommand)?.RaiseCanExecuteChanged();
                (DisconnectCommand as RelayCommand)?.RaiseCanExecuteChanged();
                (DownloadCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }

        public bool IsDownloading
        {
            get => _isDownloading;
            set
            {
                _isDownloading = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CanConnect));
                OnPropertyChanged(nameof(CanDownload));
                OnPropertyChanged(nameof(CanCancel));
                (ConnectCommand as RelayCommand)?.RaiseCanExecuteChanged();
                (DownloadCommand as RelayCommand)?.RaiseCanExecuteChanged();
                (CancelCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
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

        public bool UseSimulator
        {
            get => _useSimulator;
            set
            {
                _useSimulator = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsSimulatorMode));
                OnPropertyChanged(nameof(CanSelectPort));

                // Enable/disable simulator in service
                if (_serialService is MockSerialCommunicationService mockService)
                {
                    mockService.EnableSimulator(value);
                    if (value)
                    {
                        StatusMessage = "Simulator mode enabled - no hardware required";
                        AddLog("Virtual ESP32 Simulator activated");
                    }
                    else
                    {
                        StatusMessage = "Simulator mode disabled - using real hardware";
                        AddLog("Switched to real hardware mode");
                    }
                }

                // Refresh command states
                (ConnectCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }

        public bool IsSimulatorMode => UseSimulator;
        public bool CanSelectPort => !IsConnected && !IsDownloading && !UseSimulator;

        // Fixed: Allow connection when simulator is enabled OR when a port is selected
        public bool CanConnect => !IsConnected && !IsDownloading && (UseSimulator || SelectedPort != null);
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
            // Use MockSerialCommunicationService
            _serialService = new MockSerialCommunicationService();
            _downloadService = new ConfigurationDownloadService(_serialService);

            // Subscribe to events
            _serialService.MessageReceived += OnSerialMessage;
            _serialService.ErrorOccurred += OnSerialError;
            _serialService.DownloadProgressChanged += OnDownloadProgress;

            // Commands
            RefreshPortsCommand = new RelayCommand(RefreshPorts);
            ConnectCommand = new RelayCommand(async () => await ConnectAsync(), () => CanConnect);
            DisconnectCommand = new RelayCommand(Disconnect, () => IsConnected);
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
                    StatusMessage = "No COM ports found. Enable simulator or connect ESP32.";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error refreshing ports: {ex.Message}";
            }
        }

        private async Task ConnectAsync()
        {
            try
            {
                string portName = UseSimulator ? "COM_SIMULATOR" : SelectedPort?.PortName;

                if (!UseSimulator && SelectedPort == null)
                {
                    StatusMessage = "Please select a COM port";
                    return;
                }

                StatusMessage = UseSimulator ? "Connecting to simulator..." : $"Connecting to {portName}...";
                AddLog($"Connecting to {portName}...");

                _cancellationTokenSource = new CancellationTokenSource();
                bool connected = await _serialService.ConnectAsync(portName, _cancellationTokenSource.Token);

                if (connected)
                {
                    IsConnected = true;
                    StatusMessage = UseSimulator
                        ? "Connected to Virtual ESP32 Simulator"
                        : $"Connected to {portName}";
                    AddLog("Connection successful!");
                }
                else
                {
                    StatusMessage = "Connection failed";
                    AddLog("Connection failed - check if ESP32 is available");
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

                    // Raise event to notify MainViewModel
                    DownloadCompleted?.Invoke(projectData);

                    // Close the dialog
                    RequestClose?.Invoke();
                }
                else
                {
                    StatusMessage = "Download failed";
                    AddLog("=== Download Failed ===");
                    MessageBox.Show(
                        "Configuration download failed. Check the log for details.",
                        "Download Failed",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
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

        private void OnDownloadProgress(object sender, DownloadProgress progress)
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

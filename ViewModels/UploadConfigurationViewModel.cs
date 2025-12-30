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
    public class UploadConfigurationViewModel : INotifyPropertyChanged
    {
        private readonly SerialCommunicationService _serialService;
        private readonly ConfigurationUploadService _uploadService;
        private readonly ProjectData _projectData;
        private CancellationTokenSource _cancellationTokenSource;

        private ObservableCollection<SerialPortInfo> _availablePorts;
        private SerialPortInfo _selectedPort;
        private bool _isConnected;
        private bool _isUploading;
        private int _uploadProgress;
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
                OnPropertyChanged(nameof(CanUpload));
                (ConnectCommand as RelayCommand)?.RaiseCanExecuteChanged();
                (DisconnectCommand as RelayCommand)?.RaiseCanExecuteChanged();
                (UploadCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }

        public bool IsUploading
        {
            get => _isUploading;
            set
            {
                _isUploading = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CanConnect));
                OnPropertyChanged(nameof(CanUpload));
                OnPropertyChanged(nameof(CanCancel));
                (ConnectCommand as RelayCommand)?.RaiseCanExecuteChanged();
                (UploadCommand as RelayCommand)?.RaiseCanExecuteChanged();
                (CancelCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }

        public int UploadProgress
        {
            get => _uploadProgress;
            set { _uploadProgress = value; OnPropertyChanged(); }
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
        public bool CanSelectPort => !IsConnected && !IsUploading && !UseSimulator;

        // Fixed: Allow connection when simulator is enabled OR when a port is selected
        public bool CanConnect => !IsConnected && !IsUploading && (UseSimulator || SelectedPort != null);
        public bool CanUpload => IsConnected && !IsUploading;
        public bool CanCancel => IsUploading;

        public ICommand RefreshPortsCommand { get; }
        public ICommand ConnectCommand { get; }
        public ICommand DisconnectCommand { get; }
        public ICommand UploadCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand CloseCommand { get; }

        public event Action RequestClose;

        public UploadConfigurationViewModel(ProjectData projectData)
        {
            _projectData = projectData ?? throw new ArgumentNullException(nameof(projectData));

            // Use MockSerialCommunicationService
            _serialService = new MockSerialCommunicationService();
            _uploadService = new ConfigurationUploadService(_serialService);

            // Subscribe to events
            _serialService.MessageReceived += OnSerialMessage;
            _serialService.ErrorOccurred += OnSerialError;
            _serialService.UploadProgressChanged += OnUploadProgress;

            // Commands
            RefreshPortsCommand = new RelayCommand(RefreshPorts);
            ConnectCommand = new RelayCommand(async () => await ConnectAsync(), () => CanConnect);
            DisconnectCommand = new RelayCommand(Disconnect, () => IsConnected);
            UploadCommand = new RelayCommand(async () => await UploadAsync(), () => CanUpload);
            CancelCommand = new RelayCommand(CancelUpload, () => CanCancel);
            CloseCommand = new RelayCommand(Close);

            // Initialize
            StatusMessage = "Ready to upload configuration";
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
                    AddLog("Connection failed - check if ESP32 is running the receiver code");
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

        private async Task UploadAsync()
        {
            if (!IsConnected)
            {
                StatusMessage = "Not connected to ESP32";
                return;
            }

            try
            {
                IsUploading = true;
                UploadProgress = 0;
                StatusMessage = "Starting upload...";
                AddLog("=== Starting Configuration Upload ===");

                _cancellationTokenSource = new CancellationTokenSource();

                bool success = await _uploadService.UploadConfigurationAsync(
                    _projectData,
                    _cancellationTokenSource.Token);

                if (success)
                {
                    StatusMessage = "Upload complete!";
                    AddLog("=== Upload Successful ===");
                    MessageBox.Show(
                        "Configuration uploaded successfully to ESP32!",
                        "Upload Complete",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
                else
                {
                    StatusMessage = "Upload failed";
                    AddLog("=== Upload Failed ===");
                    MessageBox.Show(
                        "Configuration upload failed. Check the log for details.",
                        "Upload Failed",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            }
            catch (OperationCanceledException)
            {
                StatusMessage = "Upload cancelled";
                AddLog("Upload cancelled by user");
            }
            catch (Exception ex)
            {
                StatusMessage = $"Upload error: {ex.Message}";
                AddLog($"ERROR: {ex.Message}");
                MessageBox.Show(
                    $"Upload error: {ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            finally
            {
                IsUploading = false;
                UploadProgress = 0;
            }
        }

        private void CancelUpload()
        {
            _cancellationTokenSource?.Cancel();
            StatusMessage = "Cancelling upload...";
        }

        private void Close()
        {
            if (IsUploading)
            {
                var result = MessageBox.Show(
                    "Upload is in progress. Are you sure you want to close?",
                    "Upload in Progress",
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

        private void OnUploadProgress(object sender, UploadProgress progress)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                UploadProgress = progress.PercentComplete;
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

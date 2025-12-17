using System.Windows.Input;
using System.ComponentModel;
using BlackBoxControl.Models;

namespace BlackBoxControl.ViewModels
{


    public class SerialCommViewModel : INotifyPropertyChanged
    {
        public SerialCommModel _serialModel;

        public event PropertyChangedEventHandler PropertyChanged;

        public SerialCommViewModel()
        {
            _serialModel = new SerialCommModel("COM3", 9600);
            _serialModel.DataReceived += OnDataReceived;
            OpenCommand = new RelayCommand(OpenSerialPort);
            CloseCommand = new RelayCommand(CloseSerialPort);
        }

        private string _receivedData;
        public string ReceivedData
        {
            get => _receivedData;
            set
            {
                _receivedData = value;
                OnPropertyChanged(nameof(ReceivedData));
            }
        }

        public ICommand OpenCommand { get; }
        public ICommand CloseCommand { get; }
        public ICommand SendCommand { get; }

        private void OpenSerialPort()
        {
            _serialModel.Open();
        }

        private void CloseSerialPort()
        {
            _serialModel.Close();
        }

        private void OnDataReceived(byte[] data)
        {
            // Assuming you want to display the received data as a string
            ReceivedData = System.Text.Encoding.ASCII.GetString(data);
        }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

}

using System;
using System.IO.Ports;

namespace BlackBoxControl.Models
{
    public class SerialCommModel
    {
        private SerialPort _serialPort;

        public SerialCommModel(string portName, int baudRate)
        {
            _serialPort = new SerialPort(portName, baudRate);
            _serialPort.DataReceived += SerialDataReceived;
        }

        public void Open()
        {
            if (!_serialPort.IsOpen)
            {
                _serialPort.Open();
            }
        }

        public void Close()
        {
            if (_serialPort.IsOpen)
            {
                _serialPort.Close();
            }
        }

        public void SendData(byte[] data)
        {
            if (_serialPort.IsOpen)
            {
                _serialPort.Write(data, 0, data.Length);
            }
        }

        private void SerialDataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            SerialPort sp = (SerialPort)sender;
            byte[] buffer = new byte[sp.BytesToRead];
            sp.Read(buffer, 0, buffer.Length);
            OnDataReceived(buffer);
        }

        public event Action<byte[]> DataReceived;

        protected virtual void OnDataReceived(byte[] data)
        {
            DataReceived?.Invoke(data);
        }
    }

}

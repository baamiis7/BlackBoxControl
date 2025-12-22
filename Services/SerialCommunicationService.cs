using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BlackBoxControl.Protocol;

namespace BlackBoxControl.Services
{
    /// <summary>
    /// Serial port information
    /// </summary>
    public class SerialPortInfo
    {
        public string PortName { get; set; }
        public string Description { get; set; }

        public override string ToString() => $"{PortName} - {Description}";
    }

    /// <summary>
    /// Upload progress information
    /// </summary>
    public class UploadProgress
    {
        public int TotalPackets { get; set; }
        public int SentPackets { get; set; }
        public int PercentComplete => TotalPackets > 0 ? (SentPackets * 100) / TotalPackets : 0;
        public string CurrentOperation { get; set; }
        public bool IsComplete => SentPackets >= TotalPackets;
    }

    /// <summary>
    /// Service for serial communication with ESP32
    /// </summary>
    public class SerialCommunicationService : IDisposable
    {
        private SerialPort _serialPort;
        private readonly object _lock = new object();
        private bool _isConnected;

        public event EventHandler<string> MessageReceived;
        public event EventHandler<Exception> ErrorOccurred;
        public event EventHandler<UploadProgress> UploadProgressChanged;

        public bool IsConnected => _isConnected && _serialPort?.IsOpen == true;

        /// <summary>
        /// Get list of available COM ports
        /// </summary>
        public static List<SerialPortInfo> GetAvailablePorts()
        {
            var ports = new List<SerialPortInfo>();

            foreach (string portName in SerialPort.GetPortNames())
            {
                ports.Add(new SerialPortInfo
                {
                    PortName = portName,
                    Description = $"Serial Port {portName}"
                });
            }

            return ports.OrderBy(p => p.PortName).ToList();
        }

        /// <summary>
        /// Connect to ESP32 on specified port
        /// </summary>
        public async Task<bool> ConnectAsync(string portName, CancellationToken cancellationToken = default)
        {
            try
            {
                Disconnect();

                _serialPort = new SerialPort(portName)
                {
                    BaudRate = ProtocolConstants.BAUD_RATE,
                    DataBits = 8,
                    Parity = Parity.None,
                    StopBits = StopBits.One,
                    Handshake = Handshake.None,
                    ReadTimeout = 1000,
                    WriteTimeout = 1000
                };

                _serialPort.DataReceived += SerialPort_DataReceived;
                _serialPort.Open();

                // Small delay for ESP32 to stabilize
                await Task.Delay(100, cancellationToken);

                // Perform handshake
                bool handshakeSuccess = await PerformHandshakeAsync(cancellationToken);

                if (handshakeSuccess)
                {
                    _isConnected = true;
                    OnMessage($"Connected to {portName}");
                    return true;
                }
                else
                {
                    Disconnect();
                    OnError(new Exception("Handshake failed - ESP32 did not respond"));
                    return false;
                }
            }
            catch (Exception ex)
            {
                OnError(ex);
                Disconnect();
                return false;
            }
        }

        /// <summary>
        /// Disconnect from ESP32
        /// </summary>
        public void Disconnect()
        {
            lock (_lock)
            {
                _isConnected = false;

                if (_serialPort != null)
                {
                    try
                    {
                        if (_serialPort.IsOpen)
                        {
                            _serialPort.DataReceived -= SerialPort_DataReceived;
                            _serialPort.Close();
                        }
                        _serialPort.Dispose();
                    }
                    catch (Exception ex)
                    {
                        OnError(ex);
                    }
                    finally
                    {
                        _serialPort = null;
                    }
                }
            }
        }

        /// <summary>
        /// Perform handshake with ESP32
        /// </summary>
        private async Task<bool> PerformHandshakeAsync(CancellationToken cancellationToken)
        {
            try
            {
                OnMessage("Performing handshake...");

                // Send handshake request
                var handshakePacket = new BinaryPacket(ProtocolConstants.PACKET_HANDSHAKE, new byte[0]);
                await SendPacketAsync(handshakePacket, cancellationToken);

                // Wait for ACK
                var response = await WaitForAckAsync(ProtocolConstants.HANDSHAKE_TIMEOUT_MS, cancellationToken);

                if (response)
                {
                    OnMessage("Handshake successful");
                    return true;
                }
                else
                {
                    OnMessage("Handshake failed - no response");
                    return false;
                }
            }
            catch (Exception ex)
            {
                OnError(ex);
                return false;
            }
        }

        /// <summary>
        /// Send binary packet to ESP32
        /// </summary>
        public async Task SendPacketAsync(BinaryPacket packet, CancellationToken cancellationToken)
        {
            if (!IsConnected)
                throw new InvalidOperationException("Not connected to ESP32");

            try
            {
                byte[] packetBytes = packet.ToBytes();

                await Task.Run(() =>
                {
                    lock (_lock)
                    {
                        _serialPort.Write(packetBytes, 0, packetBytes.Length);
                    }
                }, cancellationToken);

                // Small delay between packets
                await Task.Delay(10, cancellationToken);
            }
            catch (Exception ex)
            {
                OnError(ex);
                throw;
            }
        }

        /// <summary>
        /// Wait for ACK from ESP32
        /// </summary>
        private async Task<bool> WaitForAckAsync(int timeoutMs, CancellationToken cancellationToken)
        {
            var startTime = DateTime.Now;
            var buffer = new List<byte>();

            while ((DateTime.Now - startTime).TotalMilliseconds < timeoutMs)
            {
                if (cancellationToken.IsCancellationRequested)
                    return false;

                try
                {
                    if (_serialPort.BytesToRead > 0)
                    {
                        int bytesRead = _serialPort.BytesToRead;
                        byte[] readBuffer = new byte[bytesRead];
                        _serialPort.Read(readBuffer, 0, bytesRead);
                        buffer.AddRange(readBuffer);

                        // Try to parse packet
                        if (TryParseAckPacket(buffer.ToArray(), out bool isAck))
                        {
                            return isAck;
                        }
                    }
                }
                catch (TimeoutException)
                {
                    // Continue waiting
                }

                await Task.Delay(10, cancellationToken);
            }

            return false; // Timeout
        }

        /// <summary>
        /// Try to parse ACK/NACK packet from buffer
        /// </summary>
        private bool TryParseAckPacket(byte[] buffer, out bool isAck)
        {
            isAck = false;

            try
            {
                if (buffer.Length < 6) // Minimum packet size
                    return false;

                var packet = BinaryPacket.FromBytes(buffer);

                if (packet.PacketType == ProtocolConstants.PACKET_ACK)
                {
                    isAck = true;
                    return true;
                }
                else if (packet.PacketType == ProtocolConstants.PACKET_NACK)
                {
                    isAck = false;
                    return true;
                }
            }
            catch
            {
                // Invalid packet, continue
            }

            return false;
        }

        /// <summary>
        /// Send packet and wait for acknowledgment
        /// </summary>
        public async Task<bool> SendPacketWithAckAsync(BinaryPacket packet, CancellationToken cancellationToken)
        {
            await SendPacketAsync(packet, cancellationToken);
            return await WaitForAckAsync(ProtocolConstants.ACK_TIMEOUT_MS, cancellationToken);
        }

        /// <summary>
        /// Handle received data
        /// </summary>
        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                if (_serialPort == null || !_serialPort.IsOpen)
                    return;

                int bytesToRead = _serialPort.BytesToRead;
                if (bytesToRead > 0)
                {
                    byte[] buffer = new byte[bytesToRead];
                    _serialPort.Read(buffer, 0, bytesToRead);

                    // Log received data for debugging
                    string hex = BitConverter.ToString(buffer).Replace("-", " ");
                    OnMessage($"Received: {hex}");
                }
            }
            catch (Exception ex)
            {
                OnError(ex);
            }
        }

        /// <summary>
        /// Raise message event
        /// </summary>
        private void OnMessage(string message)
        {
            MessageReceived?.Invoke(this, message);
        }

        /// <summary>
        /// Raise error event
        /// </summary>
        private void OnError(Exception ex)
        {
            ErrorOccurred?.Invoke(this, ex);
        }

        /// <summary>
        /// Raise upload progress event
        /// </summary>
        public void OnUploadProgress(UploadProgress progress)
        {
            UploadProgressChanged?.Invoke(this, progress);
        }

        /// <summary>
        /// Dispose resources
        /// </summary>
        public void Dispose()
        {
            Disconnect();
        }
    }
}

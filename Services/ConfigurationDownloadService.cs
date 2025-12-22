using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BlackBoxControl.Models;
using BlackBoxControl.Protocol;

namespace BlackBoxControl.Services
{
    /// <summary>
    /// Service to download and deserialize configuration from ESP32
    /// </summary>
    public class ConfigurationDownloadService
    {
        private readonly SerialCommunicationService _serialService;
        private ProjectData _receivedProject;
        private BlackBoxControlPanelData _currentPanel;
        private LoopData _currentLoop;
        private BusData _currentBus;

        public ConfigurationDownloadService(SerialCommunicationService serialService)
        {
            _serialService = serialService ?? throw new ArgumentNullException(nameof(serialService));
        }

        /// <summary>
        /// Download complete configuration from ESP32
        /// </summary>
        public async Task<ProjectData> DownloadConfigurationAsync(CancellationToken cancellationToken = default)
        {
            if (!_serialService.IsConnected)
                throw new InvalidOperationException("Not connected to ESP32");

            try
            {
                // Initialize new project
                _receivedProject = new ProjectData
                {
                    ProjectName = "Downloaded from ESP32",
                    BlackBoxControlPanels = new List<BlackBoxControlPanelData>()
                };

                UpdateProgress(0, 100, "Requesting configuration from ESP32...");

                // Send download request to ESP32
                var requestPacket = new BinaryPacket(ProtocolConstants.PACKET_DOWNLOAD_REQUEST, new byte[0]);
                await _serialService.SendPacketAsync(requestPacket, cancellationToken);

                // Wait for ESP32 to send configuration packets
                bool downloadComplete = false;
                int packetsReceived = 0;

                while (!downloadComplete && !cancellationToken.IsCancellationRequested)
                {
                    // Read packets from ESP32
                    var packet = await ReceivePacketAsync(cancellationToken);

                    if (packet != null)
                    {
                        packetsReceived++;
                        UpdateProgress(packetsReceived, 100, $"Receiving packet {packetsReceived}...");

                        // Process packet based on type
                        switch (packet.PacketType)
                        {
                            case ProtocolConstants.PACKET_PANEL_CONFIG:
                                HandlePanelConfig(packet.Data);
                                break;

                            case ProtocolConstants.PACKET_LOOP_CONFIG:
                                HandleLoopConfig(packet.Data);
                                break;

                            case ProtocolConstants.PACKET_DEVICE_CONFIG:
                                HandleDeviceConfig(packet.Data);
                                break;

                            case ProtocolConstants.PACKET_BUS_CONFIG:
                                HandleBusConfig(packet.Data);
                                break;

                            case ProtocolConstants.PACKET_END_TRANSMISSION:
                                downloadComplete = true;
                                UpdateProgress(100, 100, "Download complete!");
                                break;

                            default:
                                // Unknown packet type, ignore
                                break;
                        }

                        // Send ACK
                        await SendAckAsync(cancellationToken);
                    }

                    // Timeout if no data received for 5 seconds
                    await Task.Delay(100, cancellationToken);
                }

                if (downloadComplete)
                {
                    return _receivedProject;
                }
                else
                {
                    throw new TimeoutException("Download timed out - no data received from ESP32");
                }
            }
            catch (Exception ex)
            {
                UpdateProgress(0, 100, $"Download failed: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Receive packet from ESP32
        /// </summary>
        private async Task<BinaryPacket> ReceivePacketAsync(CancellationToken cancellationToken)
        {
            // This is a simplified version - you'll need to implement proper packet reception
            // with buffering and timeout handling

            // Wait for data to be available
            var timeout = TimeSpan.FromSeconds(10);
            var startTime = DateTime.Now;

            while ((DateTime.Now - startTime) < timeout && !cancellationToken.IsCancellationRequested)
            {
                // Check if we have data (this would need to be implemented in SerialCommunicationService)
                // For now, return null as placeholder
                await Task.Delay(100, cancellationToken);
            }

            return null; // Placeholder - implement actual packet reception
        }

        /// <summary>
        /// Send ACK to ESP32
        /// </summary>
        private async Task SendAckAsync(CancellationToken cancellationToken)
        {
            var ackPacket = new BinaryPacket(ProtocolConstants.PACKET_ACK, new byte[0]);
            await _serialService.SendPacketAsync(ackPacket, cancellationToken);
        }

        /// <summary>
        /// Handle panel configuration packet
        /// </summary>
        private void HandlePanelConfig(byte[] data)
        {
            var reader = new ProtocolReader(data);

            _currentPanel = new BlackBoxControlPanelData
            {
                PanelAddress = reader.ReadByte(),
                PanelName = reader.ReadString(),
                Location = reader.ReadString(),
                NumberOfLoops = reader.ReadByte(),
                Loops = new List<LoopData>(),
                Busses = new List<BusData>()
            };

            _receivedProject.BlackBoxControlPanels.Add(_currentPanel);
        }

        /// <summary>
        /// Handle loop configuration packet
        /// </summary>
        private void HandleLoopConfig(byte[] data)
        {
            if (_currentPanel == null)
                return;

            var reader = new ProtocolReader(data);

            byte panelId = reader.ReadByte();

            _currentLoop = new LoopData
            {
                LoopNumber = reader.ReadByte(),
                LoopName = reader.ReadString(),
                Devices = new List<LoopDeviceData>()
            };

            // Skip max devices
            reader.ReadByte();

            _currentPanel.Loops.Add(_currentLoop);
        }

        /// <summary>
        /// Handle device configuration packet
        /// </summary>
        private void HandleDeviceConfig(byte[] data)
        {
            if (_currentLoop == null)
                return;

            var reader = new ProtocolReader(data);

            var device = new LoopDeviceData
            {
                Address = reader.ReadByte(),
                Type = GetDeviceTypeName(reader.ReadByte()),
                LocationText = reader.ReadString(),
                Zone = reader.ReadByte()
            };

            // Skip enabled flag
            reader.ReadBoolean();

            _currentLoop.Devices.Add(device);
        }

        /// <summary>
        /// Handle bus configuration packet
        /// </summary>
        private void HandleBusConfig(byte[] data)
        {
            if (_currentPanel == null)
                return;

            var reader = new ProtocolReader(data);

            _currentBus = new BusData
            {
                BusName = reader.ReadString(),
                Nodes = new List<BusNodeData>()
            };

            _currentPanel.Busses.Add(_currentBus);
        }

        /// <summary>
        /// Convert device type code to name
        /// </summary>
        private string GetDeviceTypeName(byte typeCode)
        {
            return typeCode switch
            {
                0x01 => "Smoke Detector",
                0x02 => "Heat Detector",
                0x03 => "Manual Call Point",
                0x04 => "Optical Detector",
                0x10 => "Sounder",
                0x11 => "Beacon",
                0x12 => "Sounder Beacon",
                _ => "Unknown Device"
            };
        }

        /// <summary>
        /// Update download progress
        /// </summary>
        private void UpdateProgress(int received, int total, string operation)
        {
            _serialService.OnUploadProgress(new UploadProgress
            {
                SentPackets = received,
                TotalPackets = total,
                CurrentOperation = operation
            });
        }
    }
}

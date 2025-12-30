using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BlackBoxControl.Helpers;
using BlackBoxControl.Models;
using BlackBoxControl.Protocol;

namespace BlackBoxControl.Services
{
    public class ConfigurationDownloadService
    {
        private readonly SerialCommunicationService _serialService;
        private ProjectData _downloadedProject;
        private BlackBoxControlPanelData _currentPanel;
        private LoopData _currentLoop;
        private BusData _currentBus;
        private CauseAndEffectData _currentCauseEffect; // 🔥 NEW

        public ConfigurationDownloadService(SerialCommunicationService serialService)
        {
            _serialService = serialService ?? throw new ArgumentNullException(nameof(serialService));
        }

        public async Task<ProjectData> DownloadConfigurationAsync(CancellationToken cancellationToken = default)
        {
            if (!_serialService.IsConnected)
                throw new InvalidOperationException("Not connected to ESP32");

            try
            {
                ClearState();

                _downloadedProject = new ProjectData
                {
                    ProjectName = "Downloaded Configuration",
                    ProjectVersion = "1.0",
                    CreatedDate = DateTime.Now,
                    LastModifiedDate = DateTime.Now,
                    BlackBoxControlPanels = new List<BlackBoxControlPanelData>()
                };

                UpdateProgress(0, 100, "Requesting configuration from panel...");

                var requestPacket = new BinaryPacket(ProtocolConstants.PACKET_DOWNLOAD_REQUEST, new byte[0]);
                if (!await _serialService.SendPacketWithAckAsync(requestPacket, cancellationToken))
                {
                    throw new Exception("Failed to send download request");
                }

                Debug.WriteLine("[Download] Download request sent, waiting for data...");

                bool downloadComplete = false;
                int packetCount = 0;

                while (!downloadComplete && !cancellationToken.IsCancellationRequested)
                {
                    var packet = await _serialService.ReceivePacketAsync(cancellationToken);

                    if (packet == null)
                    {
                        Debug.WriteLine("[Download] Timeout waiting for packet");
                        break;
                    }

                    packetCount++;
                    UpdateProgress(packetCount, packetCount + 10, $"Receiving packet {packetCount}...");

                    switch (packet.PacketType)
                    {
                        case ProtocolConstants.PACKET_PANEL_CONFIG:
                            ProcessPanelPacket(packet);
                            break;

                        case ProtocolConstants.PACKET_LOOP_CONFIG:
                            ProcessLoopPacket(packet);
                            break;

                        case ProtocolConstants.PACKET_DEVICE_CONFIG:
                            ProcessDevicePacket(packet);
                            break;

                        case ProtocolConstants.PACKET_BUS_CONFIG:
                            ProcessBusPacket(packet);
                            break;

                        case ProtocolConstants.PACKET_BUS_NODE_CONFIG:
                            ProcessBusNodePacket(packet);
                            break;

                        // 🔥 NEW: C&E packet handlers
                        case ProtocolConstants.PACKET_CE_HEADER:
                            ProcessCEHeaderPacket(packet);
                            break;

                        case ProtocolConstants.PACKET_CE_INPUT:
                            ProcessCEInputPacket(packet);
                            break;

                        case ProtocolConstants.PACKET_CE_OUTPUT:
                            ProcessCEOutputPacket(packet);
                            break;

                        case ProtocolConstants.PACKET_END_TRANSMISSION:
                            Debug.WriteLine("[Download] End of transmission received");
                            downloadComplete = true;
                            break;

                        default:
                            Debug.WriteLine($"[Download] Unknown packet type: 0x{packet.PacketType:X2}");
                            break;
                    }
                }

                UpdateProgress(100, 100, "Download complete!");
                return _downloadedProject;
            }
            catch (Exception ex)
            {
                UpdateProgress(0, 100, $"Download failed: {ex.Message}");
                throw;
            }
        }

        private void ClearState()
        {
            _downloadedProject = null;
            _currentPanel = null;
            _currentLoop = null;
            _currentBus = null;
            _currentCauseEffect = null; // 🔥 NEW
        }

        private void ProcessPanelPacket(BinaryPacket packet)
        {
            try
            {
                var data = packet.Data;
                int offset = 0;

                byte panelAddress = data[offset++];
                string panelName = Encoding.UTF8.GetString(data, offset, 32).TrimEnd('\0');
                offset += 32;
                string location = Encoding.UTF8.GetString(data, offset, 32).TrimEnd('\0');
                offset += 32;
                byte numLoops = data[offset++];
                byte numZones = data[offset++];

                _currentPanel = new BlackBoxControlPanelData
                {
                    PanelAddress = panelAddress,
                    PanelName = panelName,
                    Location = location,
                    NumberOfLoops = numLoops,
                    NumberOfZones = numZones,
                    Loops = new List<LoopData>(),
                    Busses = new List<BusData>(),
                    CauseAndEffects = new List<CauseAndEffectData>() // 🔥 NEW
                };

                _downloadedProject.BlackBoxControlPanels.Add(_currentPanel);
                Debug.WriteLine($"[Download] Received panel: {panelName}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Download] Error processing panel packet: {ex.Message}");
            }
        }

        private void ProcessLoopPacket(BinaryPacket packet)
        {
            try
            {
                var data = packet.Data;
                int offset = 0;

                byte panelAddress = data[offset++];
                byte loopNumber = data[offset++];
                string loopName = Encoding.UTF8.GetString(data, offset, 32).TrimEnd('\0');
                offset += 32;
                byte protocol = data[offset++];
                byte numDevices = data[offset++];

                _currentLoop = new LoopData
                {
                    LoopNumber = loopNumber,
                    LoopName = loopName,
                    Devices = new List<LoopDeviceData>()
                };

                if (_currentPanel != null)
                {
                    _currentPanel.Loops.Add(_currentLoop);
                }

                Debug.WriteLine($"[Download] Received loop: {loopName}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Download] Error processing loop packet: {ex.Message}");
            }
        }

        private void ProcessDevicePacket(BinaryPacket packet)
        {
            try
            {
                var data = packet.Data;
                int offset = 0;

                byte panelAddress = data[offset++];
                byte loopNumber = data[offset++];
                byte deviceAddress = data[offset++];
                byte deviceTypeCode = data[offset++];
                string locationText = Encoding.UTF8.GetString(data, offset, 32).TrimEnd('\0');
                offset += 32;
                byte zone = data[offset++];

                string deviceType = DeviceTypeMapper.GetDeviceTypeName(deviceTypeCode);

                var device = new LoopDeviceData
                {
                    Address = deviceAddress,
                    Type = deviceType,
                    LocationText = locationText,
                    Zone = zone,
                    ImagePath = DeviceTypeMapper.GetDeviceImagePath(deviceType)
                };

                if (_currentLoop != null)
                {
                    _currentLoop.Devices.Add(device);
                }

                Debug.WriteLine($"[Download] Received device: {deviceType} at address {deviceAddress}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Download] Error processing device packet: {ex.Message}");
            }
        }

        private void ProcessBusPacket(BinaryPacket packet)
        {
            try
            {
                var data = packet.Data;
                int offset = 0;

                byte panelAddress = data[offset++];
                byte busNumber = data[offset++];
                string busName = Encoding.UTF8.GetString(data, offset, 32).TrimEnd('\0');
                offset += 32;
                byte busTypeByte = data[offset++];
                byte numNodes = data[offset++];

                _currentBus = new BusData
                {
                    BusNumber = busNumber,
                    BusName = busName,
                    BusType = busTypeByte == 1 ? "CAN" : "RS485",
                    Nodes = new List<BusNodeData>()
                };

                if (_currentPanel != null)
                {
                    _currentPanel.Busses.Add(_currentBus);
                }

                Debug.WriteLine($"[Download] Received bus: {busName}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Download] Error processing bus packet: {ex.Message}");
            }
        }

        private void ProcessBusNodePacket(BinaryPacket packet)
        {
            try
            {
                var data = packet.Data;
                int offset = 0;

                byte panelAddress = data[offset++];
                byte busNumber = data[offset++];
                byte nodeAddress = data[offset++];
                string nodeName = Encoding.UTF8.GetString(data, offset, 32).TrimEnd('\0');
                offset += 32;
                string locationText = Encoding.UTF8.GetString(data, offset, 32).TrimEnd('\0');
                offset += 32;

                var node = new BusNodeData
                {
                    Address = nodeAddress,
                    Name = nodeName,
                    LocationText = locationText,
                    ImagePath = $"/BusImages/{nodeName.Replace(" ", "_")}.png"
                };

                if (_currentBus != null)
                {
                    _currentBus.Nodes.Add(node);
                }

                Debug.WriteLine($"[Download] Received bus node: {nodeName} at address {nodeAddress}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Download] Error processing bus node packet: {ex.Message}");
            }
        }

        // 🔥 NEW: Cause & Effect Download Methods

        private void ProcessCEHeaderPacket(BinaryPacket packet)
        {
            try
            {
                var data = packet.Data;
                int offset = 0;

                byte panelAddr = data[offset++];
                string ceName = Encoding.UTF8.GetString(data, offset, 32).TrimEnd('\0');
                offset += 32;
                bool isEnabled = data[offset++] == 1;
                byte logicGate = data[offset++];
                int inputCount = data[offset++];
                int outputCount = data[offset++];

                string logicGateStr = logicGate switch
                {
                    0x01 => "AND",
                    0x02 => "XOR",
                    _ => "OR"
                };

                _currentCauseEffect = new CauseAndEffectData
                {
                    Name = ceName,
                    IsEnabled = isEnabled,
                    LogicGate = logicGateStr,
                    Inputs = new List<CauseInputData>(),
                    Outputs = new List<EffectOutputData>()
                };

                if (_currentPanel != null)
                {
                    _currentPanel.CauseAndEffects.Add(_currentCauseEffect);
                    Debug.WriteLine(
                        $"[Download] Received C&E: {ceName}, Logic={logicGateStr}, Inputs={inputCount}, Outputs={outputCount}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Download] Error processing C&E header: {ex.Message}");
            }
        }

        private void ProcessCEInputPacket(BinaryPacket packet)
        {
            try
            {
                var data = packet.Data;
                int offset = 0;

                byte panelAddr = data[offset++];
                byte addressType = data[offset++];
                byte data1 = data[offset++];
                byte data2 = data[offset++];
                byte data3 = data[offset++];
                byte data4 = data[offset++];
                string extendedData = Encoding.UTF8.GetString(data, offset, 64).TrimEnd('\0');

                var inputData = new CauseInputData();

                switch (addressType)
                {
                    case CEDataHelper.DEVICE_INPUT:
                        inputData.InputType = "Device";
                        if (!string.IsNullOrEmpty(extendedData))
                        {
                            var parts = extendedData.Split('|');
                            inputData.Type = parts.Length > 0 ? parts[0] : "";
                            inputData.LocationText = parts.Length > 1 ? parts[1] : "";
                        }
                        inputData.DeviceId = $"Device_{data3}";
                        break;

                    case CEDataHelper.TIME_OF_DAY_INPUT:
                        inputData.InputType = "TimeOfDay";
                        inputData.StartTime = $"{data1:D2}:{data2:D2}";
                        inputData.EndTime = $"{data3:D2}:{data4:D2}";
                        break;

                    case CEDataHelper.DATETIME_INPUT:
                        inputData.InputType = "DateTime";
                        int year = string.IsNullOrEmpty(extendedData) ? DateTime.Now.Year : int.Parse(extendedData);
                        inputData.TriggerDateTime = new DateTime(year, data1, data2, data3, data4, 0);
                        break;

                    case CEDataHelper.API_WEBHOOK_INPUT:
                        inputData.InputType = "ReceiveApi";
                        inputData.HttpMethod = CEDataHelper.DecodeHttpMethod(data1);
                        if (!string.IsNullOrEmpty(extendedData))
                        {
                            var parts = extendedData.Split('|');
                            inputData.ListenUrl = parts.Length > 0 ? parts[0] : "";
                            inputData.ExpectedPath = parts.Length > 1 ? parts[1] : "";
                            inputData.AuthToken = parts.Length > 2 ? parts[2] : "";
                        }
                        break;
                }

                if (_currentCauseEffect != null)
                {
                    _currentCauseEffect.Inputs.Add(inputData);
                    Debug.WriteLine($"[Download] Received C&E Input: Type={inputData.InputType}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Download] Error processing C&E input: {ex.Message}");
            }
        }

        private void ProcessCEOutputPacket(BinaryPacket packet)
        {
            try
            {
                var data = packet.Data;
                int offset = 0;

                byte panelAddr = data[offset++];
                byte addressType = data[offset++];
                byte data1 = data[offset++];
                byte data2 = data[offset++];
                byte data3 = data[offset++];
                byte data4 = data[offset++];
                string extendedData = Encoding.UTF8.GetString(data, offset, 64).TrimEnd('\0');

                var outputData = new EffectOutputData();

                switch (addressType)
                {
                    case CEDataHelper.DEVICE_OUTPUT:
                        outputData.OutputType = "Device";
                        if (!string.IsNullOrEmpty(extendedData))
                        {
                            var parts = extendedData.Split('|');
                            outputData.Type = parts.Length > 0 ? parts[0] : "";
                            outputData.LocationText = parts.Length > 1 ? parts[1] : "";
                        }
                        outputData.DeviceId = $"Device_{data3}";
                        break;

                    case CEDataHelper.SEND_SMS_OUTPUT:
                        outputData.OutputType = "SendText";
                        if (!string.IsNullOrEmpty(extendedData))
                        {
                            var parts = extendedData.Split('|');
                            outputData.PhoneNumber = parts.Length > 0 ? parts[0] : "";
                            outputData.Message = parts.Length > 1 ? parts[1] : "";
                        }
                        break;

                    case CEDataHelper.SEND_EMAIL_OUTPUT:
                        outputData.OutputType = "SendEmail";
                        if (!string.IsNullOrEmpty(extendedData))
                        {
                            var parts = extendedData.Split('|');
                            outputData.EmailAddress = parts.Length > 0 ? parts[0] : "";
                            outputData.Subject = parts.Length > 1 ? parts[1] : "";
                            outputData.Body = parts.Length > 2 ? parts[2] : "";
                        }
                        break;

                    case CEDataHelper.SEND_API_OUTPUT:
                        outputData.OutputType = "SendApi";
                        outputData.HttpMethod = CEDataHelper.DecodeHttpMethod(data1);
                        outputData.ContentType = CEDataHelper.DecodeContentType(data2);
                        if (!string.IsNullOrEmpty(extendedData))
                        {
                            var parts = extendedData.Split('|');
                            outputData.ApiUrl = parts.Length > 0 ? parts[0] : "";
                            outputData.RequestBody = parts.Length > 1 ? parts[1] : "";
                        }
                        break;
                }

                if (_currentCauseEffect != null)
                {
                    _currentCauseEffect.Outputs.Add(outputData);
                    Debug.WriteLine($"[Download] Received C&E Output: Type={outputData.OutputType}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Download] Error processing C&E output: {ex.Message}");
            }
        }

        private void UpdateProgress(int current, int total, string operation)
        {
            _serialService.OnDownloadProgress(new DownloadProgress
            {
                ReceivedPackets = current,
                TotalPackets = total,
                CurrentOperation = operation
            });
        }
    }
}

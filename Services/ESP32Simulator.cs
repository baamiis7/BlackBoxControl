using System;
using System.Collections.Generic;
using System.Text;
using BlackBoxControl.Protocol;
using BlackBoxControl.Models;
using BlackBoxControl.Helpers;

namespace BlackBoxControl.Services
{
    /// <summary>
    /// Virtual ESP32 simulator for testing upload/download without hardware
    /// </summary>
    public class ESP32Simulator : IDisposable
    {
        private readonly string _portName;
        private List<BlackBoxControlPanelData> _storedPanels = new List<BlackBoxControlPanelData>();
        private BlackBoxControlPanelData _currentPanel;
        private LoopData _currentLoop;
        private BusData _currentBus;
        private CauseAndEffectData _currentCauseEffect; // 🔥 NEW

        // Event to send packets back to PC
        public event Action<BinaryPacket> PacketToSend;

        private ESP32Simulator(string portName)
        {
            _portName = portName;
            System.Diagnostics.Debug.WriteLine($"[ESP32 Simulator] Started on {portName}");
        }

        public static ESP32Simulator CreateVirtualPort(string portName)
        {
            return new ESP32Simulator(portName);
        }

        public void ReceivePacket(BinaryPacket packet)
        {
            System.Diagnostics.Debug.WriteLine($"[ESP32 Simulator] Received packet type: 0x{packet.PacketType:X2}, Data length: {packet.Data.Length}");

            switch (packet.PacketType)
            {
                case ProtocolConstants.PACKET_HANDSHAKE:
                    HandleHandshake(packet);
                    break;

                case ProtocolConstants.PACKET_PANEL_CONFIG:
                    HandlePanelConfig(packet);
                    break;

                case ProtocolConstants.PACKET_LOOP_CONFIG:
                    HandleLoopConfig(packet);
                    break;

                case ProtocolConstants.PACKET_DEVICE_CONFIG:
                    HandleDeviceConfig(packet);
                    break;

                case ProtocolConstants.PACKET_BUS_CONFIG:
                    HandleBusConfig(packet);
                    break;

                case ProtocolConstants.PACKET_BUS_NODE_CONFIG:
                    HandleBusNodeConfig(packet);
                    break;

                // 🔥 NEW: Cause & Effect packet handlers
                case ProtocolConstants.PACKET_CE_HEADER:
                    HandleCEHeader(packet);
                    break;

                case ProtocolConstants.PACKET_CE_INPUT:
                    HandleCEInput(packet);
                    break;

                case ProtocolConstants.PACKET_CE_OUTPUT:
                    HandleCEOutput(packet);
                    break;

                case ProtocolConstants.PACKET_END_TRANSMISSION:
                    HandleEndTransmission(packet);
                    break;

                case ProtocolConstants.PACKET_DOWNLOAD_REQUEST:
                    HandleDownloadRequest(packet);
                    break;

                default:
                    System.Diagnostics.Debug.WriteLine($"[ESP32 Simulator] Unknown packet type: 0x{packet.PacketType:X2}");
                    break;
            }
        }

        public void ClearStoredData()
        {
            _storedPanels.Clear();
            _currentPanel = null;
            _currentLoop = null;
            _currentBus = null;
            _currentCauseEffect = null; // 🔥 NEW
            System.Diagnostics.Debug.WriteLine("[ESP32 Simulator] Cleared all stored data");
        }

        // ==================== HANDLE METHODS ====================

        private void HandleHandshake(BinaryPacket packet)
        {
            System.Diagnostics.Debug.WriteLine("[ESP32 Simulator] Handshake received - sending ACK");
            SendACK();
        }

        private void HandlePanelConfig(BinaryPacket packet)
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

                _storedPanels.Add(_currentPanel);
                System.Diagnostics.Debug.WriteLine($"[ESP32 Simulator] Stored panel: {panelName}");
                SendACK();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ESP32 Simulator] Error in HandlePanelConfig: {ex.Message}");
                throw;
            }
        }

        private void HandleLoopConfig(BinaryPacket packet)
        {
            try
            {
                var data = packet.Data;
                int offset = 0;

                byte panelAddress = data[offset++];
                byte loopNumber = data[offset++];
                string loopName = Encoding.UTF8.GetString(data, offset, 32).TrimEnd('\0');
                offset += 32;
                byte protocolByte = data[offset++];
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

                System.Diagnostics.Debug.WriteLine($"[ESP32 Simulator] Stored loop: {loopName}");
                SendACK();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ESP32 Simulator] Error in HandleLoopConfig: {ex.Message}");
                throw;
            }
        }

        private void HandleDeviceConfig(BinaryPacket packet)
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

                System.Diagnostics.Debug.WriteLine($"[ESP32 Simulator] Stored device: {deviceType} at address {deviceAddress}");
                SendACK();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ESP32 Simulator] Error in HandleDeviceConfig: {ex.Message}");
                throw;
            }
        }

        private void HandleBusConfig(BinaryPacket packet)
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

                System.Diagnostics.Debug.WriteLine($"[ESP32 Simulator] Stored bus: {busName}");
                SendACK();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ESP32 Simulator] Error in HandleBusConfig: {ex.Message}");
                throw;
            }
        }

        private void HandleBusNodeConfig(BinaryPacket packet)
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
                    LocationText = locationText
                };

                if (_currentBus != null)
                {
                    _currentBus.Nodes.Add(node);
                }

                System.Diagnostics.Debug.WriteLine($"[ESP32 Simulator] Stored bus node: {nodeName} at address {nodeAddress}");
                SendACK();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ESP32 Simulator] Error in HandleBusNodeConfig: {ex.Message}");
                throw;
            }
        }

        // 🔥 NEW: Cause & Effect Handlers
        private void HandleCEHeader(BinaryPacket packet)
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
                    System.Diagnostics.Debug.WriteLine($"[ESP32 Simulator] Stored C&E: {ceName}, Logic={logicGateStr}, Inputs={inputCount}, Outputs={outputCount}");
                }

                SendACK();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ESP32 Simulator] Error in HandleCEHeader: {ex.Message}");
                throw;
            }
        }

        private void HandleCEInput(BinaryPacket packet)
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
                    System.Diagnostics.Debug.WriteLine($"[ESP32 Simulator] Stored C&E Input: Type={inputData.InputType}");
                }

                SendACK();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ESP32 Simulator] Error in HandleCEInput: {ex.Message}");
                throw;
            }
        }

        private void HandleCEOutput(BinaryPacket packet)
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
                    System.Diagnostics.Debug.WriteLine($"[ESP32 Simulator] Stored C&E Output: Type={outputData.OutputType}");
                }

                SendACK();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ESP32 Simulator] Error in HandleCEOutput: {ex.Message}");
                throw;
            }
        }

        private void HandleEndTransmission(BinaryPacket packet)
        {
            System.Diagnostics.Debug.WriteLine("[ESP32 Simulator] Upload complete!");
            System.Diagnostics.Debug.WriteLine($"[ESP32 Simulator] Stored {_storedPanels.Count} panels");

            foreach (var panel in _storedPanels)
            {
                System.Diagnostics.Debug.WriteLine($"  - Panel: {panel.PanelName}, Loops: {panel.Loops.Count}, Buses: {panel.Busses.Count}, C&Es: {panel.CauseAndEffects.Count}");
                foreach (var loop in panel.Loops)
                {
                    System.Diagnostics.Debug.WriteLine($"    - Loop {loop.LoopNumber}: {loop.Devices.Count} devices");
                }
                foreach (var bus in panel.Busses)
                {
                    System.Diagnostics.Debug.WriteLine($"    - Bus: {bus.Nodes.Count} nodes");
                }
                foreach (var ce in panel.CauseAndEffects)
                {
                    System.Diagnostics.Debug.WriteLine($"    - C&E: {ce.Name}, Inputs={ce.Inputs.Count}, Outputs={ce.Outputs.Count}");
                }
            }

            for (int i = 1; i <= 5; i++)
            {
                System.Diagnostics.Debug.WriteLine($"[ESP32 Simulator] LED BLINK {i}/5");
                System.Threading.Thread.Sleep(200);
            }

            SendACK();
        }

        private void HandleDownloadRequest(BinaryPacket packet)
        {
            System.Diagnostics.Debug.WriteLine("[ESP32 Simulator] Download request received");
            SendACK();

            if (_storedPanels.Count == 0)
            {
                System.Diagnostics.Debug.WriteLine("[ESP32 Simulator] No configuration stored");
                SendEndTransmission();
                return;
            }

            System.Diagnostics.Debug.WriteLine($"[ESP32 Simulator] Sending {_storedPanels.Count} panel(s)");

            foreach (var panel in _storedPanels)
            {
                SendPanelConfig(panel);

                foreach (var loop in panel.Loops)
                {
                    SendLoopConfig(loop, (byte)panel.PanelAddress);

                    foreach (var device in loop.Devices)
                    {
                        SendDeviceConfig(device, (byte)panel.PanelAddress, loop.LoopNumber);
                    }
                }

                foreach (var bus in panel.Busses)
                {
                    SendBusConfig(bus, (byte)panel.PanelAddress);

                    foreach (var node in bus.Nodes)
                    {
                        SendBusNodeConfig(node, (byte)panel.PanelAddress, bus.BusNumber);
                    }
                }

                // 🔥 NEW: Send Cause & Effects
                foreach (var ce in panel.CauseAndEffects)
                {
                    SendCEHeader(ce, (byte)panel.PanelAddress);

                    foreach (var input in ce.Inputs)
                    {
                        SendCEInput(input, (byte)panel.PanelAddress);
                    }

                    foreach (var output in ce.Outputs)
                    {
                        SendCEOutput(output, (byte)panel.PanelAddress);
                    }
                }
            }

            SendEndTransmission();
            System.Diagnostics.Debug.WriteLine("[ESP32 Simulator] Download complete!");
        }

        // ==================== SEND METHODS ====================

        private void SendACK()
        {
            var packet = new BinaryPacket(ProtocolConstants.PACKET_ACK, new byte[0]);
            PacketToSend?.Invoke(packet);
        }

        private void SendPanelConfig(BlackBoxControlPanelData panel)
        {
            var data = new List<byte>();
            data.Add((byte)panel.PanelAddress);
            data.AddRange(GetFixedString(panel.PanelName, 32));
            data.AddRange(GetFixedString(panel.Location, 32));
            data.Add((byte)panel.NumberOfLoops);
            data.Add((byte)panel.NumberOfZones);

            var packet = new BinaryPacket(ProtocolConstants.PACKET_PANEL_CONFIG, data.ToArray());
            PacketToSend?.Invoke(packet);
            System.Diagnostics.Debug.WriteLine($"[ESP32 Simulator] Sent panel: {panel.PanelName}");
        }

        private void SendLoopConfig(LoopData loop, byte panelAddress)
        {
            var data = new List<byte>();
            data.Add(panelAddress);
            data.Add((byte)loop.LoopNumber);
            data.AddRange(GetFixedString(loop.LoopName, 32));
            data.Add(0); // protocol
            data.Add((byte)loop.Devices.Count);

            var packet = new BinaryPacket(ProtocolConstants.PACKET_LOOP_CONFIG, data.ToArray());
            PacketToSend?.Invoke(packet);
            System.Diagnostics.Debug.WriteLine($"[ESP32 Simulator] Sent loop: {loop.LoopName}");
        }

        private void SendDeviceConfig(LoopDeviceData device, byte panelAddress, int loopNumber)
        {
            var data = new List<byte>();
            data.Add(panelAddress);
            data.Add((byte)loopNumber);
            data.Add((byte)device.Address);
            data.Add(DeviceTypeMapper.GetDeviceTypeCode(device.Type));
            data.AddRange(GetFixedString(device.LocationText, 32));
            data.Add((byte)device.Zone);

            var packet = new BinaryPacket(ProtocolConstants.PACKET_DEVICE_CONFIG, data.ToArray());
            PacketToSend?.Invoke(packet);
            System.Diagnostics.Debug.WriteLine($"[ESP32 Simulator] Sent device: {device.Type}");
        }

        private void SendBusConfig(BusData bus, byte panelAddress)
        {
            var data = new List<byte>();
            data.Add(panelAddress);
            data.Add((byte)bus.BusNumber);
            data.AddRange(GetFixedString(bus.BusName, 32));
            data.Add((byte)(bus.BusType == "CAN" ? 1 : 0)); // bus type
            data.Add((byte)bus.Nodes.Count);

            var packet = new BinaryPacket(ProtocolConstants.PACKET_BUS_CONFIG, data.ToArray());
            PacketToSend?.Invoke(packet);
            System.Diagnostics.Debug.WriteLine($"[ESP32 Simulator] Sent bus: {bus.BusName}");
        }

        private void SendBusNodeConfig(BusNodeData node, byte panelAddress, int busNumber)
        {
            var data = new List<byte>();
            data.Add(panelAddress);
            data.Add((byte)busNumber);
            data.Add((byte)node.Address);
            data.AddRange(GetFixedString(node.Name, 32));
            data.AddRange(GetFixedString(node.LocationText, 32));

            var packet = new BinaryPacket(ProtocolConstants.PACKET_BUS_NODE_CONFIG, data.ToArray());
            PacketToSend?.Invoke(packet);
            System.Diagnostics.Debug.WriteLine($"[ESP32 Simulator] Sent bus node: {node.Name}");
        }

        // 🔥 NEW: C&E Send Methods
        private void SendCEHeader(CauseAndEffectData ce, byte panelAddress)
        {
            var data = new List<byte>();
            data.Add(panelAddress);
            data.AddRange(GetFixedString(ce.Name, 32));
            data.Add((byte)(ce.IsEnabled ? 1 : 0));

            byte logicGate = ce.LogicGate?.ToUpper() switch
            {
                "AND" => 0x01,
                "XOR" => 0x02,
                _ => 0x00
            };
            data.Add(logicGate);
            data.Add((byte)ce.Inputs.Count);
            data.Add((byte)ce.Outputs.Count);

            var packet = new BinaryPacket(ProtocolConstants.PACKET_CE_HEADER, data.ToArray());
            PacketToSend?.Invoke(packet);
            System.Diagnostics.Debug.WriteLine($"[ESP32 Simulator] Sent C&E: {ce.Name}");
        }

        private void SendCEInput(CauseInputData input, byte panelAddress)
        {
            var data = new List<byte>();
            data.Add(panelAddress);

            byte addressType = 0;
            byte data1 = 0, data2 = 0, data3 = 0, data4 = 0;
            string extendedData = "";

            switch (input.InputType)
            {
                case "Device":
                    addressType = CEDataHelper.DEVICE_INPUT;
                    extendedData = $"{input.Type}|{input.LocationText}";
                    break;

                case "TimeOfDay":
                    addressType = CEDataHelper.TIME_OF_DAY_INPUT;
                    var start = TimeSpan.Parse(input.StartTime ?? "00:00");
                    var end = TimeSpan.Parse(input.EndTime ?? "00:00");
                    data1 = (byte)start.Hours;
                    data2 = (byte)start.Minutes;
                    data3 = (byte)end.Hours;
                    data4 = (byte)end.Minutes;
                    break;

                case "DateTime":
                    addressType = CEDataHelper.DATETIME_INPUT;
                    var dt = input.TriggerDateTime ?? DateTime.Now;
                    data1 = (byte)dt.Month;
                    data2 = (byte)dt.Day;
                    data3 = (byte)dt.Hour;
                    data4 = (byte)dt.Minute;
                    extendedData = dt.Year.ToString();
                    break;

                case "ReceiveApi":
                    addressType = CEDataHelper.API_WEBHOOK_INPUT;
                    data1 = input.HttpMethod?.ToUpper() switch
                    {
                        "POST" => CEDataHelper.HTTP_POST,
                        _ => CEDataHelper.HTTP_GET
                    };
                    extendedData = $"{input.ListenUrl}|{input.ExpectedPath}|{input.AuthToken}";
                    break;
            }

            data.Add(addressType);
            data.Add(data1);
            data.Add(data2);
            data.Add(data3);
            data.Add(data4);
            data.AddRange(GetFixedString(extendedData, 64));

            var packet = new BinaryPacket(ProtocolConstants.PACKET_CE_INPUT, data.ToArray());
            PacketToSend?.Invoke(packet);
        }

        private void SendCEOutput(EffectOutputData output, byte panelAddress)
        {
            var data = new List<byte>();
            data.Add(panelAddress);

            byte addressType = 0;
            byte data1 = 0, data2 = 0, data3 = 0, data4 = 0;
            string extendedData = "";

            switch (output.OutputType)
            {
                case "Device":
                    addressType = CEDataHelper.DEVICE_OUTPUT;
                    extendedData = $"{output.Type}|{output.LocationText}";
                    break;

                case "SendText":
                    addressType = CEDataHelper.SEND_SMS_OUTPUT;
                    extendedData = $"{output.PhoneNumber}|{output.Message}";
                    break;

                case "SendEmail":
                    addressType = CEDataHelper.SEND_EMAIL_OUTPUT;
                    extendedData = $"{output.EmailAddress}|{output.Subject}|{output.Body}";
                    break;

                case "SendApi":
                    addressType = CEDataHelper.SEND_API_OUTPUT;
                    data1 = output.HttpMethod?.ToUpper() switch
                    {
                        "POST" => CEDataHelper.HTTP_POST,
                        _ => CEDataHelper.HTTP_GET
                    };
                    data2 = output.ContentType?.ToLower() switch
                    {
                        "application/xml" => CEDataHelper.CONTENT_XML,
                        _ => CEDataHelper.CONTENT_JSON
                    };
                    extendedData = $"{output.ApiUrl}|{output.RequestBody}";
                    break;
            }

            data.Add(addressType);
            data.Add(data1);
            data.Add(data2);
            data.Add(data3);
            data.Add(data4);
            data.AddRange(GetFixedString(extendedData, 64));

            var packet = new BinaryPacket(ProtocolConstants.PACKET_CE_OUTPUT, data.ToArray());
            PacketToSend?.Invoke(packet);
        }

        private void SendEndTransmission()
        {
            var packet = new BinaryPacket(ProtocolConstants.PACKET_END_TRANSMISSION, new byte[0]);
            PacketToSend?.Invoke(packet);
        }

        private byte[] GetFixedString(string text, int length)
        {
            var bytes = new byte[length];
            if (!string.IsNullOrEmpty(text))
            {
                var encoded = Encoding.UTF8.GetBytes(text);
                int copyLength = Math.Min(encoded.Length, length);
                Array.Copy(encoded, bytes, copyLength);
            }
            return bytes;
        }

        public bool HasData()
        {
            return _storedPanels != null && _storedPanels.Count > 0;
        }

        public void Dispose()
        {
            System.Diagnostics.Debug.WriteLine($"[ESP32 Simulator] Disposed");
        }
    }
}

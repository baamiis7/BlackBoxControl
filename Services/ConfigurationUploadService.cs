using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BlackBoxControl.Helpers;
using BlackBoxControl.Models;
using BlackBoxControl.Protocol;

namespace BlackBoxControl.Services
{
    public class ConfigurationUploadService
    {
        private readonly SerialCommunicationService _serialService;

        public ConfigurationUploadService(SerialCommunicationService serialService)
        {
            _serialService = serialService ?? throw new ArgumentNullException(nameof(serialService));
        }

        public async Task<bool> UploadConfigurationAsync(
            ProjectData project,
            CancellationToken cancellationToken = default)
        {
            if (project == null)
                throw new ArgumentNullException(nameof(project));

            if (!_serialService.IsConnected)
                throw new InvalidOperationException("Not connected to ESP32");

            // Clear simulator before upload if using simulator
            if (_serialService is MockSerialCommunicationService mockService)
            {
                var simulator = ESP32SimulatorManager.Instance;
                simulator.ClearStoredData();
                System.Diagnostics.Debug.WriteLine("[Upload] Cleared simulator data before new upload");
            }

            try
            {
                int totalPackets = CalculateTotalPackets(project);
                int sentPackets = 0;

                UpdateProgress(sentPackets, totalPackets, "Starting upload...");

                foreach (var panel in project.BlackBoxControlPanels)
                {
                    UpdateProgress(sentPackets++, totalPackets, $"Uploading panel: {panel.PanelName}");
                    if (!await UploadPanelAsync(panel, cancellationToken))
                        return false;

                    foreach (var loop in panel.Loops)
                    {
                        UpdateProgress(sentPackets++, totalPackets, $"Uploading loop {loop.LoopNumber}");
                        if (!await UploadLoopAsync(panel, loop, cancellationToken))
                            return false;

                        foreach (var device in loop.Devices)
                        {
                            UpdateProgress(sentPackets++, totalPackets, $"Uploading device {device.Address}");
                            if (!await UploadDeviceAsync(panel, loop, device, cancellationToken))
                                return false;
                        }
                    }

                    // Upload buses
                    foreach (var bus in panel.Busses)
                    {
                        UpdateProgress(sentPackets++, totalPackets, $"Uploading bus: {bus.BusName}");
                        if (!await UploadBusAsync(panel, bus, cancellationToken))
                            return false;

                        foreach (var node in bus.Nodes)
                        {
                            UpdateProgress(sentPackets++, totalPackets, $"Uploading bus node {node.Address}");
                            if (!await UploadBusNodeAsync(panel, bus, node, cancellationToken))
                                return false;
                        }
                    }

                    // Upload Cause & Effects
                    if (panel.CauseAndEffects != null && panel.CauseAndEffects.Count > 0)
                    {
                        foreach (var ce in panel.CauseAndEffects)
                        {
                            UpdateProgress(sentPackets++, totalPackets, $"Uploading C&E: {ce.Name}");
                            if (!await UploadCauseEffectAsync(panel, ce, cancellationToken))
                                return false;

                            sentPackets += (ce.Inputs?.Count ?? 0) + (ce.Outputs?.Count ?? 0);
                        }
                    }
                }

                UpdateProgress(sentPackets++, totalPackets, "Finalizing...");
                if (!await SendEndTransmissionAsync(cancellationToken))
                    return false;

                UpdateProgress(totalPackets, totalPackets, "Upload complete!");
                return true;
            }
            catch (Exception ex)
            {
                UpdateProgress(0, 100, $"Upload failed: {ex.Message}");
                throw;
            }
        }

        private async Task<bool> UploadPanelAsync(BlackBoxControlPanelData panel, CancellationToken cancellationToken)
        {
            var data = new List<byte>();

            data.Add((byte)panel.PanelAddress);
            data.AddRange(GetFixedString(panel.PanelName ?? "Panel", 32));
            data.AddRange(GetFixedString(panel.Location ?? "", 32));
            data.Add((byte)(panel.Loops?.Count ?? 0));
            data.Add((byte)(panel.Busses?.Count ?? 0));

            var packet = new BinaryPacket(ProtocolConstants.PACKET_PANEL_CONFIG, data.ToArray());
            return await _serialService.SendPacketWithAckAsync(packet, cancellationToken);
        }

        private async Task<bool> UploadLoopAsync(BlackBoxControlPanelData panel, LoopData loop, CancellationToken cancellationToken)
        {
            var data = new List<byte>();

            data.Add((byte)panel.PanelAddress);
            data.Add((byte)loop.LoopNumber);
            data.AddRange(GetFixedString(loop.LoopName ?? $"Loop {loop.LoopNumber}", 32));
            data.Add(0); // Protocol: 0=Standard, 1=Advanced
            data.Add((byte)(loop.Devices?.Count ?? 0));

            var packet = new BinaryPacket(ProtocolConstants.PACKET_LOOP_CONFIG, data.ToArray());
            return await _serialService.SendPacketWithAckAsync(packet, cancellationToken);
        }

        private async Task<bool> UploadDeviceAsync(BlackBoxControlPanelData panel, LoopData loop, LoopDeviceData device, CancellationToken cancellationToken)
        {
            var data = new List<byte>();

            data.Add((byte)panel.PanelAddress);
            data.Add((byte)loop.LoopNumber);
            data.Add((byte)device.Address);

            byte typeCode = GetDeviceTypeCode(device.Type);
            System.Diagnostics.Debug.WriteLine($"[Upload] Device Addr={device.Address}, OriginalType='{device.Type}', TypeCode=0x{typeCode:X2}, Location='{device.LocationText}'");

            data.Add(typeCode);
            data.AddRange(GetFixedString(device.LocationText ?? "", 32));
            data.Add((byte)device.Zone);

            var packet = new BinaryPacket(ProtocolConstants.PACKET_DEVICE_CONFIG, data.ToArray());
            return await _serialService.SendPacketWithAckAsync(packet, cancellationToken);
        }

        private async Task<bool> UploadBusAsync(BlackBoxControlPanelData panel, BusData bus, CancellationToken cancellationToken)
        {
            System.Diagnostics.Debug.WriteLine($"[Upload] Uploading bus: {bus.BusName}, BusNumber={bus.BusNumber}, Nodes={bus.Nodes?.Count ?? 0}");

            var data = new List<byte>();

            data.Add((byte)panel.PanelAddress);
            data.Add((byte)bus.BusNumber);
            data.AddRange(GetFixedString(bus.BusName ?? "Bus", 32));
            byte busTypeByte = bus.BusType == "CAN" ? (byte)1 : (byte)0;
            data.Add(busTypeByte);
            data.Add((byte)(bus.Nodes?.Count ?? 0));

            System.Diagnostics.Debug.WriteLine($"[Upload] Bus packet data - Panel={panel.PanelAddress}, BusNum={bus.BusNumber}, Name={bus.BusName}, Nodes={bus.Nodes?.Count ?? 0}");

            var packet = new BinaryPacket(ProtocolConstants.PACKET_BUS_CONFIG, data.ToArray());
            return await _serialService.SendPacketWithAckAsync(packet, cancellationToken);
        }

        private async Task<bool> UploadBusNodeAsync(BlackBoxControlPanelData panel, BusData bus, BusNodeData node, CancellationToken cancellationToken)
        {
            var data = new List<byte>();

            data.Add((byte)panel.PanelAddress);
            data.Add((byte)bus.BusNumber);
            data.Add((byte)node.Address);
            data.AddRange(GetFixedString(node.Name ?? "", 32));
            data.AddRange(GetFixedString(node.LocationText ?? "", 32));

            var packet = new BinaryPacket(ProtocolConstants.PACKET_BUS_NODE_CONFIG, data.ToArray());
            return await _serialService.SendPacketWithAckAsync(packet, cancellationToken);
        }

        // ========================================
        // CAUSE & EFFECT UPLOAD METHODS
        // ========================================

        private async Task<bool> UploadCauseEffectAsync(
            BlackBoxControlPanelData panel,
            CauseAndEffectData ce,
            CancellationToken cancellationToken)
        {
            var data = new List<byte>();

            // Panel number
            data.Add((byte)panel.PanelAddress);

            // CE Name (32 bytes)
            data.AddRange(GetFixedString(ce.Name ?? "Unnamed C&E", 32));

            // IsEnabled
            data.Add((byte)(ce.IsEnabled ? 1 : 0));

            // Logic Gate
            byte logicGate = ce.LogicGate?.ToUpper() switch
            {
                "AND" => 0x01,
                "XOR" => 0x02,
                _ => 0x00  // OR
            };
            data.Add(logicGate);

            // Input count
            data.Add((byte)(ce.Inputs?.Count ?? 0));

            // Output count
            data.Add((byte)(ce.Outputs?.Count ?? 0));

            System.Diagnostics.Debug.WriteLine(
                $"[Upload] C&E: {ce.Name}, Logic={ce.LogicGate}, Inputs={ce.Inputs?.Count ?? 0}, Outputs={ce.Outputs?.Count ?? 0}");

            // Send CE header
            var packet = new BinaryPacket(ProtocolConstants.PACKET_CE_HEADER, data.ToArray());
            if (!await _serialService.SendPacketWithAckAsync(packet, cancellationToken))
                return false;

            // Upload inputs
            if (ce.Inputs != null)
            {
                foreach (var input in ce.Inputs)
                {
                    if (cancellationToken.IsCancellationRequested)
                        return false;

                    if (!await UploadCEInputAsync(panel, input, cancellationToken))
                        return false;
                }
            }

            // Upload outputs
            if (ce.Outputs != null)
            {
                foreach (var output in ce.Outputs)
                {
                    if (cancellationToken.IsCancellationRequested)
                        return false;

                    if (!await UploadCEOutputAsync(panel, output, cancellationToken))
                        return false;
                }
            }

            return true;
        }

        private async Task<bool> UploadCEInputAsync(
            BlackBoxControlPanelData panel,
            CauseInputData input,
            CancellationToken cancellationToken)
        {
            var data = new List<byte>();

            // Panel number
            data.Add((byte)panel.PanelAddress);

            // Determine input type and encode data
            byte addressType = 0;
            byte data1 = 0, data2 = 0, data3 = 0, data4 = 0;
            var extendedData = "";

            switch (input.InputType)
            {
                case "Device":
                    addressType = CEDataHelper.DEVICE_INPUT;
                    ParseDeviceId(input.DeviceId, out data1, out data2, out data3, out data4);
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
                        "PUT" => CEDataHelper.HTTP_PUT,
                        "DELETE" => CEDataHelper.HTTP_DELETE,
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

            // Extended data (64 bytes)
            data.AddRange(GetFixedString(extendedData, 64));

            System.Diagnostics.Debug.WriteLine(
                $"[Upload] CE Input: Type={input.InputType}, AddressType=0x{addressType:X2}");

            var packet = new BinaryPacket(ProtocolConstants.PACKET_CE_INPUT, data.ToArray());
            return await _serialService.SendPacketWithAckAsync(packet, cancellationToken);
        }

        private async Task<bool> UploadCEOutputAsync(
            BlackBoxControlPanelData panel,
            EffectOutputData output,
            CancellationToken cancellationToken)
        {
            var data = new List<byte>();

            // Panel number
            data.Add((byte)panel.PanelAddress);

            // Determine output type and encode data
            byte addressType = 0;
            byte data1 = 0, data2 = 0, data3 = 0, data4 = 0;
            var extendedData = "";

            switch (output.OutputType)
            {
                case "Device":
                    addressType = CEDataHelper.DEVICE_OUTPUT;
                    ParseDeviceId(output.DeviceId, out data1, out data2, out data3, out data4);
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
                        "PUT" => CEDataHelper.HTTP_PUT,
                        "DELETE" => CEDataHelper.HTTP_DELETE,
                        _ => CEDataHelper.HTTP_GET
                    };
                    data2 = output.ContentType?.ToLower() switch
                    {
                        "application/xml" => CEDataHelper.CONTENT_XML,
                        "text/plain" => CEDataHelper.CONTENT_TEXT,
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

            // Extended data (64 bytes)
            data.AddRange(GetFixedString(extendedData, 64));

            System.Diagnostics.Debug.WriteLine(
                $"[Upload] CE Output: Type={output.OutputType}, AddressType=0x{addressType:X2}");

            var packet = new BinaryPacket(ProtocolConstants.PACKET_CE_OUTPUT, data.ToArray());
            return await _serialService.SendPacketWithAckAsync(packet, cancellationToken);
        }

        private void ParseDeviceId(string deviceId, out byte panel, out byte loopOrBus, out byte address, out byte subAddress)
        {
            panel = 1;
            loopOrBus = 1;
            address = 1;
            subAddress = 0;

            if (string.IsNullOrEmpty(deviceId))
                return;

            try
            {
                if (deviceId.Contains("BusNode_"))
                {
                    var parts = deviceId.Split('_');
                    if (parts.Length >= 2 && byte.TryParse(parts[1], out byte addr))
                    {
                        loopOrBus = 255;  // Special marker for bus
                        address = addr;
                    }
                }
                else if (deviceId.Contains("_"))
                {
                    var parts = deviceId.Split('_');
                    if (parts.Length >= 2 && byte.TryParse(parts[1], out byte addr))
                    {
                        address = addr;
                    }
                }
            }
            catch
            {
                // Use defaults
            }
        }

        private async Task<bool> SendEndTransmissionAsync(CancellationToken cancellationToken)
        {
            var packet = new BinaryPacket(ProtocolConstants.PACKET_END_TRANSMISSION, new byte[0]);
            return await _serialService.SendPacketWithAckAsync(packet, cancellationToken);
        }

        private int CalculateTotalPackets(ProjectData project)
        {
            int count = 0;
            foreach (var panel in project.BlackBoxControlPanels)
            {
                count++; // Panel

                foreach (var loop in panel.Loops)
                {
                    count++; // Loop
                    count += loop.Devices?.Count ?? 0; // Devices
                }

                foreach (var bus in panel.Busses)
                {
                    count++; // Bus
                    count += bus.Nodes?.Count ?? 0; // Bus nodes
                }

                // Cause & Effects
                foreach (var ce in panel.CauseAndEffects)
                {
                    count++; // CE header
                    count += ce.Inputs?.Count ?? 0; // Inputs
                    count += ce.Outputs?.Count ?? 0; // Outputs
                }
            }
            count++; // End transmission
            return count;
        }

        private byte GetDeviceTypeCode(string deviceType)
        {
            return DeviceTypeMapper.GetDeviceTypeCode(deviceType);
        }

        private byte[] GetFixedString(string text, int length)
        {
            var bytes = new byte[length];
            if (!string.IsNullOrEmpty(text))
            {
                var encoded = System.Text.Encoding.UTF8.GetBytes(text);
                int copyLength = Math.Min(encoded.Length, length);
                Array.Copy(encoded, bytes, copyLength);
            }
            return bytes;
        }

        private void UpdateProgress(int sent, int total, string operation)
        {
            _serialService.OnUploadProgress(new UploadProgress
            {
                SentPackets = sent,
                TotalPackets = total,
                CurrentOperation = operation
            });
        }
    }
}

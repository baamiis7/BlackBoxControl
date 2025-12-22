using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
                            if (!await UploadDeviceAsync(loop, device, cancellationToken))
                                return false;
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
            var writer = new ProtocolWriter();
            writer.WriteByte((byte)panel.GetHashCode());
            writer.WriteString(panel.PanelName ?? "Panel", 32);
            writer.WriteString(panel.Location ?? "", 64);
            writer.WriteIPAddress("192.168.1.100"); // Default IP
            writer.WriteByte((byte)(panel.Loops?.Count ?? 0));
            writer.WriteByte(0); // No buses for now

            var packet = new BinaryPacket(ProtocolConstants.PACKET_PANEL_CONFIG, writer.ToArray());
            return await _serialService.SendPacketWithAckAsync(packet, cancellationToken);
        }

        private async Task<bool> UploadLoopAsync(BlackBoxControlPanelData panel, LoopData loop, CancellationToken cancellationToken)
        {
            var writer = new ProtocolWriter();
            writer.WriteByte((byte)panel.GetHashCode());
            writer.WriteByte((byte)loop.LoopNumber);
            writer.WriteString(loop.LoopName ?? $"Loop {loop.LoopNumber}", 32);
            writer.WriteByte(127);
            writer.WriteByte((byte)(loop.Devices?.Count ?? 0));

            var packet = new BinaryPacket(ProtocolConstants.PACKET_LOOP_CONFIG, writer.ToArray());
            return await _serialService.SendPacketWithAckAsync(packet, cancellationToken);
        }

        private async Task<bool> UploadDeviceAsync(LoopData loop, LoopDeviceData device, CancellationToken cancellationToken)
        {
            var writer = new ProtocolWriter();
            writer.WriteByte((byte)loop.LoopNumber);
            writer.WriteByte((byte)device.Address);
            writer.WriteByte(GetDeviceTypeCode(device.Type));
            writer.WriteString(device.LocationText ?? "", 64);
            writer.WriteBoolean(true);
            writer.WriteByte((byte)device.Zone);

            var packet = new BinaryPacket(ProtocolConstants.PACKET_DEVICE_CONFIG, writer.ToArray());
            return await _serialService.SendPacketWithAckAsync(packet, cancellationToken);
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
                count++;
                foreach (var loop in panel.Loops)
                {
                    count++;
                    count += loop.Devices?.Count ?? 0;
                }
            }
            count++;
            return count;
        }

        private byte GetDeviceTypeCode(string deviceType)
        {
            if (string.IsNullOrEmpty(deviceType))
                return 0xFF;

            return deviceType.ToLower() switch
            {
                "smoke detector" => 0x01,
                "heat detector" => 0x02,
                "manual call point" => 0x03,
                _ => 0xFF
            };
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

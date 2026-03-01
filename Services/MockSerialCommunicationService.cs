using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using BlackBoxControl.Protocol;

namespace BlackBoxControl.Services
{
    public class MockSerialCommunicationService : SerialCommunicationService
    {
        private bool _useSimulator;
        private ESP32Simulator? _simulator;
        private bool _simulatorConnected;
        private readonly Channel<BinaryPacket> _ackChannel = Channel.CreateUnbounded<BinaryPacket>();
        private readonly Channel<BinaryPacket> _dataChannel = Channel.CreateUnbounded<BinaryPacket>();

        public override void EnableSimulator(bool enabled)
        {
            _useSimulator = enabled;

            if (enabled)
            {
                // Use the singleton instance instead of creating a new one
                _simulator = ESP32SimulatorManager.Instance;

                // Subscribe to packets from simulator
                _simulator.PacketToSend += OnSimulatorPacket;

                System.Diagnostics.Debug.WriteLine("ESP32 Simulator enabled (using shared instance)");
            }
            else
            {
                if (_simulator != null)
                {
                    _simulator.PacketToSend -= OnSimulatorPacket;
                    // Don't dispose - it's a shared instance!
                    _simulator = null;
                }
                System.Diagnostics.Debug.WriteLine("ESP32 Simulator disabled");
            }
        }

        private void OnSimulatorPacket(BinaryPacket packet)
        {
            if (packet.PacketType == ProtocolConstants.PACKET_ACK ||
                packet.PacketType == ProtocolConstants.PACKET_NACK)
                _ackChannel.Writer.TryWrite(packet);
            else
                _dataChannel.Writer.TryWrite(packet);

            System.Diagnostics.Debug.WriteLine($"[MockService] Queued packet from simulator: 0x{packet.PacketType:X2}");
        }

        public override bool IsConnected
        {
            get
            {
                if (_useSimulator)
                    return _simulatorConnected;
                return base.IsConnected;
            }
            protected set
            {
                if (_useSimulator)
                    _simulatorConnected = value;
                else
                    base.IsConnected = value;
            }
        }

        public override async Task<bool> ConnectAsync(string portName, CancellationToken cancellationToken = default)
        {
            if (_useSimulator)
            {
                System.Diagnostics.Debug.WriteLine("MockService: Connecting in simulator mode");
                await Task.Delay(100, cancellationToken);
                _simulatorConnected = true;
                OnMessage("Connected to Virtual ESP32 Simulator");
                System.Diagnostics.Debug.WriteLine($"MockService: Simulator connected = {_simulatorConnected}");
                return true;
            }

            return await base.ConnectAsync(portName, cancellationToken);
        }

        public override void Disconnect()
        {
            if (_useSimulator)
            {
                System.Diagnostics.Debug.WriteLine("MockService: Disconnecting simulator");
                _simulatorConnected = false;

                // DON'T drain channels here - download might need pending packets!

                OnMessage("Disconnected from Virtual ESP32 Simulator");
            }
            else
            {
                base.Disconnect();
            }
        }

        public override async Task SendPacketAsync(BinaryPacket packet, CancellationToken cancellationToken)
        {
            if (_useSimulator && _simulator != null)
            {
                System.Diagnostics.Debug.WriteLine($"[MockService] SendPacketAsync: type=0x{packet.PacketType:X2}, simulator={(_simulator != null ? "EXISTS" : "NULL")}, connected={_simulatorConnected}");

                if (!_simulatorConnected)
                    throw new InvalidOperationException("Not connected to simulator");

                System.Diagnostics.Debug.WriteLine($"[MockService] About to call simulator.ReceivePacket...");
                var sim = _simulator;
                await Task.Run(() => sim!.ReceivePacket(packet), cancellationToken);
                System.Diagnostics.Debug.WriteLine($"[MockService] Returned from simulator.ReceivePacket");

                await Task.Delay(50, cancellationToken);
                return;
            }

            await base.SendPacketAsync(packet, cancellationToken);
        }

        // CRITICAL: Override WaitForAckAsync to handle simulator mode
        protected override async Task<bool> WaitForAckAsync(int timeoutMs, CancellationToken cancellationToken)
        {
            if (_useSimulator)
            {
                using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                timeoutCts.CancelAfter(timeoutMs);
                try
                {
                    var ack = await _ackChannel.Reader.ReadAsync(timeoutCts.Token);
                    System.Diagnostics.Debug.WriteLine($"[MockService] Received ACK/packet: 0x{ack.PacketType:X2}");
                    return ack.PacketType == ProtocolConstants.PACKET_ACK;
                }
                catch (OperationCanceledException)
                {
                    System.Diagnostics.Debug.WriteLine("[MockService] ACK timeout");
                    return false;
                }
            }

            // If not simulator, call base implementation
            return await base.WaitForAckAsync(timeoutMs, cancellationToken);
        }

        public override async Task<BinaryPacket?> ReceivePacketAsync(CancellationToken cancellationToken)
        {
            if (_useSimulator)
            {
                using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                timeoutCts.CancelAfter(5000);
                try
                {
                    var packet = await _dataChannel.Reader.ReadAsync(timeoutCts.Token);
                    System.Diagnostics.Debug.WriteLine($"[MockService] Returning packet: 0x{packet.PacketType:X2}");
                    return packet;
                }
                catch (OperationCanceledException)
                {
                    System.Diagnostics.Debug.WriteLine("[MockService] Timeout waiting for packet");
                    return null;
                }
            }

            // For real hardware, implement actual receive
            throw new NotImplementedException("Real hardware receive not implemented");
        }

        private void ClearStoredData()
        {
            while (_ackChannel.Reader.TryRead(out _)) { }
            while (_dataChannel.Reader.TryRead(out _)) { }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_simulator != null)
                {
                    _simulator.PacketToSend -= OnSimulatorPacket;
                    // Don't dispose the shared simulator instance
                    _simulator = null;
                }
            }
            base.Dispose(disposing);
        }
    }
}

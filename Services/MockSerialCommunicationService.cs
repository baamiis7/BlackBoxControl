using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BlackBoxControl.Protocol;

namespace BlackBoxControl.Services
{
    public class MockSerialCommunicationService : SerialCommunicationService
    {
        private bool _useSimulator;
        private ESP32Simulator _simulator;
        private bool _simulatorConnected;
        private readonly object _packetLock = new object();
        private Queue<BinaryPacket> _receivedPackets = new Queue<BinaryPacket>();

        public void EnableSimulator(bool enable = true)
        {
            _useSimulator = enable;

            if (enable)
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
            lock (_packetLock)
            {
                _receivedPackets.Enqueue(packet);
                System.Diagnostics.Debug.WriteLine($"[MockService] Queued packet from simulator: 0x{packet.PacketType:X2}");
            }
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

                // DON'T clear the packet queue here - download might need them!

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
                await Task.Run(() => _simulator.ReceivePacket(packet), cancellationToken);
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
                // In simulator mode, ACKs are sent via the packet queue
                var startTime = DateTime.Now;

                while ((DateTime.Now - startTime).TotalMilliseconds < timeoutMs)
                {
                    if (cancellationToken.IsCancellationRequested)
                        return false;

                    lock (_packetLock)
                    {
                        if (_receivedPackets.Count > 0)
                        {
                            var packet = _receivedPackets.Dequeue();
                            System.Diagnostics.Debug.WriteLine($"[MockService] Received ACK/packet: 0x{packet.PacketType:X2}");

                            if (packet.PacketType == ProtocolConstants.PACKET_ACK)
                            {
                                return true;
                            }

                            // If it's not an ACK, put it back for later
                            _receivedPackets.Enqueue(packet);
                        }
                    }

                    await Task.Delay(10, cancellationToken);
                }

                System.Diagnostics.Debug.WriteLine("[MockService] ACK timeout");
                return false;
            }

            // If not simulator, call base implementation
            return await base.WaitForAckAsync(timeoutMs, cancellationToken);
        }

        public override async Task<BinaryPacket> ReceivePacketAsync(CancellationToken cancellationToken)
        {
            if (_useSimulator)
            {
                var startTime = DateTime.Now;
                var timeoutSeconds = 5; // 5 second timeout per packet

                while ((DateTime.Now - startTime).TotalSeconds < timeoutSeconds)
                {
                    if (cancellationToken.IsCancellationRequested)
                        return null;

                    lock (_packetLock)
                    {
                        if (_receivedPackets.Count > 0)
                        {
                            var packet = _receivedPackets.Dequeue();
                            System.Diagnostics.Debug.WriteLine($"[MockService] Returning packet: 0x{packet.PacketType:X2}");
                            return packet;
                        }
                    }

                    await Task.Delay(10, cancellationToken);
                }

                System.Diagnostics.Debug.WriteLine("[MockService] Timeout waiting for packet");
                return null;
            }

            // For real hardware, implement actual receive
            throw new NotImplementedException("Real hardware receive not implemented");
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

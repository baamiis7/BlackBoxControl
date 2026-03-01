using System;
using System.Threading;
using System.Threading.Tasks;
using BlackBoxControl.Protocol;

namespace BlackBoxControl.Services
{
    /// <summary>
    /// Abstracts the serial port so that ViewModels can work with real hardware or the
    /// built-in simulator interchangeably.
    /// </summary>
    /// <remarks>
    /// The real implementation is <see cref="SerialCommunicationService"/> (opens a
    /// <c>System.IO.Ports.SerialPort</c> at 115 200 baud).  The test double is
    /// <see cref="MockSerialCommunicationService"/>, which uses
    /// <c>System.Threading.Channels</c> instead of a physical port.
    /// </remarks>
    public interface ISerialCommunicationService : IDisposable
    {
        /// <summary>
        /// Gets a value indicating whether the service is connected to a serial port or the simulator.
        /// </summary>
        bool IsConnected { get; }

        /// <summary>Raised when a status or diagnostic message is available.</summary>
        event EventHandler<string> MessageReceived;

        /// <summary>Raised when an unrecoverable communication error occurs.</summary>
        event EventHandler<Exception> ErrorOccurred;

        /// <summary>Raised after each packet is sent during an upload sequence.</summary>
        event EventHandler<UploadProgress> UploadProgressChanged;

        /// <summary>Raised after each packet is received during a download sequence.</summary>
        event EventHandler<DownloadProgress> DownloadProgressChanged;

        /// <summary>
        /// Opens the serial port at 115 200 baud (no-op in simulator mode).
        /// </summary>
        /// <param name="portName">COM port name, e.g. <c>"COM3"</c>.</param>
        /// <param name="cancellationToken">Token to cancel the connection attempt.</param>
        /// <returns><c>true</c> if the port was opened successfully.</returns>
        Task<bool> ConnectAsync(string portName, CancellationToken cancellationToken = default);

        /// <summary>Closes the serial port and releases all port resources.</summary>
        void Disconnect();

        /// <summary>
        /// Serialises <paramref name="packet"/> and writes it to the port without
        /// waiting for an acknowledgement.
        /// </summary>
        /// <param name="packet">The packet to send.</param>
        /// <param name="cancellationToken">Token to cancel the write.</param>
        Task SendPacketAsync(BinaryPacket packet, CancellationToken cancellationToken);

        /// <summary>
        /// Sends a binary packet and waits for an ACK or NACK response
        /// using a <c>TaskCompletionSource</c>-based (non-polling) mechanism.
        /// </summary>
        /// <param name="packet">The packet to send.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        /// <returns><c>true</c> if an ACK was received within the configured timeout.</returns>
        Task<bool> SendPacketWithAckAsync(BinaryPacket packet, CancellationToken cancellationToken);

        /// <summary>
        /// Waits for the next inbound data packet.
        /// In simulator mode, reads from the internal <c>Channel&lt;BinaryPacket&gt;</c>.
        /// </summary>
        /// <param name="cancellationToken">Token to cancel the wait.</param>
        /// <returns>The received packet, or <c>null</c> if the receive timed out.</returns>
        Task<BinaryPacket?> ReceivePacketAsync(CancellationToken cancellationToken);

        /// <summary>Reports current upload progress to all registered listeners.</summary>
        void OnUploadProgress(UploadProgress progress);

        /// <summary>Reports current download progress to all registered listeners.</summary>
        void OnDownloadProgress(DownloadProgress progress);

        /// <summary>
        /// Enables or disables the built-in simulator.  When enabled, subsequent calls
        /// behave as if hardware is connected without requiring a physical COM port.
        /// </summary>
        /// <param name="enabled"><c>true</c> to activate the simulator, <c>false</c> to deactivate.</param>
        void EnableSimulator(bool enabled);
    }
}

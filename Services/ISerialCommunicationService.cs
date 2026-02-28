using System;
using System.Threading;
using System.Threading.Tasks;
using BlackBoxControl.Protocol;

namespace BlackBoxControl.Services
{
    public interface ISerialCommunicationService : IDisposable
    {
        bool IsConnected { get; }

        event EventHandler<string> MessageReceived;
        event EventHandler<Exception> ErrorOccurred;
        event EventHandler<UploadProgress> UploadProgressChanged;
        event EventHandler<DownloadProgress> DownloadProgressChanged;

        Task<bool> ConnectAsync(string portName, CancellationToken cancellationToken = default);
        void Disconnect();
        Task SendPacketAsync(BinaryPacket packet, CancellationToken cancellationToken);
        Task<bool> SendPacketWithAckAsync(BinaryPacket packet, CancellationToken cancellationToken);
        Task<BinaryPacket?> ReceivePacketAsync(CancellationToken cancellationToken);
        void OnUploadProgress(UploadProgress progress);
        void OnDownloadProgress(DownloadProgress progress);
        void EnableSimulator(bool enabled);
    }
}

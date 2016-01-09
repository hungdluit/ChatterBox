using System.Diagnostics;
using System.Threading;
using Windows.Networking.Sockets;
using ChatterBox.Client.Common.Signaling;

namespace ChatterBox.Client.Universal.Background
{
    public sealed class SignalingSocketOperation : ISignalingSocketOperation
    {
        private static readonly SemaphoreSlim SemaphoreSlim = new SemaphoreSlim(1, 1);
        private StreamSocket _socket;

        internal SignalingSocketOperation()
        {
            SemaphoreSlim.Wait();
        }

        public static string SignalingSocketId { get; } = nameof(SignalingSocketId);

        public StreamSocket Socket
        {
            get
            {
                if (_socket != null) return _socket;

                SocketActivityInformation socketInformation;
                _socket = SocketActivityInformation.AllSockets.TryGetValue(SignalingSocketId, out socketInformation)
                    ? socketInformation.StreamSocket
                    : null;

                if (_socket == null)
                {
                    Debug.WriteLine("SignalingSocketOperation - Socket was null");
                }

                return _socket;
            }
        }
        public void Disconnect()
        {
            Dispose();
            if (Socket != null)
            {
                _socket.Dispose();
                _socket = null;
            }

        }

        #region IDisposable Members

        public void Dispose()
        {
            _socket?.TransferOwnership(SignalingSocketId);
            SemaphoreSlim.Release();
        }

        #endregion
    }
}
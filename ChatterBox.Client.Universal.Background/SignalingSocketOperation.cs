using System.Diagnostics;
using System.Threading;
using Windows.Networking.Sockets;
using ChatterBox.Client.Common.Signaling;

namespace ChatterBox.Client.Universal.Background
{
    public sealed class SignalingSocketOperation : ISignalingSocketOperation
    {
        private static readonly SemaphoreSlim SemaphoreSlim = new SemaphoreSlim(1, 1);

        internal SignalingSocketOperation()
        {
            SemaphoreSlim.Wait();
            Debug.WriteLine("SignalingSocketOperation - Create");
        }


        private StreamSocket _socket;
        public static string SignalingSocketId { get; } = nameof(SignalingSocketId);


        public void Dispose()
        {
            _socket?.TransferOwnership(SignalingSocketId);
            SemaphoreSlim.Release();
            Debug.WriteLine("SignalingSocketOperation - Dispose");
        }
        

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
    }
}
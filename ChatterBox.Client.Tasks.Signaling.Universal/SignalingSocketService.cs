using System;
using Windows.Networking;
using Windows.Networking.Sockets;
using ChatterBox.Client.Signaling;

namespace ChatterBox.Client.Tasks.Signaling.Universal
{
    public sealed class SignalingSocketService : ISignalingSocketService
    {
        private readonly Guid _signalingTaskId;
        private static string SignalingSocketId = nameof(SignalingSocketId);

        public SignalingSocketService(Guid signalingTaskId)
        {
            _signalingTaskId = signalingTaskId;
        }

        public bool Connect(string hostname, int port)
        {
            try
            {
                var socket = new StreamSocket();
                socket.EnableTransferOwnership(_signalingTaskId, SocketActivityConnectedStandbyAction.Wake);
                socket.ConnectAsync(new HostName(hostname), port.ToString(), SocketProtectionLevel.PlainSocket)
                    .AsTask()
                    .Wait();
                HandoffSocket(socket);
                return true;
            }
            catch
            {
                return false;
            }
        }

        

        public StreamSocket GetSocket()
        {
            SocketActivityInformation socketInformation;
            return SocketActivityInformation.AllSockets.TryGetValue(SignalingSocketId, out socketInformation) ? socketInformation.StreamSocket : null;
        }

        public void HandoffSocket(StreamSocket socket)
        {
            socket.TransferOwnership(SignalingSocketId);
        }
    }
}

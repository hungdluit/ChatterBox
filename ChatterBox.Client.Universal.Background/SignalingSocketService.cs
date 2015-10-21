using System;
using Windows.Networking;
using Windows.Networking.Sockets;
using ChatterBox.Client.Common.Settings;
using ChatterBox.Client.Common.Signaling;

namespace ChatterBox.Client.Universal.Background
{
    public sealed class SignalingSocketService : ISignalingSocketService
    {
        private static readonly string SignalingSocketId = nameof(SignalingSocketId);
        public Guid SignalingTaskId { get; set; }

        public bool Connect()
        {
            try
            {
                var socket = new StreamSocket();
                socket.EnableTransferOwnership(SignalingTaskId, SocketActivityConnectedStandbyAction.Wake);
                socket.ConnectAsync(new HostName(SignalingSettings.SignalingServerHost), 
                    SignalingSettings.SignalingServerPort, SocketProtectionLevel.PlainSocket)
                    .AsTask()
                    .Wait();
                HandoffSocket(socket);
                return true;
            }
            catch (Exception exception)
            {
                return false;
            }
        }

        public StreamSocket GetSocket()
        {
            SocketActivityInformation socketInformation;
            return SocketActivityInformation.AllSockets.TryGetValue(SignalingSocketId, out socketInformation)
                ? socketInformation.StreamSocket
                : null;
        }

        public void HandoffSocket(StreamSocket socket)
        {
            socket.TransferOwnership(SignalingSocketId);
        }
    }
}
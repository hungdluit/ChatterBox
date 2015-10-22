using System;
using Windows.Networking;
using Windows.Networking.Sockets;
using ChatterBox.Client.Common.Communication.Signaling;
using ChatterBox.Client.Common.Communication.Signaling.Dto;
using ChatterBox.Client.Common.Settings;
using ChatterBox.Client.Common.Signaling;
using ChatterBox.Client.Common.Signaling.PersistedData;

namespace ChatterBox.Client.Universal.Background
{
    public sealed class SignalingSocketService : ISignalingSocketService, ISignalingSocketChannel
    {
        private static readonly string SignalingSocketId = nameof(SignalingSocketId);

        public ConnectionStatus ConnectToSignalingServer(ConnectionOwner connectionOwner)
        {
            try
            {
                SignaledPeerData.Reset();
                SignalingStatus.Reset();
                SignaledRelayMessages.Reset();


                var socket = new StreamSocket();
                socket.EnableTransferOwnership(Guid.Parse(connectionOwner.OwnerId),
                    SocketActivityConnectedStandbyAction.Wake);
                socket.ConnectAsync(new HostName(SignalingSettings.SignalingServerHost),
                    SignalingSettings.SignalingServerPort, SocketProtectionLevel.PlainSocket)
                    .AsTask()
                    .Wait();
                HandoffSocket(socket);
                return new ConnectionStatus
                {
                    IsConnected = true
                };
            }
            catch (Exception exception)
            {
                return new ConnectionStatus
                {
                    IsConnected = false
                };
            }
        }

        public ConnectionStatus GetConnectionStatus()
        {
            return new ConnectionStatus
            {
                IsConnected = GetSocket() != null
            };
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
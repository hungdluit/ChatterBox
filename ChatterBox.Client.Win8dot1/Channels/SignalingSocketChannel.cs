using ChatterBox.Client.Common.Communication.Signaling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChatterBox.Client.Common.Communication.Signaling.Dto;
using Windows.Networking.Sockets;
using Windows.Networking;
using ChatterBox.Client.Common.Settings;
using ChatterBox.Client.Common.Signaling;
using System.IO;
using System.Diagnostics;
using ChatterBox.Common.Communication.Helpers;
using Microsoft.Practices.Unity;

namespace ChatterBox.Client.Win8dot1.Channels
{
    internal class SignalingSocketChannel : ISignalingSocketChannel
    {
        private StreamSocket _streamSocket;
        private bool _isConnected;
        private SignalingClient _signalingClient;
        private IUnityContainer _unityContainer;

        public SignalingSocketChannel(IUnityContainer unityContainer)
        {
            _unityContainer = unityContainer;
        }

        public ConnectionStatus ConnectToSignalingServer(ConnectionOwner connectionOwner)
        {            
            _streamSocket = new StreamSocket();
            _streamSocket.ConnectAsync(new HostName(SignalingSettings.SignalingServerHost),
                                       SignalingSettings.SignalingServerPort, SocketProtectionLevel.PlainSocket)
                         .AsTask()
                         .Wait();

            _isConnected = true;

            _signalingClient = _unityContainer.Resolve<SignalingClient>();

            StartReading();

            return new ConnectionStatus
            {
                IsConnected = _isConnected
            };
        }

        private void StartReading()
        {
            Task.Run(async () =>
            {
                try
                {
                    var reader = new StreamReader(_streamSocket.InputStream.AsStreamForRead());
                    while (_isConnected)
                    {
                        var message = await reader.ReadLineAsync();
                        if (message == null) break;
                        _signalingClient.HandleRequest(message);
                    }
                }
                catch (Exception exception)
                {
                    Debug.WriteLine($"Reading error: {exception.Message}");
                    _isConnected = false;
                }
            });
        }

        public ConnectionStatus GetConnectionStatus()
        {
            return new ConnectionStatus
            {
                IsConnected = _isConnected
            };
        }

        public StreamSocket StreamSocket { get { return _streamSocket; } }

        public void Dispose()
        {

        }
    }
}

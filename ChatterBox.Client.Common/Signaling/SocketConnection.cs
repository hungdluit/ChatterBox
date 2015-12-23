using ChatterBox.Client.Common.Communication.Signaling;
using ChatterBox.Client.Common.Settings;
using ChatterBox.Common.Communication.Contracts;
using ChatterBox.Common.Communication.Messages.Registration;
using System;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;

namespace ChatterBox.Client.Common.Signaling
{
    public sealed class SocketConnection : ISocketConnection
    {
        private readonly IClientChannel _clientChannel;
        private readonly ISignalingSocketChannel _signalingSocketChannel;
        private bool _isConnecting;

        private static readonly object _connectingLock = new object();

        public SocketConnection(ISignalingSocketChannel signalingSocketChannel, IClientChannel clientChannel)
        {
            _clientChannel = clientChannel;
            _signalingSocketChannel = signalingSocketChannel;
        }

        public bool IsConnected
        {
            get
            {
                var connectionStatus = _signalingSocketChannel.GetConnectionStatus();
                if (connectionStatus != null)
                    return connectionStatus.IsConnected;
                return false;
            }
        }

        public void Disconnect()
        {
            if (Monitor.TryEnter(_connectingLock))
            {
              if (!IsConnected) {
                  return;
              }

              //Todo: de-register with the current server?
              _signalingSocketChannel.DisconnectSignalingServer();

              Monitor.Exit(_connectingLock);
            }
        }

        public IAsyncOperation<bool> Connect()
        {
            return Task.Run(() =>
            {
                bool ret = false;
                if (Monitor.TryEnter(_connectingLock))
                {
                    try
                    {
                        if (!IsConnected)
                        {
                            _signalingSocketChannel.ConnectToSignalingServer(null);
                            if (IsConnected) Register();
                        }
                    }
                    catch
                    {
                    }
                    finally
                    {
                        Monitor.Exit(_connectingLock);
                    }
                }
                return ret;
            }).AsAsyncOperation();
        }

        public void Register()
        {
            _clientChannel.Register(new Registration
            {
                Name = RegistrationSettings.Name,
                UserId = RegistrationSettings.UserId,
                Domain = RegistrationSettings.Domain,
                PushNotificationChannelURI = RegistrationSettings.PushNotificationChannelURI
            });
        }
    }
}
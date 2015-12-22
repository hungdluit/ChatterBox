using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using ChatterBox.Client.Common.Communication.Signaling;
using ChatterBox.Common.Communication.Contracts;
using ChatterBox.Client.Common.Signaling.PersistedData;
using ChatterBox.Common.Communication.Messages.Registration;
using ChatterBox.Client.Common.Settings;

namespace ChatterBox.Client.Common.Signaling
{
    public sealed class SocketConnection : ISocketConnection
    {
        private readonly IClientChannel _clientChannel;
        private readonly ISignalingSocketChannel _signalingSocketChannel;
        private bool _connectingFailed;
        private bool _isConnecting;

        private static readonly object _connectingLock = new object();

        private static readonly object _isConnectingLock = new object();
        private static readonly object _connectionFailedLock = new object();

        public event EventHandler<object> OnConnectingStarted;
        public event EventHandler<object> OnConnectingFinished;
        public event EventHandler<object> OnConnectionTerminated;
        public event EventHandler<object> OnRegistering;
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

        public bool IsConnecting
        {
            get
            {
                lock(_isConnectingLock)
                {
                    return _isConnecting;
                }
            }

            private set
            {
                lock (_isConnectingLock)
                {
                    _isConnecting = value;
                }
            }
        }

        public bool IsConnectingFailed
        {
            get
            {
                lock (_connectionFailedLock)
                {
                    return _connectingFailed;
                }
            }

            private set
            {
                lock (_connectionFailedLock)
                {
                    _connectingFailed = value;
                }
            }
        }

        public void Disconnect()
        {
            if (Monitor.TryEnter(_connectingLock))
            {
              if (IsConnected) {
                //Todo: de-register with the current server?
              }
              _signalingSocketChannel.DisconnectSignalingServer();

              IsConnectingFailed = false;
              IsConnecting = false;

              Monitor.Exit(_connectingLock);
              OnConnectionTerminated?.Invoke(this, EventArgs.Empty);


            }
        }

        public bool Connect()
        {
            bool ret = false;
            if (Monitor.TryEnter(_connectingLock))
            {
                try
                {
                    IsConnectingFailed = false;
                    IsConnecting = true;
                    OnConnectingStarted?.Invoke(this, EventArgs.Empty);

                    if (!IsConnected)
                    {
                        _signalingSocketChannel.ConnectToSignalingServer(null);
                        if (IsConnected)
                            Register();
                        else
                            IsConnectingFailed = true;
                    }
                }
                catch
                {
                    IsConnectingFailed = true;
                }
                finally
                {
                    ret = !IsConnectingFailed;
                    IsConnecting = false;
                    Monitor.Exit(_connectingLock);
                    OnConnectingFinished?.Invoke(this, EventArgs.Empty);
                }
            }
            return ret;
        }

        public void Register()
        {
            OnRegistering?.Invoke(this, EventArgs.Empty);

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
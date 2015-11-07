using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        private bool _connectionFailed;
        private bool _isConnecting;

        public event EventHandler<object> OnConnectionFailed;
        public event EventHandler<object> OnConnecting;
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
                bool ret = false;
                if (_signalingSocketChannel != null)
                {
                    var connectionStatus = _signalingSocketChannel.GetConnectionStatus();
                    if (connectionStatus != null)
                        ret = connectionStatus.IsConnected;
                }
                return ret;
            }
        }

        public bool IsConnecting
        {
            get
            {
                return _isConnecting;
            }

            set
            {
                if (_isConnecting != value)
                {
                    _isConnecting = value;
                    OnConnecting?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        public bool IsConnectionFailed
        {
            get
            {
                return _connectionFailed;
            }

            set
            {
                if (_connectionFailed != value)
                {
                    _connectionFailed = value;
                    OnConnectionFailed?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        public void Disconnect()
        {
            throw new NotImplementedException();
        }

        public void Connect()
        {
            IsConnectionFailed = false;
            IsConnecting = true;
            if (IsConnected)
            {
                IsConnectionFailed = false;
                IsConnecting = false;

            }
            else
            {
                _signalingSocketChannel.ConnectToSignalingServer(null);
                if (IsConnected)
                {
                    Register();
                }
                else
                {
                    IsConnectionFailed = true;
                    IsConnecting = false;
                }
            }
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
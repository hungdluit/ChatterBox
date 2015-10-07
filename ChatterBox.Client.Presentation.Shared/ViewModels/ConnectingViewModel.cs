using System;
using ChatterBox.Client.Presentation.Shared.MVVM;
using ChatterBox.Client.Presentation.Shared.Services;
using ChatterBox.Client.Signaling;

namespace ChatterBox.Client.Presentation.Shared.ViewModels
{
    public class ConnectingViewModel : BindableBase
    {
        private readonly SignalingClient _signalingClient;
        private bool _connectionFailed;
        private bool _isConnecting;
        private string _status;

        public ConnectingViewModel(ISignalingUpdateService signalingUpdateService, SignalingClient signalingClient)
        {
            _signalingClient = signalingClient;
            signalingUpdateService.OnUpdate += SignalingUpdateService_OnUpdate;
            ConnectCommand = new DelegateCommand(OnConnectCommandExecute, OnConnectCommandCanExecute);
        }

        public DelegateCommand ConnectCommand { get; }

        public bool ConnectionFailed
        {
            get { return _connectionFailed; }
            set
            {
                if (SetProperty(ref _connectionFailed, value))
                {
                    ConnectCommand.RaiseCanExecuteChanged();
                }
            }
        }

        public bool IsConnecting
        {
            get { return _isConnecting; }
            set
            {
                if (SetProperty(ref _isConnecting, value))
                {
                    ConnectCommand.RaiseCanExecuteChanged();
                }
            }
        }

        public string Status
        {
            get { return _status; }
            set { SetProperty(ref _status, value); }
        }

        public event Action ConnectionEstablished;

        public void EstablishConnection()
        {
            ConnectionFailed = false;
            IsConnecting = true;

            var isConnected = _signalingClient.CheckConnection();
            if (isConnected)
            {
                ConnectionFailed = false;
                IsConnecting = false;
                ConnectionEstablished?.Invoke();
                return;
            }
            var connected = _signalingClient.Connect();
            if (!connected)
            {
                Status = "Failed to connect to the server. Check you settings and try again.";
                ConnectionFailed = true;
                IsConnecting = false;
                return;
            }

            Status = "Registering with the server...";
            _signalingClient.RegisterUsingSettings();
        }

        private bool OnConnectCommandCanExecute()
        {
            return !IsConnecting && ConnectionFailed;
        }

        private void OnConnectCommandExecute()
        {
            EstablishConnection();
        }

        private void SignalingUpdateService_OnUpdate()
        {
            if (SignalingStatus.IsRegistered) ConnectionEstablished?.Invoke();
        }
    }
}
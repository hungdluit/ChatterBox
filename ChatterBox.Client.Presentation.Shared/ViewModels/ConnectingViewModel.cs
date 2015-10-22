﻿using System;
using ChatterBox.Client.Common.Communication.Signaling;
using ChatterBox.Client.Common.Settings;
using ChatterBox.Client.Common.Signaling.PersistedData;
using ChatterBox.Client.Presentation.Shared.MVVM;
using ChatterBox.Client.Presentation.Shared.Services;
using ChatterBox.Common.Communication.Contracts;
using ChatterBox.Common.Communication.Messages.Registration;

namespace ChatterBox.Client.Presentation.Shared.ViewModels
{
    public class ConnectingViewModel : BindableBase
    {
        private readonly IClientChannel _clientChannel;
        private readonly ISignalingSocketChannel _signalingSocketChannel;
        private bool _connectionFailed;
        private bool _isConnecting;
        private string _status;

        public ConnectingViewModel(ISignalingUpdateService signalingUpdateService,
            ISignalingSocketChannel signalingSocketChannel, IClientChannel clientChannel)
        {
            _clientChannel = clientChannel;
            _signalingSocketChannel = signalingSocketChannel;
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
            var connectionStatus = _signalingSocketChannel.GetConnectionStatus();
            if (connectionStatus.IsConnected)
            {
                ConnectionFailed = false;
                IsConnecting = false;
                ConnectionEstablished?.Invoke();
                return;
            }
            connectionStatus = _signalingSocketChannel.ConnectToSignalingServer(null);
            if (!connectionStatus.IsConnected)
            {
                Status = "Failed to connect to the server. Check you settings and try again.";
                ConnectionFailed = true;
                IsConnecting = false;
                return;
            }

            Status = "Registering with the server...";
            _clientChannel.Register(new Registration
            {
                Name = RegistrationSettings.Name,
                UserId = RegistrationSettings.UserId,
                Domain = RegistrationSettings.Domain,
                PushToken = RegistrationSettings.UserId
            });
            if (connectionStatus.IsConnected)
            {
                ConnectionEstablished?.Invoke();
            }
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
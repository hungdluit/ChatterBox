using System;
using ChatterBox.Client.Common.Signaling;
using ChatterBox.Client.Common.Signaling.PersistedData;
using ChatterBox.Client.Presentation.Shared.MVVM;
using ChatterBox.Client.Presentation.Shared.Services;
using System.Diagnostics;
using Windows.UI.Core;

namespace ChatterBox.Client.Presentation.Shared.ViewModels
{
    public class ConnectingViewModel : BindableBase
    {
        private readonly ISocketConnection _connection;
        private bool _isConnecting;
        private readonly CoreDispatcher _dispatcher;
        private string _status;

        public ConnectingViewModel(IForegroundUpdateService foregroundUpdateService,
                                   ISocketConnection socketConnection)
        {
            _connection = socketConnection;

            foregroundUpdateService.OnRegistrationStatusUpdated += OnRegistrationStatusUpdated;

            ConnectCommand = new DelegateCommand(OnConnectCommandExecute, OnConnectCommandCanExecute);
            ShowSettings = new DelegateCommand(() => OnShowSettings?.Invoke());

            UpdateStatus();
        }

        public DelegateCommand ConnectCommand { get; }

        public DelegateCommand ShowSettings { get; set; }

        public string Status
        {
            get { return _status; }
            set { SetProperty(ref _status, value); }
        }

        public async void EstablishConnection()
        {
            if (_connection.IsConnected)
            {
                if (SignalingStatus.IsRegistered)
                {
                    OnRegistered?.Invoke();
                }
            }
            else
            {
                _isConnecting = true;
                UpdateStatus();

                await _connection.Connect();

                _isConnecting = false;
                UpdateStatus();
            }
        }

        public async void SwitchSignalingServer()
        {
            if (_connection.IsConnected)
            {
                await _connection.Disconnect();

                EstablishConnection();

            }
        }

        private bool OnConnectCommandCanExecute()
        {
            var ret = !_connection.IsConnected && !_isConnecting;
            return ret;
        }

        private void OnConnectCommandExecute()
        {
            EstablishConnection();
        }

        private void OnRegistrationStatusUpdated()
        {
            if (SignalingStatus.IsRegistered)
            {
                OnRegistered?.Invoke();
            }
            else
            {
                OnRegistrationFailed?.Invoke();
                Status = "Connection to the server has been terminated.";
                ConnectCommand.RaiseCanExecuteChanged();
            }
        }

        private void UpdateStatus()
        {
            if (!_connection.IsConnected && !_isConnecting)
            {
                Status = "Failed to connect to the server. Check your settings and try again.";
            }
            else if (!_isConnecting)
            {
                Status = "Registering with the server...";
            }

            ConnectCommand.RaiseCanExecuteChanged();
        }

        public event Action OnRegistered;
        public event Action OnRegistrationFailed;
        public event Action OnShowSettings;
    }
}
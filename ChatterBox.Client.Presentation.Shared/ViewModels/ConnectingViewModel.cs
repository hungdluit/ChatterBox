using System;
using ChatterBox.Client.Common.Communication.Signaling;
using ChatterBox.Client.Common.Settings;
using ChatterBox.Client.Common.Signaling;
using ChatterBox.Client.Common.Signaling.PersistedData;
using ChatterBox.Client.Presentation.Shared.MVVM;
using ChatterBox.Client.Presentation.Shared.Services;
using ChatterBox.Common.Communication.Contracts;
using ChatterBox.Common.Communication.Messages.Registration;


namespace ChatterBox.Client.Presentation.Shared.ViewModels
{
    public class ConnectingViewModel : BindableBase
    {
        private readonly ISocketConnection _connection;
        private string _status;

        public ConnectingViewModel(IForegroundUpdateService foregroundUpdateService, ISocketConnection socketConnection)
        {
            _connection = socketConnection;
            _connection.OnConnectingStarted += OnConnectingStarted;
            _connection.OnConnectingFinished += OnConnectingFinished;
            _connection.OnConnectionTerminated += OnConnectionTerminated;
            _connection.OnRegistering += OnRegistering;

            foregroundUpdateService.OnRegistrationStatusUpdated += OnRegistrationStatusUpdated;

            ConnectCommand = new DelegateCommand(OnConnectCommandExecute, OnConnectCommandCanExecute);
            ShowSettings = new DelegateCommand(() => OnShowSettings?.Invoke());
        }

        public DelegateCommand ConnectCommand { get; }

        public DelegateCommand ShowSettings { get; set; }

        public string Status
        {
            get { return _status; }
            set { SetProperty(ref _status, value); }
        }

        public void EstablishConnection()
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
                _connection.Connect();
            }
        }

        public void SwitchSignalingServer()
        {
            if (_connection.IsConnected)
            {
                _connection.Disconnect();
                //Todo: switch to new signaling server right away without user intervention?
            }
        }

        private bool OnConnectCommandCanExecute()
        {
            return !_connection.IsConnected ||
                  (!_connection.IsConnecting && _connection.IsConnectingFailed);
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

        private void OnConnectingStarted(object sender, object e)
        {
            ConnectCommand.RaiseCanExecuteChanged();
        }

        private void OnRegistering(object sender, object e)
        {
            Status = "Registering with the server...";
        }

        private void OnConnectingFinished(object sender, object e)
        {
            if (_connection.IsConnectingFailed)
                Status = "Failed to connect to the server. Check your settings and try again.";

            ConnectCommand.RaiseCanExecuteChanged();
        }
        private void OnConnectionTerminated(object sender, object e)
        {
            Status = "Connection to server is disconnected. Please click reconnect";
            ConnectCommand.RaiseCanExecuteChanged();
        }

        public event Action OnRegistered;
        public event Action OnRegistrationFailed;
        public event Action OnShowSettings;
    }
}
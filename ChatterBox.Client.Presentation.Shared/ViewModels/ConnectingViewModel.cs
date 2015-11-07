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

        public ConnectingViewModel(ISignalingUpdateService signalingUpdateService, ISocketConnection socketConnection)
        {
            _connection = socketConnection;
            _connection.OnConnecting += OnConnecting;
            _connection.OnConnectionFailed += OnConnectionFailed;
            _connection.OnRegistering += OnRegistering;

            signalingUpdateService.OnRegistrationStatusUpdated += OnRegistrationStatusUpdated;
            ConnectCommand = new DelegateCommand(OnConnectCommandExecute, OnConnectCommandCanExecute);
        }

        public DelegateCommand ConnectCommand { get; }

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

        private bool OnConnectCommandCanExecute()
        {
            return !_connection.IsConnecting && _connection.IsConnectionFailed;
        }

        private void OnConnectCommandExecute()
        {
            EstablishConnection();
        }

        public event Action OnRegistered;

        private void OnRegistrationStatusUpdated()
        {
            if (SignalingStatus.IsRegistered) OnRegistered?.Invoke();
        }

        private void OnConnecting(object sender, object e)
        {
            ConnectCommand.RaiseCanExecuteChanged();
        }

        private void OnRegistering(object sender, object e)
        {
            Status = "Registering with the server...";
        }

        private void OnConnectionFailed(object sender, object e)
        {
            Status = "Failed to connect to the server. Check you settings and try again.";
            ConnectCommand.RaiseCanExecuteChanged();
        }
    }
}
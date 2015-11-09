using System;
using ChatterBox.Client.Common.Settings;
using ChatterBox.Client.Presentation.Shared.MVVM;

namespace ChatterBox.Client.Presentation.Shared.ViewModels
{
    public class SettingsViewModel : BindableBase
    {
        private string _domain;
        private string _signalingServerHost;
        private int _signalingServerPort;

        public SettingsViewModel()
        {
            CloseCommand = new DelegateCommand(OnCloseCommandExecute);
            SaveCommand = new DelegateCommand(OnSaveCommandExecute);
            Reset();
        }

        public DelegateCommand CloseCommand { get; set; }

        public string Domain
        {
            get { return _domain; }
            set { SetProperty(ref _domain, value); }
        }

        public DelegateCommand SaveCommand { get; set; }

        public string SignalingServerHost
        {
            get { return _signalingServerHost; }
            set { SetProperty(ref _signalingServerHost, value); }
        }

        public int SignalingServerPort
        {
            get { return _signalingServerPort; }
            set { SetProperty(ref _signalingServerPort, value); }
        }

        public event Action OnClose;

        private void OnCloseCommandExecute()
        {
            Reset();
            OnClose?.Invoke();
        }

        private void OnSaveCommandExecute()
        {
            SignalingSettings.SignalingServerPort = SignalingServerPort.ToString();
            SignalingSettings.SignalingServerHost = SignalingServerHost;
            RegistrationSettings.Domain = Domain;
            OnCloseCommandExecute();
        }

        private void Reset()
        {
            SignalingServerPort = int.Parse(SignalingSettings.SignalingServerPort);
            SignalingServerHost = SignalingSettings.SignalingServerHost;
            Domain = RegistrationSettings.Domain;
        }
    }
}
using Windows.UI.Core;
using ChatterBox.Client.Presentation.Shared.MVVM;
using ChatterBox.Client.Signaling;
using ChatterBox.Common.Communication.Messages.Standard;

namespace ChatterBox.Client.Presentation.Shared.ViewModels
{
    public sealed class MainViewModel : DispatcherBindableBase
    {
        private readonly SignalingClient _signalingClient;
        private bool _isActive;

        public MainViewModel(
            WelcomeViewModel welcomeViewModel,
            ConnectingViewModel connectingViewModel,
            ContactsViewModel contactsViewModel,
            SettingsViewModel settingsViewModel,
            SignalingClient signalingClient,
            CoreDispatcher uiDispatcher) : base(uiDispatcher)
        {
            _signalingClient = signalingClient;
            WelcomeViewModel = welcomeViewModel;
            ConnectingViewModel = connectingViewModel;
            ContactsViewModel = contactsViewModel;
            SettingsViewModel = settingsViewModel;

            WelcomeViewModel.OnCompleted += WelcomeCompleted;
            ConnectingViewModel.ConnectionEstablished += ConnectingViewModel_ConnectionEstablished;
        }


        public ConnectingViewModel ConnectingViewModel { get; }
        public ContactsViewModel ContactsViewModel { get; }

        public bool IsActive
        {
            get { return _isActive; }
            set { SetProperty(ref _isActive, value); }
        }

        public SettingsViewModel SettingsViewModel { get; }
        public WelcomeViewModel WelcomeViewModel { get; }

        private void ConnectingViewModel_ConnectionEstablished()
        {
            IsActive = true;
        }

        public void OnNavigatedTo()
        {
            if (WelcomeViewModel.IsCompleted) WelcomeCompleted();
            if (SignalingStatus.IsRegistered)
            {
                _signalingClient.GetPeerList(new Message());
            }
        }

        private void WelcomeCompleted()
        {
            ConnectingViewModel.EstablishConnection();
        }
    }
}
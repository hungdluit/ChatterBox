using Windows.UI.Core;
using ChatterBox.Client.Presentation.Shared.MVVM;
using ChatterBox.Client.Presentation.Shared.MVVM.Utils;
using ChatterBox.Client.Presentation.Shared.Models;
using ChatterBox.Client.Settings;

namespace ChatterBox.Client.Presentation.Shared.ViewModels
{
    public abstract class MainViewModelBase : DispatcherBindableBase
    {
        private bool _isWelcomeOpen;
        private bool _isChatOpen;
        private bool _isCallOpen;
        private bool _isContactsOpen;
        private bool _isSettingsOpen;
        private ScreenUtils.AppState _appState;
        protected ScreenUtils ScreenUtils;

        protected MainViewModelBase(CoreDispatcher uiDispatcher) : base(uiDispatcher)
        {
            ScreenUtils = new ScreenUtils();
            ScreenUtils.StateChanged += AppStateChanged;
            AppState = ScreenUtils.CurrentState;
            WelcomeViewModel.Domain = RegistrationSettings.Domain;

            WelcomeViewModel.OnCompleted += WelcomeCompleted;

            ContactsViewModel.ContactSelected += ContactSelected;
            ContactsViewModel.SettingsSelected += SettingsSelected;

            ChatViewModel.OnCompleted += ChatClosed;
            ChatViewModel.OnCall += OnCall;
            CallViewModel.OnCompleted += CallClosed;
            SettingsViewModel.OnCompleted += SettingsClose;
        }

        public WelcomeViewModel WelcomeViewModel { get; } = new WelcomeViewModel();

        public ContactsViewModel ContactsViewModel { get; } = new ContactsViewModel();

        public CallViewModel CallViewModel { get; } = new CallViewModel();

        public ChatViewModel ChatViewModel { get; } = new ChatViewModel();

        public SettingsViewModel SettingsViewModel { get; } = new SettingsViewModel();

        public bool IsWelcomeOpen
        {
            get { return _isWelcomeOpen; }
            set { SetProperty(ref _isWelcomeOpen, value); }
        }

        public bool IsContactsOpen
        {
            get { return _isContactsOpen; }
            set { SetProperty(ref _isContactsOpen, value); }
        }

        public bool IsSettingsOpen
        {
            get { return _isSettingsOpen; }
            set { SetProperty(ref _isSettingsOpen, value); }
        }

        public bool IsChatOpen
        {
            get { return _isChatOpen; }
            set { SetProperty(ref _isChatOpen, value); }
        }

        public bool IsCallOpen
        {
            get { return _isCallOpen; }
            set { SetProperty(ref _isCallOpen, value); }
        }

        public ScreenUtils.AppState AppState
        {
            get { return _appState; }
            set { SetProperty(ref _appState, value); }
        }

        private void AppStateChanged(ScreenUtils.AppState newAppState)
        {
            this.AppState = newAppState;
            if (newAppState == ScreenUtils.AppState.ParallelState)
            {
                IsContactsOpen = true;
                if(ContactsViewModel.SelectedContactModel != null && !CallViewModel.IsInCall)
                {
                    ChatViewModel.OnNavigatedTo(ContactsViewModel.SelectedContactModel);
                    IsChatOpen = true;
                }
            }
            if (newAppState == ScreenUtils.AppState.OverlayState && 
                (IsChatOpen || IsSettingsOpen || IsCallOpen))
            {
                IsContactsOpen = false;
            }
        }

        private void WelcomeCompleted()
        {
            OnRegistrationCompleted();
            IsWelcomeOpen = false;
            IsContactsOpen = true;
        }

        private void ContactSelected(ContactModel contact)
        {
            if (ScreenUtils.CurrentState == ScreenUtils.AppState.OverlayState)
            {
                IsContactsOpen = false;
            }
            var isContactInCall = CallViewModel.Contact?.Name.Equals(contact.Name);
            if (CallViewModel.IsInCall && isContactInCall.HasValue && isContactInCall.Value)
            {
                IsChatOpen = false;
                IsCallOpen = true;
            }
            else
            {
                IsChatOpen = true;
                IsCallOpen = false;
            }
            IsSettingsOpen = false;
            ChatViewModel.OnNavigatedTo(contact);
        }

        private void SettingsSelected()
        {
            IsSettingsOpen = true;
            IsChatOpen = false;
            IsCallOpen = false;
            if (ScreenUtils.CurrentState == ScreenUtils.AppState.OverlayState)
            {
                IsContactsOpen = false;
            }
        }

        private void SettingsClose()
        {
            IsSettingsOpen = false;
            IsContactsOpen = true;
        }

        private void ChatClosed()
        {
            IsContactsOpen = true;
            IsChatOpen = false;
        }

        private void OnCall(bool onlyAudio)
        {
            IsChatOpen = false;
            IsCallOpen = true;
            CallViewModel.OnNvigatedTo(ChatViewModel.Contact, onlyAudio);
        }

        private void CallClosed()
        {
            IsCallOpen = false;
            IsChatOpen = true;
        }

        public virtual void OnNavigatedTo()
        {
            IsWelcomeOpen = (string.IsNullOrWhiteSpace(RegistrationSettings.Name) ||
                             string.IsNullOrWhiteSpace(RegistrationSettings.Domain));

            if (!IsWelcomeOpen) WelcomeCompleted();
        }

        public abstract void OnRegistrationCompleted();
    }
}

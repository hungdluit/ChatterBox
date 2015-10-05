using Windows.UI.Core;
using ChatterBox.Client.Presentation.Shared.MVVM;
using ChatterBox.Client.Presentation.Shared.MVVM.Utils;
using ChatterBox.Client.Presentation.Shared.Models;
using ChatterBox.Client.Settings;

namespace ChatterBox.Client.Presentation.Shared.ViewModels
{
    public enum ViewState
    {
        Contacts,
        Settings,
        Chat,
        Call,
        Empty
    }

    public abstract class MainViewModelBase : DispatcherBindableBase
    {
        private bool _isWelcomeOpen;
        private bool _isChatOpen;
        private bool _isCallOpen;
        private bool _isContactsOpen;
        private bool _isSettingsOpen;
        private ViewState _viewState;
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
            var currentState = GetViewState();
            SetViewState(currentState);
        }

        private void WelcomeCompleted()
        {
            OnRegistrationCompleted();
            SetViewState(ViewState.Contacts);
        }

        private void ContactSelected(ContactModel contact)
        {
            if (ScreenUtils.CurrentState == ScreenUtils.AppState.OverlayState)
            {
                SetViewState(ViewState.Contacts);
            }
            var isContactInCall = CallViewModel.Contact?.Name.Equals(contact.Name);
            if (CallViewModel.IsInCall && isContactInCall.HasValue && isContactInCall.Value)
            {
                SetViewState(ViewState.Call);
            }
            else
            {
                SetViewState(ViewState.Chat);
            }
            IsSettingsOpen = false;
            ChatViewModel.OnNavigatedTo(contact);
        }

        private void SettingsSelected()
        {
            _viewState = GetViewState();
            SetViewState(ViewState.Settings);
        }

        private void SettingsClose()
        {
            if (_viewState == ViewState.Settings)
            {
                SetViewState(ViewState.Empty);
            }
            else
            {
                SetViewState(_viewState);
            }
        }

        private void ChatClosed()
        {
            SetViewState(ViewState.Contacts);
        }

        private void OnCall(bool onlyAudio)
        {
            SetViewState(ViewState.Call);
            CallViewModel.OnNvigatedTo(ChatViewModel.Contact, onlyAudio);
        }

        private void CallClosed()
        {
            SetViewState(ViewState.Chat);
        }

        private ViewState GetViewState()
        {
            if (IsSettingsOpen) return ViewState.Settings;

            if (IsCallOpen) return ViewState.Call;

            if (IsChatOpen) return ViewState.Chat;

            if (IsContactsOpen && AppState == ScreenUtils.AppState.OverlayState) return ViewState.Contacts;

            return ViewState.Empty;
        }

        private void SetViewState(ViewState state)
        {
            IsWelcomeOpen = false;
            IsSettingsOpen = false;
            IsChatOpen = false;
            IsCallOpen = false;
            if (AppState == ScreenUtils.AppState.OverlayState)
            {
                IsContactsOpen = false;
            }
            else
            {
                IsContactsOpen = true;
            }

            switch (state)
            {
                case ViewState.Contacts:
                    IsContactsOpen = true;
                    break;
                case ViewState.Settings:
                    IsSettingsOpen = true;
                    break;
                case ViewState.Chat:
                    IsChatOpen = true;
                    break;
                case ViewState.Call:
                    IsCallOpen = true;
                    break;
                case ViewState.Empty:
                    break;
                default:
                    break;
            }
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

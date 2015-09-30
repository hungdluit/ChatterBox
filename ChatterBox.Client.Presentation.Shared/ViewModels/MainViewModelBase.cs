using Windows.UI.Core;
using ChatterBox.Client.Presentation.Shared.MVVM;

namespace ChatterBox.Client.Presentation.Shared.ViewModels
{
    public abstract class MainViewModelBase : DispatcherBindableBase
    {
        private bool _isWelcomeOpen;
        private bool _isChatOpen;
        private bool _isCallOpen;
        private bool _isContactsOpen;

        public WelcomeViewModel WelcomeViewModel {get; } = new WelcomeViewModel();

        public ContactsViewModel ContactsViewModel { get; } = new ContactsViewModel();

        public CallViewModel CallViewModel { get; } = new CallViewModel();

        public ChatViewModel ChatViewModel { get; } = new ChatViewModel();

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

        protected MainViewModelBase(CoreDispatcher uiDispatcher) : base(uiDispatcher)
        {
        }

        public abstract void OnNavigatedTo();
    }
}

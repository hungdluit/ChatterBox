using Windows.UI.Core;
using ChatterBox.Client.Presentation.Shared.MVVM;

namespace ChatterBox.Client.Presentation.Shared.ViewModels
{
    public abstract class MainViewModelBase : DispatcherBindableBase
    {
        private bool _isWelcomeOpened;
        private bool _isChatOpened;
        private bool _isCallOpened;
        private bool _isContactsOpened;

        public WelcomeViewModel WelcomeViewModel {get; } = new WelcomeViewModel();

        public ContactsViewModel ContactsViewModel { get; } = new ContactsViewModel();

        public CallViewModel CallViewModel { get; } = new CallViewModel();

        public ChatViewModel ChatViewModel { get; } = new ChatViewModel();

        public bool IsWelcomeOpened
        {
            get { return _isWelcomeOpened; }
            set { SetProperty(ref _isWelcomeOpened, value); }
        }

        public bool IsContactsOpened
        {
            get { return _isContactsOpened; }
            set { SetProperty(ref _isContactsOpened, value); }
        }

        public bool IsChatOpened
        {
            get { return _isChatOpened; }
            set { SetProperty(ref _isChatOpened, value); }
        }

        public bool IsCallOpened
        {
            get { return _isCallOpened; }
            set { SetProperty(ref _isCallOpened, value); }
        }

        protected MainViewModelBase(CoreDispatcher uiDispatcher) : base(uiDispatcher)
        {
        }

        public abstract void OnNavigatedTo();
    }
}

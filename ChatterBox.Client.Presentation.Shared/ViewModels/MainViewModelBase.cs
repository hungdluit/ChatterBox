using Windows.UI.Core;
using ChatterBox.Client.Presentation.Shared.MVVM;

namespace ChatterBox.Client.Presentation.Shared.ViewModels
{
    public abstract class MainViewModelBase : DispatcherBindableBase
    {
        private bool _isSetupCompleted;

        public WelcomeViewModel WelcomeViewModel {get; } = new WelcomeViewModel();
        public ContactsViewModel ContactsViewModel { get; } = new ContactsViewModel();

        public bool IsSetupCompleted
        {
            get { return _isSetupCompleted; }
            set { SetProperty(ref _isSetupCompleted, value); }
        }

        protected MainViewModelBase(CoreDispatcher uiDispatcher) : base(uiDispatcher)
        {
        }

        public abstract void OnNavigatedTo();
    }
}

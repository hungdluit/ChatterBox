using Windows.UI.Core;
using ChatterBox.Client.Presentation.Shared.MVVM;

namespace ChatterBox.Client.Presentation.Shared.ViewModels
{
    public abstract class MainViewModelBase : DispatcherBindableBase
    {
        private bool _isRegistrationCompleted;
        private RegistrationViewModel _registrationViewModel;

        public RegistrationViewModel RegistrationViewModel
        {
            get { return _registrationViewModel; }
            set { SetProperty(ref _registrationViewModel, value); }
        }

        public bool IsRegistrationCompleted
        {
            get { return _isRegistrationCompleted; }
            set { SetProperty(ref _isRegistrationCompleted, value); }
        }

        protected MainViewModelBase(CoreDispatcher uiDispatcher) : base(uiDispatcher)
        {
        }

        public abstract void OnNavigatedTo();


    }
}

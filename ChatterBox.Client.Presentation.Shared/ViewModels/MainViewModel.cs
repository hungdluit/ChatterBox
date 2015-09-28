using Windows.UI.Core;
using ChatterBox.Client.Presentation.Shared.MVVM;
using ChatterBox.Client.Settings;

namespace ChatterBox.Client.Presentation.Shared.ViewModels
{
    public class MainViewModel : DispatcherBindableBase
    {
        private bool _isRegistrationCompleted;
        public RegistrationViewModel RegistrationViewModel { get; }

        public bool IsRegistrationCompleted
        {
            get { return _isRegistrationCompleted; }
            set { SetProperty(ref _isRegistrationCompleted, value); }
        }

        public MainViewModel(CoreDispatcher uiDispatcher) : base(uiDispatcher)
        {
            RegistrationViewModel = new RegistrationViewModel(uiDispatcher);
        }

        public void OnNavigatedTo()
        {
            IsRegistrationCompleted = (RegistrationSettings.Name == null);
            if(!IsRegistrationCompleted) return;
        }
    }
}

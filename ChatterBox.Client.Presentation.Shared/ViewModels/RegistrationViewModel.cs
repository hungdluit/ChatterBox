using Windows.UI.Core;
using ChatterBox.Client.Presentation.Shared.MVVM;
using ChatterBox.Client.Settings;
using ChatterBox.Client.Signaling;

namespace ChatterBox.Client.Presentation.Shared.ViewModels
{
    public class RegistrationViewModel : DispatcherBindableBase
    {
        private readonly SignalingClient _signalingClient;
        private string _name;
        public DelegateCommand CompleteRegistrationCommand { get; }



        public RegistrationViewModel(CoreDispatcher uiDispatcher, SignalingClient signalingClient ) : base(uiDispatcher)
        {
            _signalingClient = signalingClient;
            CompleteRegistrationCommand = new DelegateCommand(OnCompleteRegistrationCommandExecute, CanCompleteRegistrationCommandExecute);
        }


        public string Name
        {
            get { return _name; }
            set
            {
                if (SetProperty(ref _name, value))
                {
                    CompleteRegistrationCommand.RaiseCanExecuteChanged();
                }
            }
        }



        private bool CanCompleteRegistrationCommandExecute()
        {
            return !string.IsNullOrWhiteSpace(Name);
        }

        private async void OnCompleteRegistrationCommandExecute()
        {
            RegistrationSettings.Name = Name;
            _signalingClient.RegisterUsingSettings();
        }
    }
}

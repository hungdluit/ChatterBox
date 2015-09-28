using Windows.UI.Core;
using ChatterBox.Client.Presentation.Shared.MVVM;

namespace ChatterBox.Client.Presentation.Shared.ViewModels
{
    public class RegistrationViewModel : DispatcherBindableBase
    {
        private string _name;
        public DelegateCommand CompleteRegistrationCommand { get; }



        public RegistrationViewModel(CoreDispatcher uiDispatcher) : base(uiDispatcher)
        {
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
        }





        public delegate void RegistrationEventHandler();
    }
}

using Windows.UI.Core;
using ChatterBox.Client.Presentation.Shared.MVVM;
using ChatterBox.Client.Settings;
using ChatterBox.Client.Signaling;

namespace ChatterBox.Client.Presentation.Shared.ViewModels
{
    public class WelcomeViewModel : BindableBase
    {
        private string _name;
        private string _domain;
        public DelegateCommand CompleteCommand { get; }



        public WelcomeViewModel()
        {
            CompleteCommand = new DelegateCommand(OnCompleteCommandExecute, CanCompleteCommandExecute);
        }


        public string Name
        {
            get { return _name; }
            set
            {
                if (SetProperty(ref _name, value))
                {
                    CompleteCommand.RaiseCanExecuteChanged();
                }
            }
        }


        public string Domain
        {
            get { return _domain; }
            set
            {
                if (SetProperty(ref _domain, value))
                {
                    CompleteCommand.RaiseCanExecuteChanged();
                }
            }
        }



        private bool CanCompleteCommandExecute()
        {
            return !string.IsNullOrWhiteSpace(Name);
        }

        private void OnCompleteCommandExecute()
        {
            RegistrationSettings.Name = Name;
            RegistrationSettings.Domain = Domain;

            OnCompleted?.Invoke();

        }
        public event WelcomeEventHandler OnCompleted;
        public delegate void WelcomeEventHandler();
    }
}

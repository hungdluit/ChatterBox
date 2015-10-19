using ChatterBox.Client.Presentation.Shared.MVVM;
using ChatterBox.Client.Settings;
using System;

namespace ChatterBox.Client.Presentation.Shared.ViewModels
{
    public class SettingsViewModel : BindableBase
    {
        private DelegateCommand _closeCommand;

        public SettingsViewModel()
        {
            CloseCommand = new DelegateCommand(CloseExecute);
        }

        public string RegisteredName
        {
            get { return RegistrationSettings.Name; }
        }

        public string RegisteredDomain
        {
            get { return RegistrationSettings.Domain; }
        }

        public DelegateCommand CloseCommand
        {
            get { return _closeCommand; }
            set { SetProperty(ref _closeCommand, value); }
        }

        private void CloseExecute()
        {
            Close?.Invoke();
        }

        public event Action Close;
    }
}
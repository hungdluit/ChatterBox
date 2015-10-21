using System;
using ChatterBox.Client.Common.Settings;
using ChatterBox.Client.Presentation.Shared.MVVM;

namespace ChatterBox.Client.Presentation.Shared.ViewModels
{
    public class SettingsViewModel : BindableBase
    {
        private DelegateCommand _closeCommand;

        public SettingsViewModel()
        {
            CloseCommand = new DelegateCommand(CloseExecute);
        }

        public DelegateCommand CloseCommand
        {
            get { return _closeCommand; }
            set { SetProperty(ref _closeCommand, value); }
        }

        public string RegisteredDomain
        {
            get { return RegistrationSettings.Domain; }
        }

        public string RegisteredName
        {
            get { return RegistrationSettings.Name; }
        }

        public event Action Close;

        private void CloseExecute()
        {
            Close?.Invoke();
        }
    }
}
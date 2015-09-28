using ChatterBox.Client.Common.Mvvm.Base;
using ChatterBox.Client.Common.Mvvm.Utils;
using System.Windows.Input;
using Windows.UI.Core;

namespace ChatterBox.Client.Common.Mvvm.ViewModels
{
    internal class SettingsViewModel : DispatcherBindableBase
    {
        public SettingsViewModel(CoreDispatcher uiDispatcher, NavigationService navigationService) :
            base(uiDispatcher, navigationService)
        {
            BackButtonCommand = new ActionCommand(BackButtonExecute);
        }

        private ICommand _backButtonCommand;
        public ICommand BackButtonCommand
        {
            get { return _backButtonCommand; }
            set { SetProperty(ref _backButtonCommand, value); }
        }

        private void BackButtonExecute(object obj)
        {
            NavigationService.NavigateBack();
        }
    }
}

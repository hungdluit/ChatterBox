using ChatterBox.Client.Common.Mvvm.Base;
using ChatterBox.Client.Common.Mvvm.Utils;
using ChatterBox.Client.Common.Views;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Windows.UI.Core;

namespace ChatterBox.Client.Common.Mvvm.ViewModels
{
    internal class FirstPanelViewModel : DispatcherBindableBase
    {
        public FirstPanelViewModel(CoreDispatcher uiDispatcher, NavigationService frame) :
            base(uiDispatcher, frame)
        {
            SettingsButtonCommand = new ActionCommand(SettingsButtonExecute, SettingsButtonCanExecute);
            CallButtonCommand = new ActionCommand(CallButtonExecute, CallButtonCanExecute);

            Peers = new ObservableCollection<PeerViewModel>
            {
                new PeerViewModel {DisplayName = "Antony Burgess" },
                new PeerViewModel {DisplayName = "John Steinbeck" },
                new PeerViewModel {DisplayName = "Thomas Wolfe" },
                new PeerViewModel {DisplayName = "Ernesto Sabbato" }
            };
        }

        private ObservableCollection<PeerViewModel> _peers;
        public ObservableCollection<PeerViewModel> Peers
        {
            get { return _peers; }
            set { SetProperty(ref _peers, value); }
        }

        #region Commands

        private ICommand _settingsButtonCommand;
        public ICommand SettingsButtonCommand
        {
            get { return _settingsButtonCommand; }
            set { SetProperty(ref _settingsButtonCommand, value); }
        }
        private void SettingsButtonExecute(object obj)
        {
            NavigationService.NavigateTo(typeof(SettingsView));
        }
        private bool SettingsButtonCanExecute(object obj)
        {
            return true;
        }

        private ICommand _callButtonCommand;
        public ICommand CallButtonCommand
        {
            get { return _callButtonCommand; }
            set { _callButtonCommand = value; }
        }

        private void CallButtonExecute(object obj)
        {
            NavigationService.NavigateTo(typeof(CallView));
        }

        private bool CallButtonCanExecute(object obj)
        {
            return true;
        }

        #endregion
    }
}

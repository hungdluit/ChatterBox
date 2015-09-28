using ChatterBox.Client.Common.Mvvm.Base;
using ChatterBox.Client.Common.Mvvm.Utils;
using System.Windows.Input;
using Windows.UI.Core;

namespace ChatterBox.Client.Common.Mvvm.ViewModels
{
    internal class CallViewModel : DispatcherBindableBase
    {        
        public CallViewModel(CoreDispatcher uiDispatcher, NavigationService frame) :
            base(uiDispatcher, frame)
        {
            CloseCallButtonCommand = new ActionCommand(CloseCallButtonExecute);
        }

        private ICommand _closeCallButtonCommand;
        public ICommand CloseCallButtonCommand
        {
            get { return _closeCallButtonCommand; }
            set { SetProperty(ref _closeCallButtonCommand, value); }
        }

        private void CloseCallButtonExecute(object obj)
        {
            NavigationService.NavigateBack();
        }
    }
}

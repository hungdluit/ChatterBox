using ChatterBox.Common.Mvvm.Base;
using ChatterBox.Common.Mvvm.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;

namespace ChatterBox.Common.Mvvm.ViewModels
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

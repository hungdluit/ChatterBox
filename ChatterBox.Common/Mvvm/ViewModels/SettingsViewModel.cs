using ChatterBox.Common.Mvvm.Base;
using ChatterBox.Common.Mvvm.Utils;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;

namespace ChatterBox.Common.Mvvm.ViewModels
{
    internal class SettingsViewModel : DispatcherBindableBase
    {
        Frame _frame;

        public SettingsViewModel(CoreDispatcher uiDispatcher, Frame frame) :
            base(uiDispatcher)
        {
            _frame = frame;
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
            if (_frame.CanGoBack)
            {
                _frame.GoBack();
            }
        }
    }
}

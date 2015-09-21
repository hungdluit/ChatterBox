using ChatterBox.Common.Mvvm.Base;
using ChatterBox.Common.Mvvm.Utils;
using ChatterBox.Common.Views;
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
    internal class FirstPanelViewModel : DispatcherBindableBase
    {
        private Frame _frame;

        public FirstPanelViewModel(CoreDispatcher uiDispatcher, Frame frame) :
            base(uiDispatcher)
        {
            _frame = frame;
            SettingsButtonCommand = new ActionCommand(SettingsButtonExecute, SettingsButtonCanExecute);
        }

        private ICommand _settingsButtonCommand;
        public ICommand SettingsButtonCommand
        {
            get { return _settingsButtonCommand; }
            set { SetProperty(ref _settingsButtonCommand, value); }
        }

        private void SettingsButtonExecute(object obj)
        {
            _frame.Navigate(typeof(SettingsView), _frame);
        }

        private bool SettingsButtonCanExecute(object obj)
        {
            return true;
        }
    }
}

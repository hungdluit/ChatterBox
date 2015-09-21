using ChatterBox.Common.Mvvm.Base;
using ChatterBox.Common.Mvvm.Utils;
using ChatterBox.Common.Mvvm.ViewModels;
using ChatterBox.Common.Views;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace ChatterBox.Common.Mvvm
{
    internal class CompositeViewModel : DispatcherBindableBase
    {
        Frame _frame;

        public CompositeViewModel(CoreDispatcher uiDispatcher, Frame frame)
            : base(uiDispatcher)
        {
            _frame = frame;
            _frame.Navigated += _frame_Navigated;
            IsLeftPanelVisible = true;
            SettingsButtonCommand = new ActionCommand(SettingsButtonExecute, SettingsButtonCanExecute);
            _frame.Navigate(typeof(Page));
        }

        private void _frame_Navigated(object sender, NavigationEventArgs e)
        {
            if (e.NavigationMode == NavigationMode.Back &&
                _frame.BackStackDepth == 0)
            {
                IsLeftPanelVisible = true;
            }
        }        

        #region Binding Properties

        private bool _isLeftPanelVisible;
        public bool IsLeftPanelVisible
        {
            get { return _isLeftPanelVisible; }
            set { SetProperty(ref _isLeftPanelVisible, value); }
        }

        #endregion

        #region Commands

        private ICommand _settingsButtonCommand;
        public ICommand SettingsButtonCommand
        {
            get { return _settingsButtonCommand; }
            set { SetProperty(ref _settingsButtonCommand, value); }
        }

        private void SettingsButtonExecute(object obj)
        {
            IsLeftPanelVisible = false;
            _frame.Navigate(typeof(SettingsView), _frame);
        }

        private bool SettingsButtonCanExecute(object obj)
        {
            return true;
        }

        #endregion
    }
}

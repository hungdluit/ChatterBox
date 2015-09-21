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
            _frame.Navigate(typeof(FirstPanelView), frame);
        }

        private void _frame_Navigated(object sender, NavigationEventArgs e)
        {
        }        

        #region Binding Properties

        #endregion

        #region Commands

        #endregion
    }
}

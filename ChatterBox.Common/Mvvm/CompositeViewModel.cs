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
        public CompositeViewModel(CoreDispatcher uiDispatcher, NavigationService navigationService)
            : base(uiDispatcher, navigationService)
        {
        }     

        #region Binding Properties

        #endregion

        #region Commands

        #endregion
    }
}

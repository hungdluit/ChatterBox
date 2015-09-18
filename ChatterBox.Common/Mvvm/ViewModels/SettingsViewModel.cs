using ChatterBox.Common.Mvvm.Base;
using System;
using System.Collections.Generic;
using System.Text;
using Windows.UI.Core;

namespace ChatterBox.Common.Mvvm.ViewModels
{
    internal class SettingsViewModel : DispatcherBindableBase
    {
        public SettingsViewModel(CoreDispatcher uiDispatcher) :
            base(uiDispatcher)
        {

        }
    }
}

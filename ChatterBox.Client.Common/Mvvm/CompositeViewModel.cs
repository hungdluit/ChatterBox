using ChatterBox.Client.Common.Mvvm.Base;
using ChatterBox.Client.Common.Mvvm.Utils;
using Windows.UI.Core;

namespace ChatterBox.Client.Common.Mvvm
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

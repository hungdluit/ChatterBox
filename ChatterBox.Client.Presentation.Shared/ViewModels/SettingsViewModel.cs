using ChatterBox.Client.Presentation.Shared.MVVM;
using ChatterBox.Client.Presentation.Shared.MVVM.Utils;
using System;
using System.Collections.Generic;
using System.Text;

namespace ChatterBox.Client.Presentation.Shared.ViewModels
{
    public class SettingsViewModel : BindableBase
    {
        private ActionCommand _close;

        public SettingsViewModel()
        {
            Close = new ActionCommand(CloseExecute);
        }

        public ActionCommand Close
        {
            get { return _close; }
            set { SetProperty(ref _close, value); }
        }

        private void CloseExecute(object param)
        {
            OnCompleted?.Invoke();
        }

        public event Action OnCompleted;
    }
}

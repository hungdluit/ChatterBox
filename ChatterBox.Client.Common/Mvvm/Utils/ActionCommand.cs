using System;
using System.Windows.Input;

namespace ChatterBox.Client.Common.Mvvm.Utils
{
    internal class ActionCommand : ICommand
    {
        public delegate bool CanExecuteDelegate(object parameter);

        private readonly Action<object> _actionExecute;
        private readonly CanExecuteDelegate _actionCanExecute;

        public ActionCommand(Action<object> actionExecute, CanExecuteDelegate actionCanExecute = null)
        {
            _actionExecute = actionExecute;
            _actionCanExecute = actionCanExecute;
        }

        public bool CanExecute(object parameter)
        {
            return _actionCanExecute == null || _actionCanExecute(parameter);
        }

        public void Execute(object parameter)
        {
            _actionExecute(parameter);
        }

        public void RaiseCanExecuteChanged()
        {
            if (CanExecuteChanged != null)
            {
                CanExecuteChanged(this, null);
            }
        }

        public event EventHandler CanExecuteChanged;
    }
}

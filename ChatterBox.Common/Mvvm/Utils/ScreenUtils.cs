using System;
using Windows.Foundation;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;

namespace ChatterBox.Common.Mvvm.Utils
{
    internal class ScreenUtils
    {
        public enum AppState
        {
            ParallelState,
            OverlayState
        }

        public AppState CurrentState
        {
            get; set;
        }

        public event Action<AppState> StateChanged;

        public ScreenUtils()
        {
            Window.Current.SizeChanged += Current_SizeChanged;
            CurrentState = AppState.OverlayState;            
        }

        private void Current_SizeChanged(object sender, Windows.UI.Core.WindowSizeChangedEventArgs e)
        {
            GetCurrenState(e.Size);
        }

        private void GetCurrenState(Size size)
        {
            if (ApplicationView.GetForCurrentView().Orientation == ApplicationViewOrientation.Landscape)
            {
                if (size.Width > 1024)
                {
                    CurrentState = AppState.ParallelState;
                }
                else
                {
                    CurrentState = AppState.OverlayState;
                }
            }
            else
            {
                CurrentState = AppState.OverlayState;
            }
            if (StateChanged != null)
            {
                StateChanged(CurrentState);
            }
        }
    }
}

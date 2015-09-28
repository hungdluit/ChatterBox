using ChatterBox.Client.Common.Views;
using System;
using Windows.UI.Xaml.Controls;

namespace ChatterBox.Client.Common.Mvvm.Utils
{
    public sealed class NavigationService
    {
        private readonly Frame _leftFrame;
        private readonly Frame _rightFrame;
        private readonly string _rigthFrameEmptyState;
        private readonly ScreenUtils _screenUtils;

        public NavigationService(Frame leftFrame, Frame rightFrame)
        {
            _leftFrame = leftFrame;
            _rightFrame = rightFrame;
            _rigthFrameEmptyState = rightFrame.GetNavigationState();
            _screenUtils = new ScreenUtils();
            _screenUtils.StateChanged += _screenUtils_StateChanged;
        }

        private void _screenUtils_StateChanged(ScreenUtils.AppState newState)
        {
            if (newState == ScreenUtils.AppState.ParallelState)
            {
                _leftFrame.Navigate(typeof(FirstPanelView), this);
                if (_rightFrame.Content is FirstPanelView)
                {
                    ClearFrame(_rightFrame);
                }
            }
            if (newState == ScreenUtils.AppState.OverlayState)
            {
                ClearFrame(_leftFrame);
                if (_rightFrame.Content == null)
                {
                    _rightFrame.Navigate(typeof(FirstPanelView), this);
                }
            }
        }

        public void NavigateTo(Type pageType)
        {
            if (_screenUtils.CurrentState == ScreenUtils.AppState.OverlayState)
            {
                _rightFrame.Navigate(pageType, this);
            }
            else
            {
                if (pageType != typeof(FirstPanelView))
                {
                    _rightFrame.Navigate(pageType, this);
                }
            }
        }

        public void NavigateBack()
        {
            if (_rightFrame.CanGoBack)
            {
                _rightFrame.GoBack();
            }
            else
            {
                ClearFrame(_rightFrame);
            }
        }

        private void ClearFrame(Frame frame)
        {
            frame.ForwardStack.Clear();
            frame.BackStack.Clear();
            frame.SetNavigationState(_rigthFrameEmptyState);
            frame.Content = null;
        }
    }
}

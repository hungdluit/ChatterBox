using ChatterBox.Client.Presentation.Shared.MVVM.Utils;
using System;
using System.Collections.Generic;
using System.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace ChatterBox.Client.Presentation.Shared.Converters
{
    public class AppStateToStyleConverter : IValueConverter
    {
        private Style _parallelStyle;
        public Style ParallelStyle
        {
            get { return _parallelStyle; }
            set { _parallelStyle = value; }
        }

        private Style _overlayStyle;
        public Style OverlayStyle
        {
            get { return _overlayStyle; }
            set { _overlayStyle = value; }
        }

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var newState = (ScreenUtils.AppState)value;
            switch (newState)
            {
                case ScreenUtils.AppState.ParallelState:
                    return ParallelStyle;
                case ScreenUtils.AppState.OverlayState:
                    return OverlayStyle;
                default:
                    return OverlayStyle;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}

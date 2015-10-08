using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using ChatterBox.Client.Presentation.Shared.Services;

namespace ChatterBox.Client.Presentation.Shared.Converters
{
    public class LayoutTypeToStyleConverter : IValueConverter
    {
        public Style OverlayStyle { get; set; }
        public Style ParallelStyle { get; set; }

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var newState = (LayoutType) value;
            switch (newState)
            {
                case LayoutType.Parallel:
                    return ParallelStyle;
                case LayoutType.Overlay:
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
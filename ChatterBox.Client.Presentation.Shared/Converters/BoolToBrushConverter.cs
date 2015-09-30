using System;
using System.Collections.Generic;
using System.Text;
using Windows.UI;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

namespace ChatterBox.Client.Presentation.Shared.Converters
{
    public sealed class BoolToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var boolValue = (bool)value;
            Color color = boolValue ? ColorForTrue :
                                      ColorForFalse;
            return new SolidColorBrush(color);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }

        public Color ColorForTrue { get; set; }

        public Color ColorForFalse { get; set; }
    }
}

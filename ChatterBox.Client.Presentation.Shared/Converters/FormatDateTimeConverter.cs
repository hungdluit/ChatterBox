using System;
using Windows.UI.Xaml.Data;

namespace ChatterBox.Client.Presentation.Shared.Converters
{
    public class FormatDateTimeConverter : IValueConverter
    {
        public string Format { get; set; }

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null) return null;
            if (!(value is DateTime)) return value;

            return ((DateTime) value).ToString(Format);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
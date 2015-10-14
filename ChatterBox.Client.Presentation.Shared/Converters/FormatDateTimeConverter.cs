using System;
using System.Collections.Generic;
using System.Text;
using Windows.UI.Xaml.Data;

namespace ChatterBox.Client.Presentation.Shared.Converters
{
    public class FormatDateTimeConverter : IValueConverter
    {
        private string _format;

        public string Format
        {
            get { return _format; }
            set { _format = value; }
        }


        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null) return null;
            if (!(value is DateTime)) return value;
            
            return ((DateTime)value).ToString(Format);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}

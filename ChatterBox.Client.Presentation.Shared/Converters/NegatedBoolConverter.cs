using System;
using Windows.UI.Xaml.Data;

namespace ChatterBox.Client.Presentation.Shared.Converters
{
    public sealed class NegatedBoolConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (!(value is bool))
                throw new ArgumentException(string.Format("The value converted is not of type {0}", typeof (bool).Name));
            return !(bool) value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
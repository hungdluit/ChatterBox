﻿using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace ChatterBox.Client.Presentation.Shared.Converters
{
    public sealed class BooleanToVisibilityConverter : IValueConverter
    {
        public bool Inverted { get; set; }

        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (!(value is bool))
                throw new ArgumentException(string.Format("The value converted is not of type {0}", typeof (bool).Name));
            var booleanValue = (bool) value;

            return booleanValue
                ? (!Inverted ? Visibility.Visible : Visibility.Collapsed)
                : (!Inverted ? Visibility.Collapsed : Visibility.Visible);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
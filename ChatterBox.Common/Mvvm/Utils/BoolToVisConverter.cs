using System;
using System.Collections.Generic;
using System.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace ChatterBox.Common.Mvvm.Utils
{
    // <summary>
    /// Class provides functionality to convert from boolean to Visibility.
    /// Implements the IValueConverter interface.
    /// </summary>
    internal class BoolToVisConverter : IValueConverter
    {
        /// <summary>
        /// Converts a boolean to it's negated value.
        /// </summary>
        public bool Negated { get; set; }

        /// <summary>
        /// See the IValueConverter.Convert().
        /// </summary>
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var result = (bool)value;
            result = Negated ? !result : result;
            return result ? Visibility.Visible : Visibility.Collapsed;
        }

        /// <summary>
        /// See the IValueConverter.ConvertBack().
        /// </summary>
        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (Negated)
            {
                return (bool)value ? Visibility.Collapsed : Visibility.Visible;
            }
            return (bool)value ? Visibility.Visible : Visibility.Collapsed;
        }
    }
}

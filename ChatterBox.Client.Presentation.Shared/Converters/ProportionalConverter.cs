using System;
using Windows.UI.Xaml.Data;

namespace ChatterBox.Client.Presentation.Shared.Converters
{
    public class ProportionalConverter : IValueConverter
    {
        /// <summary>
        ///     Represented in percents - %. (e.g. 25)
        /// </summary>
        public int DownscalingFactor { get; set; } = 25;

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var refDimension = (double) value;
            var scaledDimension = (DownscalingFactor/100)*refDimension;
            return scaledDimension;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
using System;
using System.Collections.Generic;
using System.Text;
using Windows.UI.Xaml.Data;

namespace ChatterBox.Client.Presentation.Shared.Converters
{
    public class ProportionalConverter : IValueConverter
    {
        private int _downscalingFactor = 25;
        /// <summary>
        /// Represented in percents - %. (e.g. 25)
        /// </summary>
        public int DownscalingFactor
        {
            get { return _downscalingFactor; }
            set { _downscalingFactor = value; }
        }

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var refDimension = (double)value;
            var scaledDimension = (DownscalingFactor / 100) * refDimension;
            return scaledDimension;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}

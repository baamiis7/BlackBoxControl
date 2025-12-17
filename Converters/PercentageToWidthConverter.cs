using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace BlackBoxControl.Converters
{
    public class PercentageToWidthConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double percent)
            {
                // scale percent to container width (default 200px)
                double containerWidth = 200;

                if (parameter != null && double.TryParse(parameter.ToString(), out double paramWidth))
                    containerWidth = paramWidth;

                return (percent / 100.0) * containerWidth;
            }

            return 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => DependencyProperty.UnsetValue;
    }
}

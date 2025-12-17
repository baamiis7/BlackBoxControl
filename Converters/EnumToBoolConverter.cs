using System;
using System.Globalization;
using System.Windows.Data;

namespace BlackBoxControl.Converters
{
    /// <summary>
    /// Converts an enum value to boolean for RadioButton binding
    /// </summary>
    public class EnumToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null)
                return false;

            string enumValue = value.ToString();
            string targetValue = parameter.ToString();

            return enumValue.Equals(targetValue, StringComparison.OrdinalIgnoreCase);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null)
                return null;

            bool isChecked = (bool)value;
            if (!isChecked)
                return null;

            return Enum.Parse(targetType, parameter.ToString());
        }
    }
}
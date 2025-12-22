using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace BlackBoxControl.Converters
{
    /// <summary>
    /// Inverts a boolean value
    /// </summary>
    public class InvertBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
                return !boolValue;
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
                return !boolValue;
            return false;
        }
    }

    /// <summary>
    /// Converts boolean to Brush (Green if true, Red if false)
    /// </summary>
    public class BooleanToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
                return boolValue ? new SolidColorBrush(Colors.Green) : new SolidColorBrush(Colors.Red);
            return new SolidColorBrush(Colors.Gray);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Converts IsConnected to button text (Connect/Disconnect)
    /// </summary>
    public class ConnectButtonTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
                return boolValue ? "Disconnect" : "Connect";
            return "Connect";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Converts IsConnected to the appropriate command (Connect or Disconnect)
    /// </summary>
    public class ConnectCommandConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Converts IsConnected to button background color
    /// </summary>
    public class ConnectionColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                if (boolValue)
                {
                    return new SolidColorBrush(Color.FromRgb(244, 67, 54));  // Red for Disconnect
                }
                else
                {
                    return new SolidColorBrush(Color.FromRgb(76, 175, 80)); // Green for Connect
                }
            }
            return new SolidColorBrush(Colors.Gray);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

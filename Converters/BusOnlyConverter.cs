using BlackBoxControl.ViewModels;
using System;
using System.Globalization;
using System.Windows.Data;

namespace BlackBoxControl.Converters
{
    public class BusOnlyConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (value is TreeNodeType type && type == TreeNodeType.Bus)
                ? "Visible"
                : "Collapsed";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}

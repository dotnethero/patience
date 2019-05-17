using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Patience.Converters
{
    internal class ParameterVisibilityConverter: IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter == null) return Visibility.Collapsed;
            if (value == null) return Visibility.Collapsed;
            return parameter.ToString().Equals(value.ToString()) ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

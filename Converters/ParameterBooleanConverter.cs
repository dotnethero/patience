using System;
using System.Globalization;
using System.Windows.Data;

namespace Patience.Converters
{
    internal class ParameterBooleanConverter: IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter == null) return false;
            if (value == null) return false;
            return parameter.ToString().Equals(value.ToString());
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace MCLevelEdit.Converter
{
    public class Int16ToStringConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return value?.ToString() ?? "0";
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            short.TryParse(value?.ToString(), out short result);
            return result;
        }
    }
}

using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace MCLevelEdit.Converter
{
    public class UInt8ToStringConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value != null)
            {
                return value.ToString();
            }
            return "0";
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value != null)
            {
                byte i = 0;
                byte.TryParse(value.ToString(), out i);
                return i;
            }
            return (byte)0;
        }
    }
}

using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace MCLevelEdit.Converter
{
    public class UInt32ToStringConverter : IValueConverter
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
                uint i = 0;
                uint.TryParse(value.ToString(), out i);
                return i;
            }
            return (uint)0;
        }
    }
}

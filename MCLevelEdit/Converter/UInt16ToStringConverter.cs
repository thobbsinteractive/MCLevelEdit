using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace MCLevelEdit.Converter
{
    public class UInt16ToStringConverter : IValueConverter
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
                ushort i = 0;
                ushort.TryParse(value.ToString(), out i);
                return i;
            }
            return (ushort)0;
        }
    }
}

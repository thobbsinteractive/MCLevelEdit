using Avalonia.Data.Converters;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace MCLevelEdit.Converter;

public class KeyValuePairToNameConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value != null)
        {
            var keyValuePair = (KeyValuePair<int, string>)value;
            return keyValuePair.Value;
        }
        return null;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

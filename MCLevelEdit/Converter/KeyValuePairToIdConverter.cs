using Avalonia.Data.Converters;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace MCLevelEdit.Converter;

public class KeyValuePairToIdConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value != null)
        {
            var entityType = (KeyValuePair<int, string>)value;
            return entityType.Key;
        }
        return null;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value != null)
        {
            var entityType = (KeyValuePair<int, string>)value;
            return entityType.Key;
        }
        return null;
    }
}


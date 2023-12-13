using Avalonia.Data.Converters;
using MCLevelEdit.DataModel;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace MCLevelEdit.Converter;

public class TypeIdConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not null)
        {
            var entityType = ((TypeId)value).GetEntityFromTypeId();
            return new KeyValuePair<int, string>(key: (int)entityType.TypeId, value: Enum.GetName(typeof(TypeId), entityType.TypeId));
        }
        return null;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not null)
        {
            return ((KeyValuePair<int, string>)value).Key;
        }
        return null;
    }
}

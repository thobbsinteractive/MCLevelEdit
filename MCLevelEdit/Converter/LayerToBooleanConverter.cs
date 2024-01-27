using Avalonia.Data;
using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace MCLevelEdit.Converter;

public class LayerToBooleanConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value?.ToString().Equals(parameter);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        switch (parameter)
        {
            case "Game":
                return MCLevelEdit.Model.Enums.Layer.Game;
                break;
            case "Height":
                return MCLevelEdit.Model.Enums.Layer.Height;
        }

        return BindingOperations.DoNothing;
    }
}

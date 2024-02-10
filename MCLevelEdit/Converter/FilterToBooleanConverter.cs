using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace MCLevelEdit.Converter;

public class FilterToBooleanConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value?.ToString().Equals(parameter);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        switch (parameter)
        {
            case "Fail":
                return Model.Enums.Result.Fail;
            case "Warning":
                return Model.Enums.Result.Warning;
            case "Pass":
                return Model.Enums.Result.Pass;
            default:
                return Model.Enums.Result.None;
        }
    }
}

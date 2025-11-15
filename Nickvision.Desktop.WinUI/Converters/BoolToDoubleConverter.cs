using Microsoft.UI.Xaml.Data;
using System;

namespace Nickvision.Desktop.WinUI.Converters;

public class BoolToDoubleConverter : IValueConverter
{
    public double TrueValue { get; set; }
    public double FalseValue { get; set; }

    public object? Convert(object? value, Type targetType, object? parameter, string language)
    {
        if (value is bool boolValue)
        {
            return boolValue ? TrueValue : FalseValue;
        }
        return FalseValue;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        if (value is double doubleValue)
        {
            return doubleValue == TrueValue;
        }
        return false;
    }
}

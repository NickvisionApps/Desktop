using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using System;

namespace Nickvision.Desktop.WinUI.Converters;

public class BoolToStyleConverter : IValueConverter
{
    public Style? TrueValue { get; set; }
    public Style? FalseValue { get; set; }

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
        if (value is Style style)
        {
            return style == TrueValue;
        }
        return false;
    }
}

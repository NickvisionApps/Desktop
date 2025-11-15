using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using System;

namespace Nickvision.Desktop.WinUI.Converters;

public class NullableToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language) => value is not null ? Visibility.Visible : Visibility.Collapsed;

    public object ConvertBack(object value, Type targetType, object parameter, string language) => throw new NotImplementedException();
}

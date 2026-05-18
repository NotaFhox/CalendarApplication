using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;

namespace Calender.Converters;

/// <summary>
/// Converts <c>bool</c> → <see cref="Visibility"/>.
/// Pass <c>ConverterParameter=Invert</c> to flip the result.
/// </summary>
public sealed class BoolToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        bool flag = value is true;
        if (parameter is string s && s == "Invert") flag = !flag;
        return flag ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
        => throw new NotImplementedException();
}

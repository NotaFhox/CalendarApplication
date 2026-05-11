using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;

namespace Calender.Converters;

/// Converts bool → Visibility.
er
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

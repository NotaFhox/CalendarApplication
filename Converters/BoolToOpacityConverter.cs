using Microsoft.UI.Xaml.Data;

namespace Calender.Converters;

/// Returns 1.0 for true (current-month days) and 0.35 for false (padding days).

public sealed class BoolToOpacityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
        => value is true ? 1.0 : 0.35;

    public object ConvertBack(object value, Type targetType, object parameter, string language)
        => throw new NotImplementedException();
}

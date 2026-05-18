using Microsoft.UI.Xaml.Data;

namespace Calender.Converters;

/// <summary>
/// Returns <c>1.0</c> for <c>true</c> (current-month days) and
/// <c>0.35</c> for <c>false</c> (padding days from adjacent months).
/// </summary>
public sealed class BoolToOpacityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
        => value is true ? 1.0 : 0.35;

    public object ConvertBack(object value, Type targetType, object parameter, string language)
        => throw new NotImplementedException();
}

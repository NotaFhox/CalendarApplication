using Microsoft.UI;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;

namespace Calender.Converters;

/// Returns the system accent brush when the value is true (used to highlight today's date badge).
/// Returns a transparent brush when false.
public sealed class BoolToAccentBrushConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is true)
            return new SolidColorBrush(Microsoft.UI.Xaml.Application.Current.Resources
                .TryGetValue("SystemAccentColor", out var accent) && accent is Windows.UI.Color c
                ? c
                : Colors.SteelBlue);

        return new SolidColorBrush(Colors.Transparent);
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
        => throw new NotImplementedException();
}

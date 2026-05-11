using Microsoft.UI;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;

namespace Calender.Converters;

public sealed class StringToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is string hex && hex.StartsWith('#') && hex.Length == 7)
        {
            try
            {
                return new SolidColorBrush(Windows.UI.Color.FromArgb(
                    255,
                    System.Convert.ToByte(hex[1..3], 16),
                    System.Convert.ToByte(hex[3..5], 16),
                    System.Convert.ToByte(hex[5..7], 16)));
            }
            catch { /* fall through to default */ }
        }
        return new SolidColorBrush(Colors.SteelBlue);
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
        => throw new NotImplementedException();
}

using System.Globalization;

namespace Point_v1.Converters;

public class IsRelevantToBorderColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool isRelevant && isRelevant)
        {
            return Color.FromArgb("#4CAF50");
        }
        return Colors.Transparent;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
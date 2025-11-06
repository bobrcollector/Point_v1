using System.Globalization;

namespace Point_v1.Converters;

public class IsRelevantToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool isRelevant && isRelevant)
        {
            // Более заметный светло-зеленый фон
            return Color.FromArgb("#F1F8E9");
        }
        // Стандартный фон
        return Color.FromArgb("{AppThemeBinding Light=White, Dark=#2C2C2E}");
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
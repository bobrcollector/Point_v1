using System.Globalization;
using Microsoft.Maui.Controls;

namespace Point_v1.Converters;

public class IsRelevantToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var isDark = Application.Current?.RequestedTheme == AppTheme.Dark;
        
        if (value is bool isRelevant && isRelevant)
        {
            // Более заметный светло-зеленый фон для светлой темы, темнее для темной
            return isDark ? Color.FromArgb("#1B5E20") : Color.FromArgb("#F1F8E9");
        }
        // Стандартный фон
        return isDark ? Color.FromArgb("#2C2C2E") : Colors.White;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
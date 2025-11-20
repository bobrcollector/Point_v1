using System.Globalization;
using Microsoft.Maui.Controls;

namespace Point_v1.Converters;

public class PendingReportsColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is int count)
        {
            var isDark = Application.Current?.RequestedTheme == AppTheme.Dark;
            
            if (isDark)
            {
                // Темные версии цветов для темной темы
                return count switch
                {
                    0 => Color.FromArgb("#2E7D32"), // Зеленый - темнее
                    <= 5 => Color.FromArgb("#E65100"), // Оранжевый - темнее
                    _ => Color.FromArgb("#C62828") // Красный - темнее
                };
            }
            else
            {
                // Светлые версии цветов для светлой темы
                return count switch
                {
                    0 => Color.FromArgb("#4CAF50"), // Зеленый
                    <= 5 => Color.FromArgb("#FF9800"), // Оранжевый
                    _ => Color.FromArgb("#F44336") // Красный
                };
            }
        }
        return Application.Current?.RequestedTheme == AppTheme.Dark 
            ? Color.FromArgb("#2E7D32") 
            : Color.FromArgb("#4CAF50");
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
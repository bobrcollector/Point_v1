using System.Globalization;
using Microsoft.Maui.Controls;

namespace Point_v1.Converters;

public class InterestSelectionToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        System.Diagnostics.Debug.WriteLine($"🎨 Конвертер цвета вызван: {value}");
        
        var isDark = Application.Current?.RequestedTheme == AppTheme.Dark;
        
        if (value is bool isSelected)
        {
            if (isSelected)
            {
                return Color.FromArgb("#512BD4"); // Фиолетовый для выбранного
            }
            else
            {
                // Серый для невыбранного, темнее для темной темы
                return isDark ? Color.FromArgb("#3A3A3A") : Color.FromArgb("#E8E8E8");
            }
        }
        return isDark ? Color.FromArgb("#3A3A3A") : Color.FromArgb("#E8E8E8");
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class InterestSelectionToTextColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        System.Diagnostics.Debug.WriteLine($"🎨 Конвертер текста вызван: {value}");
        
        var isDark = Application.Current?.RequestedTheme == AppTheme.Dark;
        
        if (value is bool isSelected)
        {
            if (isSelected)
            {
                return Colors.White; // Белый для выбранного
            }
            else
            {
                // Для невыбранного учитываем тему
                return isDark ? Colors.White : Colors.Black;
            }
        }
        return isDark ? Colors.White : Colors.Black;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
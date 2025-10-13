using System.Globalization;

namespace Point_v1.Converters;

public class InterestSelectionToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        System.Diagnostics.Debug.WriteLine($"🎨 Конвертер цвета вызван: {value}");
        
        if (value is bool isSelected)
        {
            return isSelected ? 
                Color.FromArgb("#512BD4") : // Фиолетовый для выбранного
                Color.FromArgb("#E8E8E8");  // Серый для невыбранного
        }
        return Color.FromArgb("#E8E8E8");
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
        
        if (value is bool isSelected)
        {
            return isSelected ? 
                Colors.White : // Белый для выбранного
                Colors.Black;  // Черный для невыбранного
        }
        return Colors.Black;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
using System.Globalization;

namespace Point_v1.Converters;

public class BoolToBorderColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool isSelected && isSelected)
        {
            return Color.FromArgb("#7B1FA2"); // Темно-фиолетовая граница
        }
        return Colors.Transparent; // Прозрачная граница
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
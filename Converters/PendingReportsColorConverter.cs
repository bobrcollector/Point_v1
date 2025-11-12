using System.Globalization;

namespace Point_v1.Converters;

public class PendingReportsColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is int count)
        {
            return count switch
            {
                0 => Color.FromArgb("#4CAF50"), // Зеленый - нет жалоб
                <= 5 => Color.FromArgb("#FF9800"), // Оранжевый - мало жалоб
                _ => Color.FromArgb("#F44336") // Красный - много жалоб
            };
        }
        return Color.FromArgb("#4CAF50");
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
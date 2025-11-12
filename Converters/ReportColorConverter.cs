using System.Globalization;
using Point_v1.Models;

namespace Point_v1.Converters;


public class ReportColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is ReportStatus status)
        {
            return status switch
            {
                ReportStatus.Pending => Color.FromArgb("#FFF3CD"), // Желтый
                ReportStatus.Approved => Color.FromArgb("#D4EDDA"), // Зеленый
                ReportStatus.Rejected => Color.FromArgb("#F8D7DA"), // Красный
                _ => Colors.White
            };
        }
        return Colors.White;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
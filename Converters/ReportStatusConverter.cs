using System.Globalization;
using Point_v1.Models;

namespace Point_v1.Converters;

public class ReportStatusConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is ReportStatus status)
        {
            return status switch
            {
                ReportStatus.Pending => "⏳ Ожидает",
                ReportStatus.Approved => "✅ Одобрена",
                ReportStatus.Rejected => "❌ Отклонена",
                _ => "Неизвестно"
            };
        }
        return "Неизвестно";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
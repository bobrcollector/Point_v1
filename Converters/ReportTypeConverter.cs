using System.Globalization;
using Point_v1.Models;


namespace Point_v1.Converters;


public class ReportTypeConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is ReportType type)
        {
            return type switch
            {
                ReportType.Spam => "📧 Спам",
                ReportType.Inappropriate => "🚫 Неуместный контент",
                ReportType.Scam => "💸 Мошенничество",
                ReportType.Illegal => "⚖️ Нелегальное",
                ReportType.Other => "❓ Другое",
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
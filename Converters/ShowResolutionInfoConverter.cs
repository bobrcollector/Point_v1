using System.Globalization;
using Point_v1.Models;

namespace Point_v1.Converters;

public class ShowResolutionInfoConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is ReportStatus status)
        {
            return status != ReportStatus.Pending;
        }
        return false;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
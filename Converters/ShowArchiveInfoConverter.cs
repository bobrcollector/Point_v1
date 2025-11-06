using System.Globalization;

namespace Point_v1.Converters;

public class ShowArchiveInfoConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is string selectedTab && selectedTab == "Archived";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
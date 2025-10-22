using System.Globalization;

namespace Point_v1.Converters;

public class BoolToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool boolValue && boolValue)
        {
            // Активный - фиолетовый
            return Color.FromArgb("#512BD4");
        }
        else
        {
            // Неактивный - серый
            return Application.Current.RequestedTheme == AppTheme.Dark
                ? Color.FromArgb("#3A3A3A")
                : Color.FromArgb("#E0E0E0");
        }
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
namespace Point_v1.Converters;

public class BoolToColorConverter : IValueConverter
{
    public Color TrueColor { get; set; } = Color.FromArgb("#4CAF50"); // Зеленый
    public Color FalseColor { get; set; } = Color.FromArgb("#FF6B6B"); // Красный

    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        if (value is bool boolValue)
        {
            return boolValue ? TrueColor : FalseColor;
        }
        return FalseColor;
    }

    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
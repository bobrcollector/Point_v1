using Point_v1.ViewModels;
using System.Globalization;

namespace Point_v1.Converters;

public class PasswordLengthToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is int length)
        {
            return length >= 6 ? "#2E7D32" : "#D32F2F"; // Зеленый если >=6, красный если меньше
        }
        return "#666666"; // Серый по умолчанию
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class PasswordMatchConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string confirmPassword && parameter is Binding binding)
        {
            var source = binding.Source as AuthViewModel;
            if (source != null)
            {
                return source.Password == confirmPassword ? "#2E7D32" : "#D32F2F";
            }
        }
        return "#666666";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class PasswordMatchTextConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string confirmPassword && parameter is Binding binding)
        {
            var source = binding.Source as AuthViewModel;
            if (source != null)
            {
                if (string.IsNullOrEmpty(confirmPassword))
                    return "Подтвердите пароль";

                return source.Password == confirmPassword ? "Пароли совпадают" : "Пароли не совпадают";
            }
        }
        return "Подтвердите пароль";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
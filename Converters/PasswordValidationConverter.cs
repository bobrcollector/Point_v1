using Point_v1.ViewModels;
using System.Globalization;

namespace Point_v1.Converters;

public class PasswordLengthToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is int length)
        {
            return length >= 6 ? "#2E7D32" : "#D32F2F"; // ������� ���� >=6, ������� ���� ������
        }
        return "#666666"; // ����� �� ���������
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
                    return "����������� ������";

                return source.Password == confirmPassword ? "������ ���������" : "������ �� ���������";
            }
        }
        return "����������� ������";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
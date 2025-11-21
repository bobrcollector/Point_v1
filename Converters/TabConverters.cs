using System.Globalization;

namespace Point_v1.Converters;

public class TabColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var selectedTab = value as string;
        var tabParameter = parameter as string;

        return selectedTab == tabParameter ?
            Application.Current.RequestedTheme == AppTheme.Dark ? "#512BD4" : "#512BD4" :
            Colors.Transparent;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class TabTextColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var selectedTab = value as string;
        var tabParameter = parameter as string;

        return selectedTab == tabParameter ? Colors.White :
            Application.Current.RequestedTheme == AppTheme.Dark ? Colors.LightGray : Colors.Black;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class ShowActionsConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var selectedTab = value as string;
        return selectedTab == "Created" || selectedTab == "Participating";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class ShowEditButtonConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var selectedTab = value as string;
        return selectedTab == "Created";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class ShowLeaveButtonConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var selectedTab = value as string;
        return selectedTab == "Participating";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

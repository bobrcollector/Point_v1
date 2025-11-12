using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

public class TabTextColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string selectedTab && parameter is string tabName)
        {
            return selectedTab == tabName ? "White" : "#512BD4";
        }
        return "#512BD4";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
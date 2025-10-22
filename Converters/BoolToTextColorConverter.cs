﻿using System.Globalization;

namespace Point_v1.Converters;

public class BoolToTextColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool boolValue && boolValue)
        {
            return Colors.White;
        }
        return Application.Current.RequestedTheme == AppTheme.Dark
            ? Color.FromArgb("#CCCCCC")
            : Color.FromArgb("#666666");
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
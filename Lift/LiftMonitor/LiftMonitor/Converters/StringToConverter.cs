using Avalonia.Data;
using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace LiftMonitor.Converters
{
    public class StringToConverter : IValueConverter
    {
        public static StringToConverter Instance = new();

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is string stringValue && int.TryParse(stringValue, out var result)
            ? result 
            : new BindingNotification(new InvalidCastException(), BindingErrorType.Error);


        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();

    }
}

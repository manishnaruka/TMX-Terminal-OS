using System;
using System.Globalization;
using System.Windows.Data;

namespace TMXWinTerminal.Helpers
{
    public class UtcToLocalConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DateTime utc && utc != default(DateTime))
            {
                var local = utc.Kind == DateTimeKind.Utc
                    ? utc.ToLocalTime()
                    : DateTime.SpecifyKind(utc, DateTimeKind.Utc).ToLocalTime();
                return local.ToString("g", culture);
            }

            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}

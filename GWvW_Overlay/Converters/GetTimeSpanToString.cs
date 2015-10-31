using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace GWvW_Overlay.Converters
{
    public class GetTimeSpanToString : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is TimeSpan)
            {
                var v = (TimeSpan)value;
                return v.TotalSeconds == 0 ? "" : v.ToString(@"mm\:ss");
            }
            return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
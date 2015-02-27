using System;
using System.Windows.Data;

namespace GWvW_Overlay.Converters
{
    public class GetName : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if ((bool)Properties.Settings.Default["show_names"])
                return value;
            else
                return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
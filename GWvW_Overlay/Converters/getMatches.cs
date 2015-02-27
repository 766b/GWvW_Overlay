using System;
using System.Windows.Data;

namespace GWvW_Overlay.Converters
{
    public class getMatches : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (values.Length < 4)
                return "Value count is low";

            return string.Format("{0}. {1} vs {2} vs {3}", values[0], values[1], values[2], values[3]);
        }
        public object[] ConvertBack(object values, Type[] targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
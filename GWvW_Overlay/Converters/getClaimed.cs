using System;
using System.Windows.Data;

namespace GWvW_Overlay.Converters
{
    public class GetClaimed : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value != null)
            {
                return "visible";
            }
            else
            {
                return "collapsed";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
using System;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace GWvW_Overlay.Converters
{
    public class getClaimedImage : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value != null)
                return new BitmapImage(new Uri("Resources/claimed2.png", UriKind.Relative));
            else
                return new BitmapImage(new Uri("Resources/empty.png", UriKind.Relative));
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
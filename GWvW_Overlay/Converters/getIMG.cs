using System;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace GWvW_Overlay.Converters
{
    public class getIMG : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (values.Length == 1)
                return getPNG(values[0], null);
            if (values.Length == 2)
                return getPNG(values[0], values[1]);


            return null;
        }
        public object[] ConvertBack(object values, Type[] targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        private ImageSource getPNG(object type, object color)
        {
            string y;
            if (color == null || color.ToString() == "none")
            {
                y = string.Format("Resources/{0}.png", type);
            }
            else
            {
                y = string.Format("Resources/{0}_{1}.png", type, color.ToString().ToLower());

            }

            ImageSource x = new BitmapImage(new Uri(y, UriKind.Relative));
            return x;
        }
    }
}
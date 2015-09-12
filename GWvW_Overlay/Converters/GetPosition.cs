using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using MumbleLink_CSharp_GW2;

namespace GWvW_Overlay.Converters
{
    public class GetPosition : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (string)parameter == "Top" ? (((GW2Link.Coordinates)value).Y) : ((GW2Link.Coordinates)value).X;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

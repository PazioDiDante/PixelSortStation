using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Nika_Vizard_Station.Converters
{
    public class TimeLineConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double dbl)
            {
                int wholePart = (int)Math.Floor(dbl);
                double fractionPart = dbl - wholePart;
                int timeTick = (int)(Math.Round(fractionPart * 0.23, 2)*100);
                return $"{wholePart.ToString("D2")}:{timeTick.ToString("D2")}";
            }

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

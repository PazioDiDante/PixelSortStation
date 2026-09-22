using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace Nika_Vizard_Station.Converters
{
    public class IntToGradientBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int intValue)
            {
                switch (intValue)
                {
                    case 0:
                        return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFDF815D"));

                        return Brushes.Transparent;
                    case 1:
                        return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFDF815D"));
                    case 2:
                        return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF8BCA74"));
                    default:
                        return Brushes.Transparent;
                }
            }

            return Brushes.Transparent;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

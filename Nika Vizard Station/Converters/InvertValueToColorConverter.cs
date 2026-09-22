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
    public class InvertValueToColorConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length == 3 && values[0] is double sliderValue && values[1] is double min && values[2] is double max)
            {
                double normalizedValue = (max - min != 0) ? (sliderValue - min) / (max - min) : 1;

                Color startColor = (Color)ColorConverter.ConvertFromString("#FFEA8759"); 
                Color endColor = (Color)ColorConverter.ConvertFromString("#FF1578A6"); 

                // Интерполяция между начальным и конечным цветами
                byte r = (byte)(startColor.R + (endColor.R - startColor.R) * normalizedValue);
                byte g = (byte)(startColor.G + (endColor.G - startColor.G) * normalizedValue);
                byte b = (byte)(startColor.B + (endColor.B - startColor.B) * normalizedValue);

                return new SolidColorBrush(Color.FromArgb(255, r, g, b));
            }

            return new SolidColorBrush(Colors.Black);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

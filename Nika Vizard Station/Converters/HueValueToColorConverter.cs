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
    public class HueValueToColorConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length == 3 && values[0] is double sliderValue && values[1] is double min && values[2] is double max)
            {
                double normalizedValue = (max - min != 0) ? (sliderValue - min) / (max - min) : 0;

                Color color0 = (Color)ColorConverter.ConvertFromString("#FFEA8267"); 
                Color color25 = (Color)ColorConverter.ConvertFromString("#FF8BD376"); 
                Color color50 = (Color)ColorConverter.ConvertFromString("#FF00ADE3"); 
                Color color75 = (Color)ColorConverter.ConvertFromString("#FF8E85E4"); 

                return new SolidColorBrush(GetColorForValue(normalizedValue, color0, color25, color50, color75));
            }

            return new SolidColorBrush(Colors.Black);
        }

        private Color GetColorForValue(double value, Color color0, Color color25, Color color50, Color color75)
        {
            Color startColor, endColor;
            double segmentValue;

            if (value <= 0.25)
            {
                startColor = color0;
                endColor = color25;
                segmentValue = value / 0.25;
            }
            else if (value <= 0.50)
            {
                startColor = color25;
                endColor = color50;
                segmentValue = (value - 0.25) / 0.25;
            }
            else if (value <= 0.75)
            {
                startColor = color50;
                endColor = color75;
                segmentValue = (value - 0.50) / 0.25;
            }
            else
            {
                startColor = color75;
                endColor = color0; 
                segmentValue = (value - 0.75) / 0.25;
            }          
            return Color.FromRgb(
                (byte)(startColor.R + (endColor.R - startColor.R) * segmentValue),
                (byte)(startColor.G + (endColor.G - startColor.G) * segmentValue),
                (byte)(startColor.B + (endColor.B - startColor.B) * segmentValue)
            );
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

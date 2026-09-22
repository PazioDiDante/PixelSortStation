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
    public class TemperatureValueToColorConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length == 3 && values[0] is double sliderValue && values[1] is double min && values[2] is double max)
            {
                double normalizedValue = (max - min != 0) ? (sliderValue - min) / (max - min) : 0;

                // Определение ключевых цветов
                Color color0 = (Color)ColorConverter.ConvertFromString("#FF00ADE3"); // 0%
                Color color50 = (Color)ColorConverter.ConvertFromString("#FF989898"); // 50%
                Color color100 = (Color)ColorConverter.ConvertFromString("#FFEA8759"); // 100%

                // Определение текущего сегмента и интерполяция между цветами
                return new SolidColorBrush(GetColorForValue(normalizedValue, color0, color50, color100));
            }

            return new SolidColorBrush(Colors.Black);
        }

        private Color GetColorForValue(double value, Color color0, Color color50, Color color100)
        {
            Color startColor, endColor;
            double segmentValue;

            if (value <= 0.50)
            {
                startColor = color0;
                endColor = color50;
                segmentValue = value / 0.50; // Процентное значение в пределах первого сегмента
            }
            else
            {
                startColor = color50;
                endColor = color100;
                segmentValue = (value - 0.50) / 0.50; // Процентное значение в пределах второго сегмента
            }

            // Интерполяция между двумя цветами
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

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
    public class SliderValueToColorConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length == 3 && values[0] is double sliderValue && values[1] is double min && values[2] is double max)
            {
                double normalizedValue = 1;
                if (max - min != 0) // Проверка, чтобы избежать деления на ноль
                {
                    normalizedValue = (sliderValue - min) / (max - min);
                }

                // Получение цвета
                byte colorValue = (byte)(normalizedValue * 255);
                return new SolidColorBrush(Color.FromArgb(255, colorValue, colorValue, colorValue));
            }

            return new SolidColorBrush(Colors.Black);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

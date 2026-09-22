using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Windows.Media.Animation;

namespace Nika_Vizard_Station.Services
{
    public static class RandomService
    {
        public static void AnimGeneratePSsettings(double rndPower, List<KeyPoint> keys)
        {
            var result = new PixelSortingSettings();

            AnimationService.AddKeyPoint(keys, 1, 0, Fields.Sorting);
            AddRandomKeys(0, 50, 30, rndPower, keys, Fields.MaskValue1);
            AddRandomKeys(51, 100, 75, rndPower, keys, Fields.MaskValue2);
            return;
        }
        public static void AnimGenerateCEsettings(double rndPower, List<KeyPoint> keys)
        {

            var result = new ColorEditingSettings();


            AddRandomKeys(-0.5, 0.5, 0, rndPower, keys, Fields.Brightness);
            AddRandomKeys(0.5, 1.5, 1, rndPower, keys, Fields.Contrast);
            AddRandomKeys(0, 2, 1, rndPower, keys, Fields.Saturation);
            AddRandomKeys(0, 359, 1, rndPower, keys, Fields.Hue);
            AddRandomKeys(0, 0.5, 0, rndPower, keys, Fields.Fade);
            AddRandomKeys(-1, 1, 0, rndPower, keys, Fields.Temperature);
            AddRandomKeys(8.9, 9.1, 9, rndPower, keys, Fields.Sharpen);

            AddRandomKeys(0, 255, 0, rndPower, keys, Fields.RGBCurvePoint1);
            AddRandomKeys(0, 255, 63, rndPower, keys, Fields.RGBCurvePoint2);
            AddRandomKeys(0, 255, 128, rndPower, keys, Fields.RGBCurvePoint3);
            AddRandomKeys(0, 255, 191, rndPower, keys, Fields.RGBCurvePoint4);
            AddRandomKeys(0, 255, 255, rndPower, keys, Fields.RGBCurvePoint5);

            AddRandomKeys(0, 255, 0, rndPower, keys, Fields.RedCurvePoint1);
            AddRandomKeys(0, 255, 63, rndPower, keys, Fields.RedCurvePoint2);
            AddRandomKeys(0, 255, 128, rndPower, keys, Fields.RedCurvePoint3);
            AddRandomKeys(0, 255, 191, rndPower, keys, Fields.RedCurvePoint4);
            AddRandomKeys(0, 255, 255, rndPower, keys, Fields.RedCurvePoint5);

            AddRandomKeys(0, 255, 0, rndPower, keys, Fields.GreenCurvePoint1);
            AddRandomKeys(0, 255, 63, rndPower, keys, Fields.GreenCurvePoint2);
            AddRandomKeys(0, 255, 128, rndPower, keys, Fields.GreenCurvePoint3);
            AddRandomKeys(0, 255, 191, rndPower, keys, Fields.GreenCurvePoint4);
            AddRandomKeys(0, 255, 255, rndPower, keys, Fields.GreenCurvePoint5);

            AddRandomKeys(0, 255, 0, rndPower, keys, Fields.BlueCurvePoint1);
            AddRandomKeys(0, 255, 63, rndPower, keys, Fields.BlueCurvePoint2);
            AddRandomKeys(0, 255, 128, rndPower, keys, Fields.BlueCurvePoint3);
            AddRandomKeys(0, 255, 191, rndPower, keys, Fields.BlueCurvePoint4);
            AddRandomKeys(0, 255, 255, rndPower, keys, Fields.BlueCurvePoint5);
            return;
        }
        public static PixelSortingSettings GeneratePSsettings(double rndPower)
        {
            var result = new PixelSortingSettings();
            result.SortFilter = (SortBy)GetRandomValue(0, 7, 4, 1);
            if ((int)result.SortFilter == 3)
                result.SortFilter++;

            result.MaskFilter = (SortBy)GetRandomValue(0, 7, 4, 1);
            if ((int)result.MaskFilter == 3)
                result.MaskFilter++;

            if ((byte)result.MaskFilter < 3)
            {
                result.MaskStart = (int)GetRandomValue(0, 125, 50, rndPower);
                result.MaskEnd = (int)GetRandomValue(126, 255, 205, rndPower);
            }
            if ((byte)result.MaskFilter == 4)
            {
                result.MaskStart = (int)GetRandomValue(0, 180, 100, rndPower);
                result.MaskEnd = (int)GetRandomValue(181, 359, 300, rndPower);
            }
            if ((byte)result.MaskFilter > 4)
            {
                result.MaskStart = (int)GetRandomValue(0, 50, 25, rndPower);
                result.MaskEnd = (int)GetRandomValue(50, 100, 75, rndPower);
            }
            return result;
        }

        public static ColorEditingSettings GenerateCEsettings(double rndPower)
        {
            var result = new ColorEditingSettings();
            result.Brightness = (float)GetRandomValue(-0.5, 0.5, 0, rndPower);
            result.Contrast = (float)GetRandomValue(0.5, 1.5, 1, rndPower);
            result.Saturation = (float)GetRandomValue(0, 2, 1, rndPower);
            result.Hue = (float)GetRandomValue(0, 359, 0, rndPower);
            result.Invert = 0;
            result.Fade = (float)GetRandomValue(0, 0.5, 0, rndPower);
            result.Temperature = (float)GetRandomValue(-1, 1, 0, rndPower);

            result.Sharpen = 9;



            if (_random.NextDouble() > 0.5)
            {
                result.RedCurveValue = new double[5] { 0, 63, 128, 191, 255 };
            }
            else
            {
                result.RedCurveValue = new double[5]
                {
                GetRandomValue(0,255,0,rndPower),
                GetRandomValue(0,255,63,rndPower),
                GetRandomValue(0,255,128,rndPower),
                GetRandomValue(0,255,191,rndPower),
                GetRandomValue(0,255,255,rndPower),
                };
            }

            if (_random.NextDouble() > 0.5)
            {
                result.BlueCurveValue = new double[5] { 0, 63, 128, 191, 255 };
            }
            else
            {
                result.BlueCurveValue = new double[5]
                {
                GetRandomValue(0,255,0,rndPower),
                GetRandomValue(0,255,63,rndPower),
                GetRandomValue(0,255,128,rndPower),
                GetRandomValue(0,255,191,rndPower),
                GetRandomValue(0,255,255,rndPower),
                };
            }
            if (_random.NextDouble() > 0.5)
            {
                result.GreenCurveValue = new double[5] { 0, 63, 128, 191, 255 };
            }
            else
            {
                result.GreenCurveValue = new double[5]
                {
                GetRandomValue(0,255,0,rndPower),
                GetRandomValue(0,255,63,rndPower),
                GetRandomValue(0,255,128,rndPower),
                GetRandomValue(0,255,191,rndPower),
                GetRandomValue(0,255,255,rndPower),
                };
            }

            result.RGBCurveValue = new double[5]
                {
                GetRandomValue(0,255,0,rndPower),
                GetRandomValue(0,255,63,rndPower),
                GetRandomValue(0,255,128,rndPower),
                GetRandomValue(0,255,191,rndPower),
                GetRandomValue(0,255,255,rndPower),
                };

            return result;
        }


        private static void AddRandomKeys(double min, double max, double average, double randomStrength, List<KeyPoint> keys, Fields field)
        {
            if (_random.NextDouble() > 0.5)
                return;

            var keyCount = GetRandomValue(1, 5, 2, randomStrength);
            for (int i = 0; i < keyCount; i++)
            {
                var randomTime = GetRandomValue(0, 10, 5, 1);
                int wholePart = (int)Math.Floor(randomTime);
                double fractionPart = randomTime - wholePart;
                var time = wholePart + (Math.Round(fractionPart * 0.23, 2));

                AnimationService.AddKeyPoint(
                    keys,
                    GetRandomValue(min, max, average, randomStrength),
                    time,
                    field);
            }
        }
        private static readonly Random _random = new Random();
        private static double GetRandomValue(double min, double max, double average, double randomStrength)
        {
            if (_random.NextDouble() > randomStrength)
            {
                if (_random.NextDouble() > 0.5)
                    return average;
            }

            double minRange = average - (average - min) * randomStrength;
            double maxRange = average + (max - average) * randomStrength;

            return _random.NextDouble() * (maxRange - minRange) + minRange;
        }
    }
}

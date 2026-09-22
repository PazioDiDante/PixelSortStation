using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Net.Sockets;
using System.Runtime;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using static Nika_Vizard_Station.MainWindow;

namespace Nika_Vizard_Station.Services
{
    public static class AnimationService
    {
        public static void AddKeyPoint(List<KeyPoint> keyPoints, double value, double time, Fields fieldName)
        {
            var keyPoint = keyPoints.FirstOrDefault(x => x.Time == time && x.FieldName == fieldName);
            if (keyPoint == null)
            {
                var point = new KeyPoint(time, value, fieldName);
                keyPoints.Add(point);
            }
            else
            {
                keyPoint.Value = value;
            }
            return;
        }

        public static void RemoveKeyPoint(List<KeyPoint> keyPoints, List<Forms> layers, Fields fieldName)
        {
            var keys = keyPoints.RemoveAll(x => x.FieldName == fieldName);
            
            var colorKeys = keyPoints.Where(x => (int)x.FieldName < 28).ToList();
            if (colorKeys.Count == 0)
            {
                layers.Remove(Forms.ColorEditing);
            }
            var sortKeys = keyPoints.Where(x => (int)x.FieldName > 27 && (int)x.FieldName < 32).ToList();
            if (sortKeys.Count == 0)
            {
                layers.Remove(Forms.PixelSorting);
            }
        }


        public static PixelSortingSettings GetPixelSortingSettins(List<KeyPoint> keyPoints, double time, PixelSortingSettings currentSettings)
        {
            var sorting = new PixelSortingSettings();
            var maskValue1 = GetValue(keyPoints, time, Fields.MaskValue1) ?? currentSettings.MaskStart;
            var maskValue2 = GetValue(keyPoints, time, Fields.MaskValue2) ?? currentSettings.MaskEnd;
            sorting.MaskStart = (int)Math.Min(maskValue1, maskValue2);
            sorting.MaskEnd = (int)Math.Max(maskValue1, maskValue2);

            var fractionTimePart = Math.Round((time - Math.Floor(time)) * 100, 0);

            var isSortingAnimated = GetValue(keyPoints, time, Fields.Sorting);
            if (isSortingAnimated != null)
            {
                SortBy sort = SortBy.Red;

                if (fractionTimePart % 3 == 0)
                    sort = SortBy.Red;
                if (fractionTimePart % 3 == 1)
                    sort = SortBy.Hue;
                if (fractionTimePart % 3 == 2)
                    sort = SortBy.Saturation;

                sorting.SortFilter = sort;
            }
            else
            {
                sorting.SortFilter = currentSettings.SortFilter;
            }
            var isMaskAnimated = GetValue(keyPoints, time, Fields.Mask);
            if (isMaskAnimated != null)
            {
                SortBy maskSort = SortBy.Red;
                if (fractionTimePart % 3 == 0)
                    maskSort = SortBy.Lightness;
                if (fractionTimePart % 3 == 1)
                    maskSort = SortBy.Green;
                if (fractionTimePart % 3 == 2)
                    maskSort = SortBy.Luminance;
                sorting.MaskFilter = maskSort;
            }
            else
            {
                sorting.MaskFilter = currentSettings.MaskFilter;
            }
            sorting.IsHorizontal = currentSettings.IsHorizontal;

            return sorting;
        }
        public static ColorEditingSettings GetColorSettins(List<KeyPoint> keyPoints, double time, ColorEditingSettings currentSettings)
        {
            var color = new ColorEditingSettings();

            color.RGBCurveValue[0] = GetValue(keyPoints, time, Fields.RGBCurvePoint1) ?? currentSettings.RGBCurveValue[0];
            color.RGBCurveValue[1] = GetValue(keyPoints, time, Fields.RGBCurvePoint2) ?? currentSettings.RGBCurveValue[1];
            color.RGBCurveValue[2] = GetValue(keyPoints, time, Fields.RGBCurvePoint3) ?? currentSettings.RGBCurveValue[2];
            color.RGBCurveValue[3] = GetValue(keyPoints, time, Fields.RGBCurvePoint4) ?? currentSettings.RGBCurveValue[3];
            color.RGBCurveValue[4] = GetValue(keyPoints, time, Fields.RGBCurvePoint5) ?? currentSettings.RGBCurveValue[4];

            color.GreenCurveValue[0] = GetValue(keyPoints, time, Fields.GreenCurvePoint1) ?? currentSettings.GreenCurveValue[0];
            color.GreenCurveValue[1] = GetValue(keyPoints, time, Fields.GreenCurvePoint2) ?? currentSettings.GreenCurveValue[1];
            color.GreenCurveValue[2] = GetValue(keyPoints, time, Fields.GreenCurvePoint3) ?? currentSettings.GreenCurveValue[2];
            color.GreenCurveValue[3] = GetValue(keyPoints, time, Fields.GreenCurvePoint4) ?? currentSettings.GreenCurveValue[3];
            color.GreenCurveValue[4] = GetValue(keyPoints, time, Fields.GreenCurvePoint5) ?? currentSettings.GreenCurveValue[4];

            color.BlueCurveValue[0] = GetValue(keyPoints, time, Fields.BlueCurvePoint1) ?? currentSettings.BlueCurveValue[0];
            color.BlueCurveValue[1] = GetValue(keyPoints, time, Fields.BlueCurvePoint2) ?? currentSettings.BlueCurveValue[1];
            color.BlueCurveValue[2] = GetValue(keyPoints, time, Fields.BlueCurvePoint3) ?? currentSettings.BlueCurveValue[2];
            color.BlueCurveValue[3] = GetValue(keyPoints, time, Fields.BlueCurvePoint4) ?? currentSettings.BlueCurveValue[3];
            color.BlueCurveValue[4] = GetValue(keyPoints, time, Fields.BlueCurvePoint5) ?? currentSettings.BlueCurveValue[4];

            color.RedCurveValue[0] = GetValue(keyPoints, time, Fields.RedCurvePoint1) ?? currentSettings.RedCurveValue[0];
            color.RedCurveValue[1] = GetValue(keyPoints, time, Fields.RedCurvePoint2) ?? currentSettings.RedCurveValue[1];
            color.RedCurveValue[2] = GetValue(keyPoints, time, Fields.RedCurvePoint3) ?? currentSettings.RedCurveValue[2];
            color.RedCurveValue[3] = GetValue(keyPoints, time, Fields.RedCurvePoint4) ?? currentSettings.RedCurveValue[3];
            color.RedCurveValue[4] = GetValue(keyPoints, time, Fields.RedCurvePoint5) ?? currentSettings.RedCurveValue[4];

            color.Brightness = (float)(GetValue(keyPoints, time, Fields.Brightness) ?? currentSettings.Brightness);
            color.Contrast = (float)(GetValue(keyPoints, time, Fields.Contrast) ?? currentSettings.Contrast);
            color.Saturation = (float)(GetValue(keyPoints, time, Fields.Saturation) ?? currentSettings.Saturation);
            color.Hue = (float)(GetValue(keyPoints, time, Fields.Hue) ?? currentSettings.Hue);
            color.Invert = (float)(GetValue(keyPoints, time, Fields.Invert) ?? currentSettings.Invert);
            color.Fade = (float)(GetValue(keyPoints, time, Fields.Fade) ?? currentSettings.Fade);
            color.Temperature = (float)(GetValue(keyPoints, time, Fields.Temperature) ?? currentSettings.Temperature);
            color.Sharpen = (float)(GetValue(keyPoints, time, Fields.Sharpen) ?? currentSettings.Sharpen);

            return color;
        }

        private static double? GetValue(List<KeyPoint> keyPoints, double time, Fields name)
        {
            double? interpolatedValue = null;
            var prevKey = keyPoints.Where(x => x.Time <= time && x.FieldName == name)
                .OrderByDescending(x => x.Time)
                .FirstOrDefault();
            var nextKey = keyPoints.Where(x => x.Time > time && x.FieldName == name)
                .OrderBy(i => i.Time)
                .FirstOrDefault();

            
            var realTime = (int)Math.Floor(time) + (time - (int)Math.Floor(time)) / 0.24;

            if (prevKey != null && nextKey != null)
            {
                var realTimePrev = (int)Math.Floor(prevKey.Time) + (prevKey.Time - (int)Math.Floor(prevKey.Time)) / 0.24;
                var realTimeNext = (int)Math.Floor(nextKey.Time) + (nextKey.Time - (int)Math.Floor(nextKey.Time)) / 0.24;

                interpolatedValue = prevKey.Value +
                (realTime - realTimePrev) / (realTimeNext - realTimePrev) * (nextKey.Value - prevKey.Value);
            }
            else
            {
                if (prevKey != null)
                {
                    interpolatedValue = prevKey.Value;
                }
                if (nextKey != null)
                {
                    interpolatedValue = nextKey.Value;
                }
            }
            return interpolatedValue;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Nika_Vizard_Station.MainWindow;

namespace Nika_Vizard_Station
{
    public static class MaskHelper
    {
        public static bool[][] GenerateMask(PixelInformation[][] currentImage, PixelSortingSettings settings)
        {

            Func<PixelInformation, bool> selector = settings.MaskFilter switch
            {
                SortBy.Blue => x => x.Blue >= settings.MaskStart && x.Blue <= settings.MaskEnd,
                SortBy.Green => x => x.Green >= settings.MaskStart && x.Green <= settings.MaskEnd,
                SortBy.Red => x => x.Red >= settings.MaskStart && x.Red <= settings.MaskEnd,
                SortBy.Alpha => x => x.Alpha >= settings.MaskStart && x.Alpha <= settings.MaskEnd,
                SortBy.Hue => x => x.Hue >= settings.MaskStart && x.Hue <= settings.MaskEnd,
                SortBy.Lightness => x => x.Lightness >= settings.MaskStart && x.Lightness <= settings.MaskEnd,
                SortBy.Saturation => x => x.Saturation >= settings.MaskStart && x.Saturation <= settings.MaskEnd,
                SortBy.Luminance => x => x.Luminance >= settings.MaskStart && x.Luminance <= settings.MaskEnd,
                _ => throw new ArgumentOutOfRangeException(nameof(settings.MaskFilter), settings.MaskFilter, null)
            };

            var width = currentImage.Length;
            var height = currentImage.FirstOrDefault()?.Length ?? 0;
            var mask = new bool[width][];

            for (int y = 0; y < width; y++)
            {
                mask[y] = currentImage[y].Select(selector).ToArray();
            }
            return mask;
        }

        public static PixelInformation[][] CreateMaskImage(bool[][] mask)
        {
            var width = mask.Length;
            var height = mask.FirstOrDefault()?.Length ?? 0;
            var image = new PixelInformation[width][];

            for (int y = 0; y < width; y++)
            {
                image[y] = mask[y].Select(x =>
                {
                    return new PixelInformation
                    {
                        Red = x ? (byte)255 : (byte)0,
                        Blue = x ? (byte)255 : (byte)0,
                        Green = x ? (byte)255 : (byte)0,
                        Alpha = 255,
                        Hue = x ? (byte)0 : (byte)0,
                        Lightness = x ? (byte)100 : (byte)0,
                        Saturation = x ? (byte)0 : (byte)0
                    };
                }).ToArray();
            }
            return image;
        }
    }
}

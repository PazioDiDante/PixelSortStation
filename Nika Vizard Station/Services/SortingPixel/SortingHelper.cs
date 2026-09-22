using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Nika_Vizard_Station.MainWindow;

namespace Nika_Vizard_Station
{
    public static class SortingHelper
    {
        public static PixelInformation[][] SortImage(PixelInformation[][] currentImage, PixelSortingSettings settings)
        {

            Func<PixelInformation, byte> selector = settings.SortFilter switch
            {
                SortBy.Blue => x => x.Blue,
                SortBy.Green => x => x.Green,
                SortBy.Red => x => x.Red,
                SortBy.Alpha => x => x.Alpha,
                SortBy.Hue => x => x.Hue,
                SortBy.Lightness => x => x.Lightness,
                SortBy.Saturation => x => x.Saturation,
                SortBy.Luminance => x => x.Luminance,
                _ => throw new ArgumentOutOfRangeException(nameof(settings.SortFilter), settings.SortFilter, null)
            };

            var width = currentImage.Length;
            var height = currentImage.FirstOrDefault()?.Length ?? 0;

            PixelInformation[][] result;
            bool[][] maskArr;
            if (settings.IsHorizontal)
            {
                maskArr = settings.Mask;
                result = new PixelInformation[width][];
                for (int y = 0; y < width; y++)
                {
                    result[y] = new PixelInformation[height];
                    for (int x = 0; x < height; x++)
                    {
                        result[y][x] = currentImage[y][x];
                    }
                }
            }
            else
            {
                maskArr = new bool[height][];
                result = new PixelInformation[height][];
                for (int y = 0; y < height; y++)
                {
                    maskArr[y] = new bool[width];
                    result[y] = new PixelInformation[width];
                    for (int x = 0; x < width; x++)
                    {
                        maskArr[y][x] = settings.Mask[x][y];
                        result[y][x] = currentImage[x][y];
                    }
                }


            }

            int alligment;
            if (settings.IsHorizontal)
            {
                alligment = width;
            }
            else
            {
                alligment = height;
            }
            for (int y = 0; y < alligment; y++)
            {
                var mask = maskArr[y];
                int zoneCounter = 0;
                var wasMaskZone = false;
                result[y] = result[y]
                    .Select((x, i) => new { Pixel = x, Index = i })
                    .GroupBy(x =>
                    {
                        var maskPixel = mask[x.Index];
                        var isMaskZone = maskPixel;
                        if (wasMaskZone != isMaskZone)
                        {
                            zoneCounter++;
                            wasMaskZone = isMaskZone;
                        }
                        return zoneCounter;
                    })
                    .SelectMany(x => x.Key % 2 == 0
                        ? x.Select(j => j.Pixel)
                        : x.Select(j => j.Pixel).OrderBy(selector))
                    .ToArray();
            }
            return result;
        }
    }

    public enum SortBy
    {
        Blue,
        Green,
        Red,
        Alpha,
        Hue,
        Lightness,
        Saturation,
        Luminance
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Nika_Vizard_Station.MainWindow;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using System.IO;
using System.Windows.Ink;

namespace Nika_Vizard_Station
{
    public static class BitmapHelper
    {
        public static PixelInformation[][] CopyPixels(BitmapSource source, int stride, int offset)
        {
            var height = source.PixelHeight;
            var width = source.PixelWidth;
            var pixelBytes = new byte[height * width * 4];
            source.CopyPixels(pixelBytes, stride, 0);
            int y0 = offset / width;
            int x0 = offset - width * y0;
            var result = new PixelInformation[height][];

            for (int y = 0; y < height; y++)
            {
                result[y] = new PixelInformation[width];
                for (int x = 0; x < width; x++)
                {
                    byte blue = pixelBytes[(y * width + x) * 4 + 0];
                    byte green = pixelBytes[(y * width + x) * 4 + 1];
                    byte red = pixelBytes[(y * width + x) * 4 + 2];
                    byte alpha = pixelBytes[(y * width + x) * 4 + 3];
                    result[y + y0][x +x0] = ColorService.FromRGB(blue, green, red);

                    //pixels[x + x0, y + y0] = new PixelColor
                    //{
                    //    Blue = pixelBytes[(y * width + x) * 4 + 0],
                    //    Green = pixelBytes[(y * width + x) * 4 + 1],
                    //    Red = pixelBytes[(y * width + x) * 4 + 2],
                    //    Alpha = pixelBytes[(y * width + x) * 4 + 3],
                    //};
                }
            }
            return result;
        }

        public static byte[] GetByteArray(this PixelInformation[][] pixels, bool isHorizontal)
        {
            var width = pixels.Length;
            var height = pixels.FirstOrDefault()?.Length ?? 0;
            var pixelBytes = new byte[height * width * 4];

            if (isHorizontal)
            {
                for (int y = 0; y < width; y++)
                    for (int x = 0; x < height; x++)
                    {
                        pixelBytes[(y * height + x) * 4 + 0] = pixels[y][x].Blue;
                        pixelBytes[(y * height + x) * 4 + 1] = pixels[y][x].Green;
                        pixelBytes[(y * height + x) * 4 + 2] = pixels[y][x].Red;
                        pixelBytes[(y * height + x) * 4 + 3] = pixels[y][x].Alpha;
                    }
            }
            else
            {
                for (int y = 0; y < width; y++)
                    for (int x = 0; x < height; x++)
                    {
                        pixelBytes[(x * width + y) * 4 + 0] = pixels[y][x].Blue;
                        pixelBytes[(x * width + y) * 4 + 1] = pixels[y][x].Green;
                        pixelBytes[(x * width + y) * 4 + 2] = pixels[y][x].Red;
                        pixelBytes[(x * width + y) * 4 + 3] = pixels[y][x].Alpha;
                    }
            }
            return pixelBytes;
        }


        public static BitmapSource FromArray(byte[] data, int w, int h, int ch)
        {
            PixelFormat format = PixelFormats.Default;

            if (ch == 1) format = PixelFormats.Gray8; //grey scale image 0-255
            if (ch == 3) format = PixelFormats.Bgr24; //RGB
            if (ch == 4) format = PixelFormats.Bgr32; //RGB + alpha


            WriteableBitmap wbm = new WriteableBitmap(w, h, 96, 96, format, null);
            wbm.WritePixels(new Int32Rect(0, 0, w, h), data, ch * w, 0);

            return wbm;
        }

        [DllImport("gdi32.dll", EntryPoint = "DeleteObject")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool DeleteObject([In] IntPtr hObject);

        public static ImageSource ImageSourceFromBitmap(Bitmap bmp)
        {
            var handle = bmp.GetHbitmap();
            try
            {
                return Imaging.CreateBitmapSourceFromHBitmap(handle, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            }
            finally { DeleteObject(handle); }
        }

        public static Bitmap CreateBitmapFromByteArray(byte[] byteArray, int width, int height)
        {
            Bitmap bitmap = new Bitmap(width, height, width * 4,
                System.Drawing.Imaging.PixelFormat.Format32bppArgb,
                Marshal.UnsafeAddrOfPinnedArrayElement(byteArray, 0));
            return bitmap;
        }
    }
}

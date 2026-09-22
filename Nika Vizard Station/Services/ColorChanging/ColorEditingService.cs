using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Runtime.InteropServices;
using Color = System.Drawing.Color;

namespace Nika_Vizard_Station.Services.ColorChanging
{
    public static class ColorEditingService
    {
        public static Bitmap AdjustColorMatrix(Bitmap origImage, float brightness, float contrast, float saturation, float hue, float invert, float fade, float temperature)
        {
            float[][] origMatrix ={

                  new float[] { 1, 0, 0, 0, 0 },
                  new float[] { 0, 1, 0, 0, 0 },
                  new float[] { 0, 0, 1, 0, 0 },
                  new float[] { 0, 0, 0, 1, 0 },
                  new float[] { 0, 0, 0, 0, 1 }
            };

            //Saturation Contrast Brightness
            if (brightness != 0 || contrast != 1 || saturation != 1)
            {
                float t = (1 - contrast) / 2;

                float sr = (1 - saturation) * 0.3086f;
                float sg = (1 - saturation) * 0.6094f;
                float sb = (1 - saturation) * 0.082f;

                float[][] colorMatrixElements = {
                    new float[] {contrast*(sr+saturation), contrast*sr, contrast*sr, 0, 0},
                    new float[] { contrast*sg, contrast*(sg+saturation), contrast * sg, 0, 0},
                    new float[] { contrast*sb, contrast * sb, contrast * (sb+saturation), 0, 0},
                    new float[] {0, 0, 0, 1, 0},
                    new float[] {brightness +t, brightness +t, brightness + t, 0, 1}
                };
                origMatrix = MultiplyMatrices(origMatrix, colorMatrixElements);
            }

            //HUE
            if (hue != 0)
            {
                var theta = hue / 360 * 2 * Math.PI;
                var c = Math.Cos(theta);
                var s = Math.Sin(theta);

                var A00 = 0.213 + 0.787 * c - 0.213 * s;
                var A01 = 0.213 - 0.213 * c + 0.413 * s;
                var A02 = 0.213 - 0.213 * c - 0.787 * s;

                var A10 = 0.715 - 0.715 * c - 0.715 * s;
                var A11 = 0.715 + 0.285 * c + 0.140 * s;
                var A12 = 0.715 - 0.715 * c + 0.715 * s;

                var A20 = 0.072 - 0.072 * c + 0.928 * s;
                var A21 = 0.072 - 0.072 * c - 0.283 * s;
                var A22 = 0.072 + 0.928 * c + 0.072 * s;

                float[][] hueMatrix ={
                      new float[] { (float)A00, (float)A01, (float)A02, 0, 0 },
                      new float[] { (float)A10, (float)A11, (float)A12, 0, 0 },
                      new float[]{ (float)A20, (float)A21, (float)A22, 0, 0},
                      new float[]{ 0, 0, 0, 1, 0},
                      new float[] { 0, 0, 0, 0, 1 }
                };
                origMatrix = MultiplyMatrices(origMatrix, hueMatrix);
            }

            //Invert
            if (invert != 0)
            {
                float[][] invertMatrix ={
                      new float[] { 1-(2*invert), 0, 0, 0, 0 },
                      new float[] { 0, 1 - (2 * invert), 0, 0, 0 },
                      new float[] { 0, 0, 1 - (2 * invert), 0, 0 },
                      new float[] { 0, 0, 0, 1, 0 },
                      new float[] { invert, invert, invert, 0, 1 }
                };
                origMatrix = MultiplyMatrices(origMatrix, invertMatrix);
            }

            //Fade
            if (fade != 0)
            {
                float[][] fadeMatrix ={

                      new float[] { 1-(0.607f*fade), 0.349f * fade, 0.272f * fade, 0, 0 },
                      new float[] { 0.769f*fade, 1 - (0.314f * fade), 0.534f * fade, 0, 0 },
                      new float[] { 0.189f * fade, 0.168f * fade, 1 - (0.869f * fade), 0, 0 },
                      new float[] { 0, 0, 0, 1, 0 },
                      new float[] { 0, 0, 0, 0, 1 }
                };
                origMatrix = MultiplyMatrices(origMatrix, fadeMatrix);
            }

            //Temperature
            if (temperature != 0)
            {
                float redMultiplier = 1 + 0.5f * temperature;
                float blueMultiplier = 1 - 0.5f * temperature;

                float[][] whiteBalanceMatrix = new float[][]
                {
                    new float[] { redMultiplier, 0, 0, 0, 0 },
                    new float[] { 0, 1, 0, 0, 0 },
                    new float[] { 0, 0, blueMultiplier, 0, 0 },
                    new float[] { 0, 0, 0, 1, 0 },
                    new float[] { 0, 0, 0, 0, 1 }
                };

                origMatrix = MultiplyMatrices(origMatrix, whiteBalanceMatrix);
            }


            ColorMatrix colorMatrix = new ColorMatrix(origMatrix);
            ImageAttributes attributes = new ImageAttributes();
            attributes.SetColorMatrix(colorMatrix);

            var image = new Bitmap(origImage);
            Graphics g = Graphics.FromImage(image);
            g.DrawImage(image, new Rectangle(0, 0, image.Width, image.Height),
                0, 0, image.Width, image.Height,
                GraphicsUnit.Pixel, attributes);
            g.Dispose();

            return image;
        }

        private static float[][] MultiplyMatrices(float[][] matrix1, float[][] matrix2)
        {
            int rows1 = matrix1.Length;
            int cols1 = matrix1[0].Length;
            int rows2 = matrix2.Length;
            int cols2 = matrix2[0].Length;

            if (cols1 != rows2)
            {
                throw new ArgumentException("Number of columns in the first matrix must be equal to the number of rows in the second matrix.");
            }

            float[][] result = new float[rows1][];

            for (int i = 0; i < rows1; i++)
            {
                result[i] = new float[cols2];
                for (int j = 0; j < cols2; j++)
                {
                    for (int k = 0; k < cols1; k++)
                    {
                        result[i][j] += matrix1[i][k] * matrix2[k][j];
                    }
                }
            }

            return result;
        }

        public static byte[] ApplySharpen(byte[] imageBytes, int width, int height, double sharpen)
        {
            double[,] filter = {
                {-1, -1, -1},
                {-1,  sharpen, -1},
                {-1, -1, -1}
            };

            double factor = 1;
            double bias = 0;

            byte[] currentBytes = new byte[imageBytes.Length];
            Array.Copy(imageBytes, currentBytes, imageBytes.Length);

            byte[] resultBytes = new byte[imageBytes.Length];

            for (int x = 1; x < width - 1; x++)
            {
                for (int y = 1; y < height - 1; y++)
                {
                    double red = 0.0, green = 0.0, blue = 0.0;

                    for (int filterX = 0; filterX < 3; filterX++)
                    {
                        for (int filterY = 0; filterY < 3; filterY++)
                        {
                            int imageX = (x - 1 + filterX);
                            int imageY = (y - 1 + filterY);
                            int index = (imageY * width + imageX) * 4;

                            red += currentBytes[index + 2] * filter[filterX, filterY];
                            green += currentBytes[index + 1] * filter[filterX, filterY];
                            blue += currentBytes[index] * filter[filterX, filterY];
                        }
                    }

                    int r = Math.Min(Math.Max((int)(factor * red + bias), 0), 255);
                    int g = Math.Min(Math.Max((int)(factor * green + bias), 0), 255);
                    int b = Math.Min(Math.Max((int)(factor * blue + bias), 0), 255);

                    int pixelIndex = (y * width + x) * 4;
                    resultBytes[pixelIndex + 2] = (byte)r;
                    resultBytes[pixelIndex + 1] = (byte)g;
                    resultBytes[pixelIndex] = (byte)b;
                    resultBytes[pixelIndex + 3] = 255;
                }
            }

            return resultBytes;
        }

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Nika_Vizard_Station.MainWindow;

namespace Nika_Vizard_Station
{
    static class ColorService
    {
        public static PixelInformation FromRGB(byte red, byte green, byte blue)
        {
            float _R = (red / 255f);
            float _G = (green / 255f);
            float _B = (blue / 255f);

            float _Min = Math.Min(Math.Min(_R, _G), _B);
            float _Max = Math.Max(Math.Max(_R, _G), _B);
            float _Delta = _Max - _Min;

            float H = 0;
            float S = 0;
            float L = (float)((_Max + _Min) / 2.0f);

            double Rl;
            double Gl;
            double Bl;

            if (_R <= 0.03928)
            {
                Rl = _R / 12.92f;
            }
            else
            {
                Rl = Math.Pow((_R + 0.055f) / 1.055f, 2.4);
            }
            if (_G <= 0.03928)
            {
                Gl = _G / 12.92f;
            }
            else
            {
                Gl = Math.Pow((_G + 0.055f) / 1.055f, 2.4);
            }
            if (_B <= 0.03928)
            {
                Bl = _B / 12.92f;
            }
            else
            {
                Bl = Math.Pow((_B + 0.055f) / 1.055f, 2.4);
            }

            if (_Delta != 0)
            {
                if (L < 0.5f)
                {
                    S = (float)(_Delta / (_Max + _Min));
                }
                else
                {
                    S = (float)(_Delta / (2.0f - _Max - _Min));
                }


                if (_R == _Max)
                {
                    H = (_G - _B) / _Delta;
                }
                else if (_G == _Max)
                {
                    H = 2f + (_B - _R) / _Delta;
                }
                else if (_B == _Max)
                {
                    H = 4f + (_R - _G) / _Delta;
                }
                H = H * 60f;
                if (H < 0) H += 360;
            }

            return new PixelInformation
            {
                Blue = red,
                Green = green,
                Red = blue,
                Alpha = 255,
                Hue = (byte)Math.Round(H),
                Saturation = (byte)Math.Round(S * 100),
                Lightness = (byte)Math.Round(L * 100),
                Luminance = (byte)Math.Round((0.2126 * Rl + 0.7152 * Gl + 0.0722 * Bl)*100)
            };
        }
    }
}

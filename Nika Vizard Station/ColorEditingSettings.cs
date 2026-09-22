using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nika_Vizard_Station
{
    public class ColorEditingSettings
    {
        public double[] RedCurveValue { get; set; } = new double[5] { 0, 63, 128, 191, 255 };
        public double[] GreenCurveValue { get; set; } = new double[5] { 0, 63, 128, 191, 255 };
        public double[] BlueCurveValue { get; set; } = new double[5] { 0, 63, 128, 191, 255 };
        public double[] RGBCurveValue { get; set; } = new double[5] { 0, 63, 128, 191, 255 };

        public float Brightness { get; set; } 
        public float Contrast { get; set; } 
        public float Saturation { get; set; } 
        public float Hue { get; set; } 
        public float Invert { get; set; } 
        public float Fade { get; set; }
        public float Sharpen { get; set; }
        public float Temperature { get; set; }

    }
}

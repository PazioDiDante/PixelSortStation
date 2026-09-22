using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nika_Vizard_Station
{
    public class KeyPoint
    {
        public Fields FieldName { get; set; }
        public double Time { get; set; }
        public double Value { get; set; }

        public KeyPoint(double t, double v, Fields name)
        {
            FieldName = name;
            Time = t;
            Value = v;
        }
    }



    public enum Fields
    {
        RGBCurvePoint1,
        RGBCurvePoint2,
        RGBCurvePoint3,
        RGBCurvePoint4,
        RGBCurvePoint5,
        RedCurvePoint1,
        RedCurvePoint2,
        RedCurvePoint3,
        RedCurvePoint4,
        RedCurvePoint5,
        BlueCurvePoint1,
        BlueCurvePoint2,
        BlueCurvePoint3,
        BlueCurvePoint4,
        BlueCurvePoint5,
        GreenCurvePoint1,
        GreenCurvePoint2,
        GreenCurvePoint3,
        GreenCurvePoint4,
        GreenCurvePoint5,
        Brightness,
        Contrast,
        Saturation,
        Hue,
        Invert,
        Fade,
        Temperature,
        Sharpen,
        Sorting,
        Mask,
        MaskValue1,
        MaskValue2,
    }

}

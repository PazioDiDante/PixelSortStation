using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Diagnostics;
using static Nika_Vizard_Station.MainWindow;
using System.Windows.Controls;
using System.Windows.Media.Media3D;

namespace Nika_Vizard_Station
{
    public static class CurveHelper
    {

        public static byte[] ChangeImage(byte[] image, List<Point> points)
        {
            var preview = new byte[image.Length];

            var pointArr = new double[256];
            foreach (var item in points)
            {
                pointArr[points.IndexOf(item)] = item.Y;
            }

            for (int i = 0; i < image.Length; i++)
            {
                if ((i + 1) % 4 != 0)
                {
                    preview[i] = (byte)(255 - pointArr[image[i]]);
                }
                else
                {
                    preview[i] = image[i];
                }
            }
            return preview;
        }

        public static byte[] ChangeImageSeperatly(byte[] image, List<Point> pointsRed, List<Point> pointsGreen, List<Point> pointsBlue)
        {
            var preview = new byte[image.Length];

            var pointRedArr = new double[256];
            var pointGreenArr = new double[256];
            var pointBlueArr = new double[256];
            foreach (var item in pointsRed)
            {
                pointRedArr[pointsRed.IndexOf(item)] = item.Y;
            }
            foreach (var item in pointsGreen)
            {
                pointGreenArr[pointsGreen.IndexOf(item)] = item.Y;
            }
            foreach (var item in pointsBlue)
            {
                pointBlueArr[pointsBlue.IndexOf(item)] = item.Y;
            }


            for (int i = 0; i < image.Length; i++)
            {
                switch ((i + 1) % 4)
                {
                    case 0:
                        preview[i] = image[i];
                        break;

                    case 1:
                        preview[i] = (byte)(255 - pointBlueArr[image[i]]);
                        break;

                    case 2:
                        preview[i] = (byte)(255 - pointGreenArr[image[i]]);
                        break;

                    case 3:
                        preview[i] = (byte)(255 - pointRedArr[image[i]]);
                        break;
                }
            }
            return preview;
        }

        public static List<Point> GetInitialPoints()
        {
            List<Point> points = new List<Point>();
            for (int i = 0; i < 256; i++)
            {
                points.Add(new Point(i, 255 - i));
            }
            return points;
        }

        #region BezierFromPath
        public static List<Point> SetPathData(List<Point> points)
        {

            var myPathFigure = new PathFigure { StartPoint = points.FirstOrDefault() };


            var myPathSegmentCollection = new PathSegmentCollection();

            var beizerSegments = InterpolatePointWithBeizerCurves(points, false);

            if (beizerSegments == null || beizerSegments.Count < 1)
            {
                //Add a line segment <this is generic for more than one line>
                foreach (var point in points.GetRange(1, points.Count - 1))
                {

                    var myLineSegment = new LineSegment { Point = point };
                    myPathSegmentCollection.Add(myLineSegment);
                }
            }
            else
            {
                foreach (var beizerCurveSegment in beizerSegments)
                {
                    var segment = new BezierSegment
                    {
                        Point1 = beizerCurveSegment.FirstControlPoint,
                        Point2 = beizerCurveSegment.SecondControlPoint,
                        Point3 = beizerCurveSegment.EndPoint
                    };
                    myPathSegmentCollection.Add(segment);
                }
            }


            myPathFigure.Segments = myPathSegmentCollection;

            var myPathFigureCollection = new PathFigureCollection { myPathFigure };

            var myPathGeometry = new PathGeometry { Figures = myPathFigureCollection };


            PathGeometry g = myPathGeometry.GetFlattenedPathGeometry();
            List<Point> pointTest = new List<Point>();
            foreach (var f in g.Figures)
                foreach (var s in f.Segments)
                    if (s is PolyLineSegment)
                        foreach (var pt in ((PolyLineSegment)s).Points)
                            pointTest.Add(pt);

            var interpolatedPoints = InterpolatePoints(pointTest, 255);
            return interpolatedPoints;
        }

        public static List<BeizerCurveSegment> InterpolatePointWithBeizerCurves(List<Point> points, bool isClosedCurve)
        {
            if (points.Count < 3)
                return null;
            var toRet = new List<BeizerCurveSegment>();

            //if is close curve then add the first point at the end
            if (isClosedCurve)
                points.Add(points.First());

            for (int i = 0; i < points.Count - 1; i++)   //iterate for points but the last one
            {
                // Assume we need to calculate the control
                // points between (x1,y1) and (x2,y2).
                // Then x0,y0 - the previous vertex,
                //      x3,y3 - the next one.
                double x1 = points[i].X;
                double y1 = points[i].Y;

                double x2 = points[i + 1].X;
                double y2 = points[i + 1].Y;

                double x0;
                double y0;

                if (i == 0) //if is first point
                {
                    if (isClosedCurve)
                    {
                        var previousPoint = points[points.Count - 2];    //last Point, but one (due inserted the first at the end)
                        x0 = previousPoint.X;
                        y0 = previousPoint.Y;
                    }
                    else    //Get some previouse point
                    {
                        var previousPoint = points[i];  //if is the first point the previous one will be it self
                        x0 = previousPoint.X;
                        y0 = previousPoint.Y;
                    }
                }
                else
                {
                    x0 = points[i - 1].X;   //Previous Point
                    y0 = points[i - 1].Y;
                }

                double x3, y3;

                if (i == points.Count - 2)    //if is the last point
                {
                    if (isClosedCurve)
                    {
                        var nextPoint = points[1];  //second Point(due inserted the first at the end)
                        x3 = nextPoint.X;
                        y3 = nextPoint.Y;
                    }
                    else    //Get some next point
                    {
                        var nextPoint = points[i + 1];  //if is the last point the next point will be the last one
                        x3 = nextPoint.X;
                        y3 = nextPoint.Y;
                    }
                }
                else
                {
                    x3 = points[i + 2].X;   //Next Point
                    y3 = points[i + 2].Y;
                }

                double xc1 = (x0 + x1) / 2.0;
                double yc1 = (y0 + y1) / 2.0;
                double xc2 = (x1 + x2) / 2.0;
                double yc2 = (y1 + y2) / 2.0;
                double xc3 = (x2 + x3) / 2.0;
                double yc3 = (y2 + y3) / 2.0;

                double len1 = Math.Sqrt((x1 - x0) * (x1 - x0) + (y1 - y0) * (y1 - y0));
                double len2 = Math.Sqrt((x2 - x1) * (x2 - x1) + (y2 - y1) * (y2 - y1));
                double len3 = Math.Sqrt((x3 - x2) * (x3 - x2) + (y3 - y2) * (y3 - y2));

                double k1 = len1 / (len1 + len2);
                double k2 = len2 / (len2 + len3);

                double xm1 = xc1 + (xc2 - xc1) * k1;
                double ym1 = yc1 + (yc2 - yc1) * k1;

                double xm2 = xc2 + (xc3 - xc2) * k2;
                double ym2 = yc2 + (yc3 - yc2) * k2;

                const double smoothValue = 0.8;
                // Resulting control points. Here smooth_value is mentioned
                // above coefficient K whose value should be in range [0...1].
                double ctrl1_x = xm1 + (xc2 - xm1) * smoothValue + x1 - xm1;
                double ctrl1_y = ym1 + (yc2 - ym1) * smoothValue + y1 - ym1;

                double ctrl2_x = xm2 + (xc2 - xm2) * smoothValue + x2 - xm2;
                double ctrl2_y = ym2 + (yc2 - ym2) * smoothValue + y2 - ym2;
                toRet.Add(new BeizerCurveSegment
                {
                    StartPoint = new Point(x1, y1),
                    EndPoint = new Point(x2, y2),
                    FirstControlPoint = i == 0 && !isClosedCurve ? new Point(x1, y1) : new Point(ctrl1_x, ctrl1_y),
                    SecondControlPoint = i == points.Count - 2 && !isClosedCurve ? new Point(x2, y2) : new Point(ctrl2_x, ctrl2_y)
                });
            }

            return toRet;
        }

        public static List<Point> InterpolatePoints(List<Point> originalPoints, int desiredCount)
        {
            List<Point> interpolatedPoints = new List<Point>();

            for (int i = 0; i < originalPoints.Count - 1; i++)
            {
                double xDiff = originalPoints[i + 1].X - originalPoints[i].X;
                double y = originalPoints[i].Y;

                for (int j = (int)originalPoints[i].X; j <= (int)originalPoints[i + 1].X; j++)
                {
                    double t = (j - originalPoints[i].X) / xDiff;

                    interpolatedPoints.Add(new Point(j, y + t * (originalPoints[i + 1].Y - y)));
                }
            }

            return interpolatedPoints.GroupBy(x => x.X).Select(x => x.First()).ToList();
        }

        #endregion
        public class BeizerCurveSegment
        {
            public Point StartPoint { get; set; }
            public Point EndPoint { get; set; }
            public Point FirstControlPoint { get; set; }
            public Point SecondControlPoint { get; set; }
        }

        public enum CurveSortBy
        {
            Blue,
            Green,
            Red,
            All
        }

    }
}

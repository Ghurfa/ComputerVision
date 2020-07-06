using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Text;

namespace CVTracking
{
    public static class Extensions
    {
        public static void Draw(this RotatedRect rect, Mat image, Scalar color, int thickness)
        {
            if (thickness< 0)
            {
                Point[] points = new Point[4];
                Point2f[] points2F = rect.Points();
                for (int i = 0; i < 4; i++)
                {
                    points[i] = (Point)points2F[i];
                }
                Cv2.FillConvexPoly(image, points, color);
            }
            else
            {
                foreach(Point2f point in rect.Points())
                {
                    foreach (Point2f point2 in rect.Points())
                    {
                        Cv2.Line(image, (Point)point, (Point)point2, color, thickness);
                    }
                }
            }
        }
        public static double AngleTo(this Point point1, Point point2)
        {
            Point distDiff = point2 - point1;
            return Math.Atan(((double)distDiff.Y) / ((double)distDiff.X));
        }
    }
}

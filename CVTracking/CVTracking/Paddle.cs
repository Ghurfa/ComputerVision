using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Text;

namespace CVTracking
{
    class Paddle
    {
        private Mat image;
        public RotatedRect Position;
        public RotatedRect LastPosition;
        public double Angle;

        private Point[] points;
        private bool isLeft;
        public Point Velocity => new Point(Position.Center.X - LastPosition.Center.X, Position.Center.Y - LastPosition.Center.Y);
        public Paddle(Mat image)
        {
            LastPosition = new RotatedRect();
            points = new Point[4];
            this.image = image;
        }
        public void Update(RotatedRect position, Ball ball)
        {
            LastPosition = Position;
            Position = position;
            isLeft = Position.Center.X < image.Width / 2;
            computeAngle(isLeft);
            bool hit = false;
            for (int i = 0; i < 3 && !hit; i++)
            {
                Point corner1 = points[i];
                for (int j = i + 1; j < 4; j++)
                {
                    Point corner2 = points[j];
                    double cornerAngle = corner1.AngleTo(corner2);
                    double angleDiff = cornerAngle - Angle;
                    if (angleDiff < -Math.PI) angleDiff += Math.PI;
                    else if (angleDiff > Math.PI) angleDiff -= Math.PI;
                    if (Math.Abs(angleDiff) < 0.1 || Math.Abs(Math.Abs(angleDiff) - Math.PI / 2) < 0.1 || Math.Abs(Math.Abs(angleDiff) - Math.PI) < 0.1)
                    {
                        if(ball.CalculateIntersection(corner1, corner2))
                        {
                            hit = true;
                            break;
                        }
                    }
                }
            }
        }
        private void computeAngle(bool isLeft)
        {
            Point2f[] points2F = Position.Points();
            for (int i = 0; i < 4; i++)
            {
                points[i] = (Point)points2F[i];
            }

            //find closest other corner - direction to it is the paddle orientation
            Point point = points[0];
            double closestDistance = point.DistanceTo(points[1]);
            Point distDiff = point - points[1];
            for (int i = 2; i < 4; i++)
            {
                double distance = point.DistanceTo(points[i]);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    distDiff = point - points[i];
                }
            }
            Angle = Math.Atan(((double)distDiff.Y) / ((double)distDiff.X));
            /*double angle = rect.Angle;
            double angleRads = angle * Math.PI /180;*/
            if (!isLeft) Angle += Math.PI;
        }
        private bool checkIntersection(Ball ball)
        {
            RotatedRect ballHitbox = new RotatedRect(ball.Center, new Size2f(ball.Radius * 2, ball.Radius * 2), (float)Angle);
            Cv2.RotatedRectangleIntersection(Position, ballHitbox, out var output);
            return output.Length > 0;
        }
        public void Draw()
        {
            if (isLeft)
            {
                //Position.Draw(image, Scalar.Red, 1);
                //Cv2.FillConvexPoly(image, points, Scalar.Red);
            }
            else
            {
                //Position.Draw(image, Scalar.Blue, 1);
                //Cv2.FillConvexPoly(image, points, Scalar.Blue);
            }
            float lineLength = 100;
            Cv2.Line(image, (Point)Position.Center, new Point(Position.Center.X + lineLength * Math.Cos(Angle), Position.Center.Y + lineLength * Math.Sin(Angle)), Scalar.AliceBlue);
        }
    }
}

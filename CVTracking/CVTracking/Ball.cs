using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Text;

namespace CVTracking
{
    class Ball
    {
        public Point Center;
        public double Speed;

        private double angle;
        public double Angle
        {
            get { return angle; }
            set
            {
                double val = value;
                double clamped = val % (2 * Math.PI);
                if (clamped < -Math.PI) clamped += 2 * Math.PI;
                else if (clamped > Math.PI) clamped -= 2 * Math.PI;
                angle = clamped;
            }
        }


        public int Radius;

        private Random random;
        private double baseSpeed;

        private Point velocity;

        private Mat image;
        public Ball(Mat image, double speed, int radius)
        {
            this.image = image;
            Center = new Point(image.Width / 2, image.Height / 2);
            Speed = speed;
            Radius = radius;

            random = new Random();
            Angle = (random.NextDouble() - 0.5) * 2 * Math.PI;
            baseSpeed = speed;

            calculateVelocity();
        }
        public void Update()
        {
            Center += velocity;
            /* Corner vertices:
             * 110 38
             * 108 581
             * 1210 52
             * 1191 611
             */
            CalculateIntersection(new Point(110, 38), new Point(1210, 52));
            CalculateIntersection(new Point(1210, 52), new Point(1191, 611));
            CalculateIntersection(new Point(1191, 611), new Point(108, 581));
            CalculateIntersection(new Point(108, 581), new Point(110, 38));
        }

        public bool CalculateIntersection(Point endPoint1, Point endPoint2)
        {
            double lineAngle = endPoint1.AngleTo(endPoint2);
            /*Point2f center = new Point2f((endPoint1.X + endPoint2.X) / 2, (endPoint1.Y + endPoint2.Y) / 2);
            Size2f size;
            if ((lineAngle > Math.PI / 4 && lineAngle < 3 * Math.PI / 4) ||
                (-lineAngle > Math.PI / 4 && -lineAngle < 3 * Math.PI / 4))
            {
                size = new Size2f(1, (float)endPoint1.DistanceTo(endPoint2));
            }
            else
            {
                size = new Size2f((float)endPoint1.DistanceTo(endPoint2), 1);
            }

            RotatedRect lineHitbox = new RotatedRect(center, size, -(float)lineAngle);
            lineHitbox.Draw(image, Scalar.Green, -1);*/
            Cv2.Line(image, endPoint1, endPoint2, Scalar.Red);

            /*Cv2.RotatedRectangleIntersection(lineHitbox, GetHitbox(lineAngle), out var intersection);
            bool intersects = intersection.Length > 0;*/

            if (Intersects(endPoint1, endPoint2))
            {
                Angle = lineAngle - (Angle - lineAngle);
                calculateVelocity();
                Center += velocity;
                return true;
            }
            return false;
        }
        public RotatedRect GetHitbox(double angle)
        {
            return new RotatedRect(Center, new Size2f(Radius * 2, Radius * 2), (float)Angle);
        }
        private void calculateVelocity()
        {
            velocity = new Point(Math.Cos(Angle) * Speed, Math.Sin(Angle) * Speed);
        }
        private bool Intersects(Point point1, Point point2)
        {
            Point d = point1 - point2;
            Point f = point2 - Center;

            double a = d.DotProduct(d);
            double b = 2 * f.DotProduct(d);
            double c = f.DotProduct(f) - Radius * Radius;

            double discriminant = b * b - 4 * a * c;
            if (discriminant < 0)
            {
                // no intersection
                return false;
            }
            else
            {
                // ray didn't totally miss sphere,
                // so there is a solution to
                // the equation.

                discriminant = Math.Sqrt(discriminant);

                // either solution may be on or off the ray so need to test both
                // t1 is always the smaller value, because BOTH discriminant and
                // a are nonnegative.
                double t1 = (-b - discriminant) / (2 * a);
                double t2 = (-b + discriminant) / (2 * a);

                // 3x HIT cases:
                //          -o->             --|-->  |            |  --|->
                // Impale(t1 hit,t2 hit), Poke(t1 hit,t2>1), ExitWound(t1<0, t2 hit), 

                // 3x MISS cases:
                //       ->  o                     o ->              | -> |
                // FallShort (t1>1,t2>1), Past (t1<0,t2<0), CompletelyInside(t1<0, t2>1)

                if (t1 >= 0 && t1 <= 1)
                {
                    // t1 is the intersection, and it's closer than t2
                    // (since t1 uses -b - discriminant)
                    // Impale, Poke
                    return true;
                }

                // here t1 didn't intersect so we are either started
                // inside the sphere or completely past it
                if (t2 >= 0 && t2 <= 1)
                {
                    // ExitWound
                    return true;
                }

                // no intn: FallShort, Past, CompletelyInside
                return false;
            }
        }
    }
}

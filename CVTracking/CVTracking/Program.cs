using FRC.CameraServer;
using FRC.CameraServer.OpenCvSharp;
using OpenCvSharp;
using System;
using System.Diagnostics;
using System.Linq;

namespace CVTracking
{
    class Program
    {
        static void Main(string[] args)
        {
            HttpCamera camera = new HttpCamera("Camera", "http://192.168.1.121:1181/stream.mjpg");
            CvSink sink = new CvSink("Sink");
            sink.Source = camera;

            Cv2.NamedWindow("Display", WindowMode.AutoSize);

            int x = 11;
            int screen = 0;
            Cv2.CreateTrackbar("X", "Display", ref x, 20);
            Cv2.CreateTrackbar("Screen", "Display", ref screen, 2);
            //Cv2.NamedWindow("Display2", WindowMode.AutoSize);


            Mat image = new Mat();
            Mat hsvImage = new Mat();
            Mat paddleMask = new Mat();
            Mat paddleSmooth = new Mat();

            while (sink.GrabFrame(image) == 0) ;

            Ball ball = new Ball(image, 20, 20);
            Paddle leftPaddle = new Paddle(image);
            Paddle rightPaddle = new Paddle(image);

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            while (true)
            {
                if (sink.GrabFrame(image) == 0)
                {
                    continue;
                }

                //Walls
                /*Cv2.Line(image, new Point(110, 38), new Point(1210, 52), Scalar.Red, 2);
                Cv2.Line(image, new Point(1210, 52), new Point(1191, 611), Scalar.Red, 2);
                Cv2.Line(image, new Point(1191, 611), new Point(108, 581), Scalar.Red, 2);
                Cv2.Line(image, new Point(108, 581), new Point(110, 38), Scalar.Red, 2);*/

                //Print stopwatch
                Cv2.PutText(image, stopwatch.ElapsedTicks.ToString(), new Point(0, 20), HersheyFonts.HersheyPlain, 1, Scalar.Red);
                stopwatch.Restart();

                //Paddle filter
                Cv2.CvtColor(image, hsvImage, ColorConversionCodes.BGR2HSV);
                Cv2.InRange(hsvImage, new Scalar(70, 100, 140), new Scalar(100, 255, 255), paddleMask);

                //Smooth paddles
                using Mat structure = Cv2.GetStructuringElement(MorphShapes.Rect, new Size(x + 1, x + 1));
                Cv2.MorphologyEx(paddleMask, paddleSmooth, MorphTypes.Close, structure);

                //Draw paddles
                Cv2.FindContours(paddleSmooth, out var contours, out var _, RetrievalModes.List, ContourApproximationModes.ApproxTC89KCOS);
                var minAreaRects = contours.Where(x => Cv2.ContourArea(x) > 1000).Select(x => Cv2.MinAreaRect(x)).OrderBy(x => x.Center.X);

                leftPaddle.Update(minAreaRects.First(), ball);
                rightPaddle.Update(minAreaRects.Last(), ball);

                leftPaddle.Draw();
                rightPaddle.Draw();

                ball.Update();

                //Draw ball
                Cv2.Circle(image, ball.Center, ball.Radius, Scalar.White, -1);

                if (screen == 0)
                {
                    Cv2.ImShow("Display", image);
                }
                else if (screen == 1)
                {
                    Cv2.ImShow("Display", paddleSmooth);
                }
                else if (screen == 2)
                {
                    Cv2.ImShow("Display", paddleMask);
                }
                Cv2.WaitKey(1);
            }
        }
    }
}

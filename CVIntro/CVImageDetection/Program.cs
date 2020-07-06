using System;
using System.IO;
using System.Linq;
using FRC.CameraServer;
using FRC.CameraServer.OpenCvSharp;
using OpenCvSharp;

namespace CVImageDetection
{
    class Program
    {
        static void Main(string[] args)
        {
            UsbCamera camera = new UsbCamera("Cam", 0);
            string cameraConfigJson = File.ReadAllText(@"\\GMRDC1\Folder Redirection\Lorenzo.Lopez\Documents\Computer Vision Camp\Camera Settings.json");
            camera.SetConfigJson(cameraConfigJson);
            MjpegServer webServer = new MjpegServer("Server", 10700);
            CvSink sink = new CvSink("Sink");
            webServer.Source = camera;
            sink.Source = camera;

            camera.SetExposureAuto();
            camera.SetWhiteBalanceAuto();
            camera.SetResolution(640, 480);

            Cv2.NamedWindow("Display", WindowMode.AutoSize);

            int lastX;
            int lastY;

            Cv2.SetMouseCallback("Display", (@event, x, y, flags, _userdata) =>
            {
                lastX = x;
                lastY = y;
            });

            int hLow = 6;
            int hHigh = 58;
            int sLow = 75;
            int sHigh = 255;
            int vLow = 100;
            int vHigh = 255;
            Cv2.CreateTrackbar("H_Low", "Display", ref hLow, 180);
            Cv2.CreateTrackbar("H_High", "Display", ref hHigh, 180);
            Cv2.CreateTrackbar("S_Low", "Display", ref sLow, 255);
            Cv2.CreateTrackbar("S_High", "Display", ref sHigh, 255);
            Cv2.CreateTrackbar("V_Low", "Display", ref vLow, 255);
            Cv2.CreateTrackbar("V_High", "Display", ref vHigh, 255);

            Mat inputImage = new Mat();
            Mat imageHSV = new Mat();
            Mat imageMask = new Mat();
            Mat finalImage = new Mat();
            Scalar lowScalar = new Scalar();
            Scalar highScalar = new Scalar();
            while (true)
            {
                if (sink.GrabFrame(inputImage) == 0)
                {
                    continue;
                }
                lowScalar.Val0 = hLow;
                lowScalar.Val1 = sLow;
                lowScalar.Val2 = vLow;
                highScalar.Val0 = hHigh;
                highScalar.Val1 = sHigh;
                highScalar.Val2 = vHigh;
                Cv2.CvtColor(inputImage, imageHSV, ColorConversionCodes.BGR2HSV);
                Cv2.InRange(imageHSV, lowScalar, highScalar, imageMask);
                Cv2.FindContours(imageMask, out Point[][] contours, out var hierarchy, RetrievalModes.List, ContourApproximationModes.ApproxSimple);

                var filtered = contours.Select(x => Cv2.ConvexHull(x))
                                        .Where(x => Cv2.ContourArea(x) > 500) //Area filter
                                        .Where(x => 
                                        {
                                            int angle = (int)Cv2.MinAreaRect(x).Angle;
                                            if (angle < -45) angle += 90;
                                            else if (angle > 45) angle -= 90;
                                            return Math.Abs(angle) < 15;
                                        }) //Angle filter
                                        .Where(x =>
                                        {
                                            var boundingRect = Cv2.BoundingRect(x);
                                            return boundingRect.Width > boundingRect.Height;
                                        }); //Orientation filter

                var rightMost = filtered.Select(x => Cv2.BoundingRect(x)).OrderBy(x => x.X).LastOrDefault();
                if(rightMost != default)
                {
                    //60 = measured inches
                    //144 = measured pixels
                    double distance =  60 * 144 / rightMost.Width;
                    //inputImage.PutText(distance.ToString(), new Point(0, 0), HersheyFonts.HersheyPlain, 10, new Scalar(0, 0, 200));
                    Console.WriteLine(distance);
                }

                var boundingRects = contours.Select(x => Cv2.BoundingRect(x));
                inputImage.DrawContours(filtered, -1, new Scalar(0, 200, 0), -1);
                inputImage.Rectangle(rightMost, new Scalar(0, 0, 200), 3);
                //Cv2.GaussianBlur(inputImage, inputImage, new Size(5, 5), 2);
                Cv2.ImShow("Display", inputImage);

                if (Cv2.WaitKey(1) != -1)
                {
                    break;
                }
            }
        }
    }
}

using System;
using System.IO;
using FRC.CameraServer;
using FRC.CameraServer.OpenCvSharp;
using OpenCvSharp;

namespace CVIntro
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

            Mat foreground = Cv2.ImRead(@"C:\Users\Lorenzo.Lopez\Pictures\godzilla 1280x720.jpg");
            //Mat background = Cv2.ImRead(@"C:\Users\Lorenzo.Lopez\Pictures\city 1280x720.jpg");
            //background = background.Resize(new OpenCvSharp.Size(foreground.Width, foreground.Height));

            Mat foregroundHsv = foreground.CvtColor(ColorConversionCodes.BGR2HSV);

            Mat mask = foregroundHsv.InRange(new Scalar(45, 40, 40), new Scalar(64, 255, 255));

            Mat invertedMask = new Mat();
            Cv2.BitwiseNot(mask, invertedMask);

            Mat finalForeground = new Mat();
            foreground.CopyTo(finalForeground, invertedMask);

            //Mat finalBackground = new Mat();
            //background.CopyTo(finalBackground, mask);

            Cv2.NamedWindow("Display", WindowMode.AutoSize);
            
            Cv2.SetMouseCallback("Display", (@event, x, y, flags, _userData) => 
            {
                if (x < 0 || x >= foregroundHsv.Width || y < 0 || y >= foregroundHsv.Height)
                {
                    return;
                }
                Vec3b hsv = foregroundHsv.At<Vec3b>(y, x);

                Console.WriteLine($"H: {hsv.Item0}, S: {hsv.Item1}, V:{hsv.Item2}");
            });

            Mat background = new Mat();
            Mat background2 = new Mat();
            Mat finalBackground = new Mat();
            Mat finalImage = new Mat();
            while (true)
            {
                if(sink.GrabFrame(background) == 0)
                {
                    continue;
                }
                background.CopyTo(finalBackground, mask);
                Cv2.BitwiseOr(finalForeground, finalBackground, finalImage);
                Cv2.Laplacian(finalImage, finalImage, finalImage.Type());
                Cv2.ImShow("Display", finalImage);
                if(Cv2.WaitKey(1) != -1)
                {
                    break;
                }
            }


            /*Mat finalImage = new Mat();
            Cv2.BitwiseOr(finalForeground, finalBackground, finalImage);
            Cv2.ImShow("Display", finalImage);

            Cv2.WaitKey(0);*/
        }
    }
}

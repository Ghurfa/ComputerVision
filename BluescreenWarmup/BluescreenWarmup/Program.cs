using System;
using OpenCvSharp;

namespace BluescreenWarmup
{
    class Program
    {
        static void Main(string[] args)
        {
            string pictures = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
            Mat foreground = Cv2.ImRead(pictures + @"\bluescreen.jpg");
            Mat background = Cv2.ImRead(pictures + @"\forest.jpg");

            Cv2.NamedWindow("Display");
            Cv2.ResizeWindow("Display", foreground.Width, foreground.Height);

            Mat foregroundHsv = foreground.CvtColor(ColorConversionCodes.BGR2HSV);
            Mat mask = foregroundHsv.InRange(new Scalar(90, 90, 80), new Scalar(140, 255, 255));
            Mat invertedMask = new Mat();
            Cv2.BitwiseNot(mask, invertedMask);

            Mat finalForeground = new Mat();
            foreground.CopyTo(finalForeground, invertedMask);

            Mat finalBackground = new Mat();
            background.CopyTo(finalBackground, mask);

            Cv2.SetMouseCallback("Display", (@event, x, y, flags, _userData) => {
                Vec3b color = foreground.At<Vec3b>(y, x);
                Console.WriteLine($"H: {color.Item0}, S: {color.Item1}, V:{color.Item2}");
            });

            Mat finalImage = new Mat();
            Cv2.BitwiseOr(finalForeground, finalBackground, finalImage);

            Cv2.ImShow("Display", finalImage);
            Cv2.WaitKey(0);
        }
    }
}

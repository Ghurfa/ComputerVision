using OpenCvSharp;
using System;

namespace CVLaneDetection
{
    class Program
    {
        static void Main(string[] args)
        {
            Mat image = Cv2.ImRead(@"C:\Users\Lorenzo.Lopez\Pictures\road nz.jpg");
            Mat gray = image.CvtColor(ColorConversionCodes.BGR2GRAY);

            Mat blur = gray.GaussianBlur(new Size(5, 5), 0);
            Mat canny = blur.Canny(50, 150);

            Mat mask = new Mat();
            canny.CopyTo(mask);

            Cv2.BitwiseXor(mask, mask, mask);

            Cv2.FillPoly(mask, new Point[][]{
                new Point[]
                {
                    new Point(mask.Width/2, mask.Height * 21/32),
                    new Point(0, mask.Height),
                    new Point(mask.Width, mask.Height),
                }

            }, Scalar.White);

            Cv2.BitwiseAnd(canny, mask, canny);

            Mat invertedMask = new Mat();
            Cv2.BitwiseNot(mask, invertedMask);

            Mat final = new Mat();
            image.CopyTo(final, invertedMask);
            //canny.CopyTo(final, mask);
            Mat cannyConverted = canny.CvtColor(ColorConversionCodes.GRAY2BGR);
            canny.ConvertTo(cannyConverted, final.Type());
            Cv2.BitwiseOr(final, cannyConverted, final);

            var lines = Cv2.HoughLinesP(canny, 2, Math.PI / 180, 100, 100, 50);

            Cv2.NamedWindow("Display", WindowMode.AutoSize);
            Cv2.NamedWindow("Raw", WindowMode.AutoSize);

            Cv2.ImShow("Display", canny);
            Cv2.ImShow("Raw", final);
            Cv2.WaitKey(0);
        }
    }
}

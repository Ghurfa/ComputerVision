using FRC.CameraServer;
using FRC.CameraServer.OpenCvSharp;
using OpenCvSharp;
using System;
using System.Linq;

namespace CVFeatureDetection
{
    class Program
    {
        static void eyesInFaceDetection(Mat image, Rect[] faces, CascadeClassifier eyeClassifier)
        {
            foreach (var face in faces)
            {
                Cv2.Rectangle(image, face, Scalar.ForestGreen, 1);

                using var faceArea = image[face];
                var faceColor = image[face];
                var eyes = eyeClassifier.DetectMultiScale(faceArea, 1.1, 4);
                foreach (var eye in eyes)
                {
                    Cv2.Rectangle(faceColor, eye, Scalar.Fuchsia, 1);
                }
            }
        }
        static void faceswap(Mat image, Rect[] faces)
        {
            //Store face images before switching
            //Before switching so that you have the original image instead of a swapped one
            Mat[] faceRegions = new Mat[faces.Length];
            for (int i = 0; i < faces.Length; i++)
            {
                Rect face = faces[i];
                faceRegions[i] = new Mat();
                image[face].CopyTo(faceRegions[i]);
                Cv2.Rectangle(image, face, Scalar.Green);
            }

            //Swap around faces
            for (int i = 0; i < faces.Length; i++)
            {
                Rect nextFace = faces[(i + 1) % faces.Length];
                Mat stretched = faceRegions[i].Resize(nextFace.Size);
                image[nextFace] = stretched;
            }
        }
        static void flipFaces(Mat image, Rect[] faces)
        {
            foreach(Rect face in faces)
            {
                Mat faceImage = image[face];
                Cv2.Flip(faceImage, faceImage, FlipMode.X);
            }
        }
        static void Main(string[] args)
        {
            UsbCamera camera = new UsbCamera("Camera", 0);
            //camera.SetResolution(160, 120);

            CvSink sink = new CvSink("Sink");
            sink.Source = camera;

            MjpegServer server = new MjpegServer("Server", 10700);
            server.Source = camera;

            Cv2.NamedWindow("Display", WindowMode.AutoSize);
            int minsize = 2;
            Cv2.CreateTrackbar("Minsize (k)", "Display", ref minsize, 10);

            Mat image = new Mat();
            Mat grayscale = new Mat();

            string documents = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            CascadeClassifier faceClassifier = new CascadeClassifier(documents + @"\Computer Vision Camp\haarcascade_frontalface_default.xml");
            CascadeClassifier eyeClassifier = new CascadeClassifier(documents + @"\Computer Vision Camp\haarcascade_eye_tree_eyeglasses.xml");

            while (true)
            {
                if (sink.GrabFrame(image) == 0) continue;
                Cv2.Flip(image, image, FlipMode.Y);

                Cv2.CvtColor(image, grayscale, ColorConversionCodes.BGR2GRAY);

                //Grab faces - filter by area
                var faces = faceClassifier.DetectMultiScale(grayscale, 1.1, 4).Where(rect => rect.Width * rect.Height > minsize * 1000).ToArray();

                //flipFaces(image, faces);
                faceswap(image, faces);

                Cv2.ImShow("Display", image);
                if (Cv2.WaitKey(1) != -1) break;
            }
        }
    }
}

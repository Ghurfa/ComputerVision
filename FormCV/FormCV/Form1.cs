using FRC.CameraServer;
using FRC.CameraServer.OpenCvSharp;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FormCV
{
    public partial class Form1 : Form
    {
        UsbCamera camera;
        MjpegServer server;
        CvSink sink;
        Mat frame;
        public Form1()
        {
            InitializeComponent();
            camera = new UsbCamera("Camera", 0);
            camera.SetResolution(320, 240);

            string configPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\Computer Vision Camp\Camera Settings.json";
            string cameraConfig = File.ReadAllText(configPath);
            camera.SetConfigJson(cameraConfig);

            sink = new CvSink("Sink");
            sink.Source = camera;

            server = new MjpegServer("Server", 10700);
            server.Source = camera;

            frame = new Mat();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            _ = FrameLoop();
        }

        private async Task<bool> GrabFrame()
        {
            return await Task.Run(() =>
            {
                return sink.GrabFrame(frame) != 0;
            });
        }

        private async Task FrameLoop()
        {
            while(true)
            {
                if (await GrabFrame())
                {
                    pictureBoxIpl1.ImageIpl = frame;
                }
            }
        }
        private void button1_Click(object sender, EventArgs e)
        {
            //pictureBox1 = BitmapConverter.ToBitmap(frame);
        }
    }
}

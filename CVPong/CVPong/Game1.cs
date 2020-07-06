using FRC.CameraServer;
using FRC.CameraServer.OpenCvSharp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using OpenCvSharp;
using System;
using System.IO;
using System.Linq;
using System.Threading;

namespace CVPong
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Game1 : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        UsbCamera camera;
        CvSink sink;
        MjpegServer server;

        Mat grabFrame;
        bool grabbedFrame;
        object frameLock;

        Mat processedFrame;
        bool processed;
        object processedLock;

        Texture2D texture;
        object textureLock;

        Thread grabFrameThread;
        Thread processFrameThread;
        Thread convertFrameThread;

        Ball ball;
        Paddle paddle;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            camera = new UsbCamera("Camera", 0);

            string configPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\Computer Vision Camp\Camera Settings.json";
            string config = File.ReadAllText(configPath);
            camera.SetConfigJson(config);
            //camera.SetResolution(320, 480);

            sink = new CvSink("Sink");
            sink.Source = camera;
            grabFrame = new Mat();

            server = new MjpegServer("Server", 10700);
            server.Source = camera;

            ball = new Ball(new Rectangle(Microsoft.Xna.Framework.Point.Zero, new Microsoft.Xna.Framework.Point(20, 20)), new Microsoft.Xna.Framework.Point(5, 5), new Microsoft.Xna.Framework.Point(640, 480));
            paddle = new Paddle(new Microsoft.Xna.Framework.Point(50, 0), new Microsoft.Xna.Framework.Point(20, 100));

            frameLock = new object();
            textureLock = new object();
            processedLock = new object();
            startGrabFrameLoop();
            startProcessFrameLoop();
            startConvertFrameLoop();
        }
        private void startGrabFrameLoop()
        {
            Mat tempFrame = new Mat();
            grabFrameThread = new Thread(() =>
            {
                while (true)
                {
                    if (sink.GrabFrame(tempFrame) != 0)
                    {
                        lock (frameLock)
                        {
                            grabFrame = tempFrame;
                            grabbedFrame = true;
                        }
                    }
                }
            });
            grabFrameThread.Start();
        }
        private void startProcessFrameLoop()
        {
            Mat tempFrame = new Mat();
            Mat hsvFrame = new Mat();
            Mat mask = new Mat();
            OpenCvSharp.Point[][] contours;
            processFrameThread = new Thread(() =>
            {
                while (true)
                {
                    if (grabbedFrame)
                    {
                        lock (frameLock)
                        {
                            tempFrame = grabFrame;
                            grabbedFrame = false;
                        }
                        Cv2.CvtColor(tempFrame, hsvFrame, ColorConversionCodes.BGR2HSV);
                        Cv2.InRange(hsvFrame, new Scalar(0, 30, 50), new Scalar(40, 255, 255), mask);

                        mask.FindContours(out contours, out var hierarchy, RetrievalModes.List, ContourApproximationModes.ApproxSimple);
                        var filtered = contours.Select(x => Cv2.ConvexHull(x)).
                                            Where(x => { return Cv2.ContourArea(x) > 2000; });
                        //Cv2.DrawContours(tempFrame, filtered, -1, Scalar.Red, -1);

                        Cv2.Circle(tempFrame, ball.Position.Location.X + ball.Position.Width/2, ball.Position.Location.Y+ ball.Position.Height / 2, ball.Position.Height / 2, Scalar.Red, -1);
                        
                        if(filtered.Count() > 0)
                        {
                            OpenCvSharp.Point position = filtered.Select(x=> { return Cv2.BoundingRect(x); }).OrderBy(x => { return x.Y; }).First().TopLeft;
                            paddle.SetY(position.Y);
                        }
                        Cv2.Rectangle(tempFrame, new Rect(paddle.Position.X, paddle.Position.Y, paddle.Position.Width, paddle.Position.Height), Scalar.ForestGreen, -1);
                        Cv2.PutText(tempFrame, ball.Score.ToString(), new OpenCvSharp.Point(200, 0), HersheyFonts.HersheyPlain, 20, Scalar.Bisque);

                        lock (processedLock)
                        {
                            processed = true;
                            processedFrame = tempFrame;
                        }
                    }
                }
            });
            processFrameThread.Start();
        }
        private void startConvertFrameLoop()
        {
            Mat tempFrame = new Mat();
            Texture2D tempTexture;
            convertFrameThread = new Thread(() =>
            {
                while(graphics == null || graphics.GraphicsDevice == null)
                {

                }
                while (true)
                {
                    if (processed)
                    {
                        lock (processedLock)
                        {
                            processed = false;
                            tempFrame = processedFrame;
                        }
                        tempTexture = Texture2D.FromStream(graphics.GraphicsDevice, tempFrame.ToMemoryStream());
                        lock (textureLock)
                        {
                            texture = tempTexture;
                        }
                    }
                }
            });
            convertFrameThread.Start();
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // TODO: Add your update logic here
            ball.Update();
            paddle.Update(ball);

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // TODO: Add your drawing code here
            spriteBatch.Begin();
            lock (textureLock)
            {
                if (texture != null)
                {
                    spriteBatch.Draw(texture, Vector2.Zero, Color.White);
                }
            }
            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}

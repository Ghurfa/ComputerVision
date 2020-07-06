using FRC.CameraServer;
using FRC.CameraServer.OpenCvSharp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using System.Threading;
using System.Threading.Tasks;

namespace MonogameCV
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
        Mat frame;
        Texture2D texture;
        object lockObj;

        Thread grabFrameThread;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            camera = new UsbCamera("Camera", 0);
            camera.SetResolution(320, 240);
            sink = new CvSink("Sink");
            sink.Source = camera;
            frame = new Mat();
            lockObj = new object();
            grabFrameThread = new Thread(() =>
            {
                while(true)
                {
                    if (sink.GrabFrame(frame) != 0)
                    {
                        Texture2D temp = Texture2D.FromStream(graphics.GraphicsDevice, frame.ToMemoryStream());
                        lock (lockObj)
                        {
                            texture = temp;
                        }
                    }
                }
            });
            grabFrameThread.Start();
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
            lock (lockObj)
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

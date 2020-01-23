using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OpenVIII.Encoding.Tags;
using System;
using System.Reflection;

namespace OpenVIII
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            if (Assembly.GetCallingAssembly().GetName().Name.Contains("DirectX"))
            {
                graphics.GraphicsProfile = GraphicsProfile.HiDef;
                Memory.currentGraphicMode = Memory.GraphicModes.DirectX;
            }
            else Memory.currentGraphicMode = Memory.GraphicModes.OpenGL;
            Content.RootDirectory = "Content";
            graphics.PreferredBackBufferWidth = Memory.PreferredViewportWidth;
            graphics.PreferredBackBufferHeight = Memory.PreferredViewportHeight;
            Window.AllowUserResizing = true;
            IsFixedTimeStep = false;
            graphics.SynchronizeWithVerticalRetrace = false;
        }

        protected override void Initialize()
        {
            Window.TextInput += TextEntered;
            Memory.Log = new Log();
            FFmpeg.AutoGen.Example.FFmpegBinariesHelper.RegisterFFmpegBinaries();
            //Input.Init();
            Memory.Input2 = new Input2();
            Memory.Init(graphics, spriteBatch, Content);
            init_debugger_Audio.Init(); //this initializes the DirectAudio, it's true that it gets loaded AFTER logo, but we will do the opposite
            init_debugger_Audio.Init_SoundAudio(); //this initalizes the WAVE format audio.dat
            Memory.Log.WriteLine($"{nameof(Game)} :: {nameof(base.Initialize)}");
            base.Initialize();
        }

        static public event EventHandler<TextInputEventArgs> onTextEntered;

        private void TextEntered(object sender, TextInputEventArgs e) => onTextEntered?.Invoke(sender, e);

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            Memory.spriteBatch = spriteBatch;
            // Memory.shadowTexture = Content.Load<Texture2D>("Shadow");
            GenerateShadowModel();
            base.LoadContent();
        }

        protected override void OnExiting(object sender, EventArgs args)
        {
            GracefullyExit();
            base.OnExiting(sender, args);
        }

        private void GenerateShadowModel()
        {
            /*
             * X-X
             * X-X
             * X-X
             */
            Vector3[] vertices = new Vector3[] //3x3
            {
                new Vector3(-10,0,10),
                new Vector3(0,0,10),
                new Vector3(0,0,0),
                new Vector3(-10,0,0),
                new Vector3(10,0,10),
                new Vector3(10,0,0),
                new Vector3(0,0,-10),
                new Vector3(-10,0,-10),
                new Vector3(10,0,-10),
            };

            Vector2[] textureCoordinates = new Vector2[]
            {
                new Vector2(0.0099f, 0.9950f),
            new Vector2(0.0099f, 0.0189f),
            new Vector2(0.9777f, 0.0189f),
            new Vector2(0.9777f, 0.9950f),
            new Vector2(0.9821f, 0.9995f),
            new Vector2(0.0143f, 0.9995f),
            new Vector2(0.0143f, 0.0144f),
            new Vector2(0.9821f, 0.0144f)
            };

            VertexPositionTexture[] vpt = new VertexPositionTexture[]
                {
                //righttop (should be bottom left)
                                new VertexPositionTexture(vertices[0], textureCoordinates[6]),
                new VertexPositionTexture(vertices[1], textureCoordinates[7]),
                new VertexPositionTexture(vertices[2], textureCoordinates[4]),
                                new VertexPositionTexture(vertices[2], textureCoordinates[4]),
                new VertexPositionTexture(vertices[3], textureCoordinates[5]),
                new VertexPositionTexture(vertices[0], textureCoordinates[6]),

                //top left
                                new VertexPositionTexture(vertices[1], textureCoordinates[0]),
                new VertexPositionTexture(vertices[4], textureCoordinates[1]),
                new VertexPositionTexture(vertices[5], textureCoordinates[2]),
                                new VertexPositionTexture(vertices[5], textureCoordinates[2]),
                new VertexPositionTexture(vertices[2], textureCoordinates[3]),
                new VertexPositionTexture(vertices[1], textureCoordinates[0]),

                //bottom right should be top right
                                new VertexPositionTexture(vertices[3], textureCoordinates[7]),
                new VertexPositionTexture(vertices[2], textureCoordinates[4]),
                new VertexPositionTexture(vertices[6], textureCoordinates[5]),
                                new VertexPositionTexture(vertices[6], textureCoordinates[5]),
                new VertexPositionTexture(vertices[7], textureCoordinates[6]),
                new VertexPositionTexture(vertices[3], textureCoordinates[7]),

                //bottom left should be bottom right
                                new VertexPositionTexture(vertices[2], textureCoordinates[4]),
                new VertexPositionTexture(vertices[5], textureCoordinates[5]),
                new VertexPositionTexture(vertices[8], textureCoordinates[6]),
                                new VertexPositionTexture(vertices[8], textureCoordinates[6]),
                new VertexPositionTexture(vertices[6], textureCoordinates[7]),
                new VertexPositionTexture(vertices[2], textureCoordinates[4]),
                };

            Memory.shadowGeometry = vpt;
        }

        protected override void UnloadContent()
        {
        }

        protected override void Update(GameTime gameTime)
        {
            Memory.gameTime = gameTime;
            Memory.IsActive = IsActive;

            //it breaks the Font
            //Memory.PreferredViewportWidth = graphics.GraphicsDevice.Viewport.Width;
            //Memory.PreferredViewportHeight = graphics.GraphicsDevice.Viewport.Height;

            Input2.Update();
            Memory.Update();

            if (Input2.Button(FF8TextTagKey.Exit) || Input2.Button(FF8TextTagKey.ExitMenu))
                Exit();
            init_debugger_Audio.Update();
            ModuleHandler.Update(gameTime);
            base.Update(gameTime);
            if (Memory.SuppressDraw)
            {
                SuppressDraw();
                Memory.SuppressDraw = false;
            }

            IsMouseVisible = Memory.IsMouseVisible;
        }

        protected override void Draw(GameTime gameTime)
        {
            ModuleHandler.Draw(gameTime);
            base.Draw(gameTime);
            if (Extended.bRequestedBackBuffer)
            {
                Texture2D tex = new Texture2D(graphics.GraphicsDevice, graphics.GraphicsDevice.Viewport.Width, graphics.GraphicsDevice.Viewport.Height, false, SurfaceFormat.Color);
                byte[] b = new byte[tex.Width * tex.Height * 4];
                graphics.GraphicsDevice.GetBackBufferData<byte>(b);
                tex.SetData(b);
                Extended.BackBufferTexture = tex;
                Extended.bRequestedBackBuffer = false;
                if (tex != null)
                {
                    Extended.bBackBufferAvailable = true;
                    Extended.postBackBufferDelegate();
                }
            }
        }

        private async void GracefullyExit()
        {
            Memory.TokenSource.Cancel(); // tell task we are done
            //step0. dispose stop sounds
            Module_movie_test.Reset();
            init_debugger_Audio.StopMusic();
            init_debugger_Audio.KillAudio();
            //step1. kill init task. to prevent exceptions if exiting before fully loaded.
            await Memory.InitTask; // wait for task to finish what it's doing.
        }
    }
}
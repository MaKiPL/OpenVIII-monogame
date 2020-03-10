using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OpenVIII.Encoding.Tags;
using System;
using System.Linq;
using System.Reflection;

namespace OpenVIII
{
    public class Game1 : Game
    {
        #region Fields

        private readonly GraphicsDeviceManager _graphics;
        private Core.ImGuiRenderer _imgui;
        private SpriteBatch _spriteBatch;

        #endregion Fields

        #region Constructors

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            if (Assembly.GetCallingAssembly().GetName().Name.Contains("DirectX"))
            {
                _graphics.GraphicsProfile = GraphicsProfile.HiDef;
                Memory.CurrentGraphicMode = Memory.GraphicModes.DirectX;
            }
            else Memory.CurrentGraphicMode = Memory.GraphicModes.OpenGL;

            if (Content != null) Content.RootDirectory = "Content";
            else throw new NullReferenceException($"{nameof(Game1)}::{nameof(Content)} Maybe running linux build on windows. As is null.");
            _graphics.PreferredBackBufferWidth = Memory.PreferredViewportWidth;
            _graphics.PreferredBackBufferHeight = Memory.PreferredViewportHeight;
            if (Window != null) Window.AllowUserResizing = true;
            else throw new NullReferenceException($"{nameof(Game1)}::{nameof(Window)} Maybe running linux build on windows. As is null.");
            IsFixedTimeStep = false;
            _graphics.SynchronizeWithVerticalRetrace = false;
        }

        #endregion Constructors

        #region Events

        public static event EventHandler<TextInputEventArgs> OnTextEntered;

        #endregion Events

        #region Properties

        public string[] Arguments { get; set; }

        #endregion Properties

        #region Methods

        protected override void Draw(GameTime gameTime)
        {
            ModuleHandler.Draw(gameTime);
            base.Draw(gameTime);
            if (!Extended.bRequestedBackBuffer) return;
            Texture2D tex = new Texture2D(_graphics.GraphicsDevice, _graphics.GraphicsDevice.Viewport.Width, _graphics.GraphicsDevice.Viewport.Height, false, SurfaceFormat.Color);
            byte[] b = new byte[tex.Width * tex.Height * 4];
            _graphics.GraphicsDevice.GetBackBufferData(b);
            tex.SetData(b);
            Extended.BackBufferTexture = tex;
            Extended.bRequestedBackBuffer = false;
            Extended.bBackBufferAvailable = true;
            Extended.postBackBufferDelegate();
        }

        protected override void Initialize()
        {
            _imgui = new Core.ImGuiRenderer(this);
            _imgui.RebuildFontAtlas();
            Memory.imgui = _imgui;
            Window.TextInput += TextEntered;
            Memory.Log = new Log();
            if (Arguments != null)
            {
                string log =
                    Arguments.FirstOrDefault(x => x.Trim().StartsWith("log=", StringComparison.OrdinalIgnoreCase));
                if (!string.IsNullOrWhiteSpace((log)))
                {
                    log = log.Trim();
                    if (log.EndsWith("false", StringComparison.OrdinalIgnoreCase))
                    {
                        Memory.Log.Enabled = false;
                    }

                    if (log.EndsWith("true", StringComparison.OrdinalIgnoreCase))
                    {
                        Memory.Log.Enabled = true;
                    }
                }
            }

            FFmpeg.AutoGen.Example.FFmpegBinariesHelper.RegisterFFmpegBinaries();
            //Input.Init();
            Memory.Input2 = new Input2();
            Memory.Init(_graphics, _spriteBatch, Content, Arguments);
            AV.Music.Init(); //this initializes the DirectAudio, it's true that it gets loaded AFTER logo, but we will do the opposite
            AV.Sound.Init(); //this initializes the WAVE format audio.dat
            Memory.Log.WriteLine($"{nameof(Game)} :: {nameof(base.Initialize)}");

            //          FORCE FPS LIMIT HERE
            //this.IsFixedTimeStep = true;
            //this.TargetElapsedTime = TimeSpan.FromSeconds(1d / 30d);
            //          =====================

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            Memory.spriteBatch = _spriteBatch;
            // Memory.shadowTexture = Content.Load<Texture2D>("Shadow");
            GenerateShadowModel();
            base.LoadContent();
        }

        protected override void OnExiting(object sender, EventArgs args)
        {
            GracefullyExit();
            base.OnExiting(sender, args);
        }

        protected override void UnloadContent()
        {
        }

        protected override void Update(GameTime gameTime)
        {
            Memory.GameTime = gameTime;
            Memory.IsActive = IsActive;
            FPSCounter.Update();

            //it breaks the Font
            //Memory.PreferredViewportWidth = graphics.GraphicsDevice.Viewport.Width;
            //Memory.PreferredViewportHeight = graphics.GraphicsDevice.Viewport.Height;

            Input2.Update();
            Memory.Update();

            if (Input2.Button(FF8TextTagKey.Exit) || Input2.Button(FF8TextTagKey.ExitMenu))
                Exit();
            AV.Music.Update();
            ModuleHandler.Update(gameTime);
            base.Update(gameTime);
            if (Memory.SuppressDraw)
            {
                SuppressDraw();
                Memory.SuppressDraw = false;
            }

            IsMouseVisible = Memory.IsMouseVisible;
        }

        private static void GenerateShadowModel()
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
                //right top (should be bottom left)
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

        private static async void GracefullyExit()
        {
            Memory.TokenSource.Cancel(); // tell task we are done
            //step0. dispose stop sounds
            Module_movie_test.Reset();
            AV.Music.Stop();
            AV.Music.KillAudio();
            AV.Sound.KillAudio();
            //step1. kill init task. to prevent exceptions if exiting before fully loaded.
            if (Memory.InitTask != null)
                await Memory.InitTask; // wait for task to finish what it's doing.
        }

        private static void TextEntered(object sender, TextInputEventArgs e) => OnTextEntered?.Invoke(sender, e);

        #endregion Methods
    }
}
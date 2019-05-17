using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.IO;
using System.Linq;

namespace FF8
{
    internal class Game1 : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        internal Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            graphics.PreferredBackBufferWidth = Memory.PreferredViewportWidth;
            graphics.PreferredBackBufferHeight = Memory.PreferredViewportHeight;
            Window.AllowUserResizing = true;
        }
        protected override void Initialize()
        {
            FFmpeg.AutoGen.Example.FFmpegBinariesHelper.RegisterFFmpegBinaries();
            Input.Init();
            Memory.Init(graphics, spriteBatch, Content);
            init_debugger_Audio.Init(); //this initializes the DirectAudio, it's true that it gets loaded AFTER logo, but we will do the opposite
            init_debugger_Audio.Init_SoundAudio(); //this initalizes the WAVE format audio.dat
            Init_debugger_fields.Init(); //this initializes the field module, it's worth to have this at the beginning
            Init_debugger_battle.Init(); //this initializes the encounters
            Saves.Init(); //loads all savegames from steam or cd2000 directories. first come first serve.

            Module_movie_test.Init();
            Module_main_menu_debug.Init();

            Memory.random = new Random(); //creates global random class for all sort of things

            base.Initialize();
            //ArchiveSearch s = new ArchiveSearch(new byte[] { 0xc8,0xc9,0xca,0xcb,0xcc,0xcd,0xce,0xcf });//used to find file a string is in. disable if not using.

        }
        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            Memory.spriteBatch = spriteBatch;
            Memory.shadowTexture = Content.Load<Texture2D>("Shadow");
            GenerateShadowModel();
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

            Input.Update();

            if (Input.Button(Buttons.Exit))
                GracefullyExit();
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
            //if (Input.GetInputDelayed(Keys.F1))  //SCREENSHOT CAPABILITIES WIP; I'm leaving it as-is for now. I'll be probably using that for battle transitions (or not)
            //{
            //Texture2D tex = new Texture2D(graphics.GraphicsDevice, graphics.GraphicsDevice.Viewport.Width, graphics.GraphicsDevice.Viewport.Height, false, SurfaceFormat.Color);
            //byte[] b = new byte[tex.Width * tex.Height * 4];
            //graphics.GraphicsDevice.GetBackBufferData<byte>(b);
            //tex.SetData(b);
            //    tex.SaveAsJpeg(new System.IO.FileStream("D:/test.jpg", System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.ReadWrite), tex.Width, tex.Height);
            //}
        }

        private void GracefullyExit()
        {
            //step1. dispose DirectMusic as it's unmanaged
            init_debugger_Audio.KillAudio();
            Exit();
        }
    }
}

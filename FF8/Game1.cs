using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.IO;
using System.Linq;

namespace FF8
{
    public class Game1 : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        public Game1()
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
            Memory.graphics = graphics;
            Memory.spriteBatch = spriteBatch;
            Memory.content = Content;

            init_debugger_Audio.DEBUG(); //this initializes the DirectAudio, it's true that it gets loaded AFTER logo, but we will do the opposite
            init_debugger_Audio.DEBUG_SoundAudio(); //this initalizes the WAVE format audio.dat
            Init_debugger_fields.DEBUG(); //this initializes the field module, it's worth to have this at the beginning
            Init_debugger_battle.DEBUG(); //this initializes the encounters
            Memory.font = new Font(); //this initializes the fonts and drawing system- holds fonts in-memory
            ArchiveWorker aw = new ArchiveWorker(Memory.Archives.A_MENU);

            //TEX tex = new TEX(ArchiveWorker.GetBinaryFile(Memory.Archives.A_MENU,
            //    aw.GetListOfFiles().First(x => x.ToLower().Contains("icon.tex"))));
            //Memory.iconsTex = new Texture2D[tex.TextureData.NumOfPalettes];
            //for (int i = 0; i < Memory.iconsTex.Length; i++)
            //    Memory.iconsTex[i] = tex.GetTexture(i);
            Memory.DirtyEncoding = new DirtyEncoding();
            Memory.FieldHolder.FieldMemory = new int[1024];
            Memory.Strings = new Strings();
#if DEBUG
            //export the string data so you can find where the string you want is.
            // then you can Memory.Strings.Read() it :)
            if (Directory.Exists(@"d:\"))
            {
                Memory.Strings.Dump(Strings.FileID.MNGRP, Path.Combine(@"d:\", "MNGRPdump.txt"));
                Memory.Strings.Dump(Strings.FileID.AREAMES, Path.Combine(@"d:\", "AREAMESdump.txt"));
                Memory.Strings.Dump(Strings.FileID.NAMEDIC, Path.Combine(@"d:\", "NAMEDICdump.txt"));
                Memory.Strings.Dump(Strings.FileID.KERNEL, Path.Combine(@"d:\", "KERNELdump.txt"));
            }
#endif

            Memory.Cards = new Cards();
            Memory.Faces = new Faces();
            Memory.Icons = new Icons();
            Ff8files.Init();

            base.Initialize();
            //ArchiveSearch s = new ArchiveSearch("Zell\0");

        }
        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            Memory.spriteBatch = spriteBatch;
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

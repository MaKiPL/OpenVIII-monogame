using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

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
            graphics.PreferredBackBufferWidth = 1280;
            graphics.PreferredBackBufferHeight = 720;
            this.Window.AllowUserResizing = true;
        }
        protected override void Initialize()
        {
            Memory.graphics = graphics;
            Memory.spriteBatch = spriteBatch;
            Memory.content = Content;

            init_debugger_Audio.DEBUG(); //this initializes the DirectAudio, it's true that it gets loaded AFTER squaresoft logo, but we will do the opposite
            init_debugger_fields.DEBUG(); //this initializes the field module, it's worth to have this at the beginning

            base.Initialize();
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
            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
                GracefullyExit();


                ModuleHandler.Update(gameTime);
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            ModuleHandler.Draw(gameTime);
            base.Draw(gameTime);
        }

        private void GracefullyExit()
        {
            //step1. dispose DirectMusic as it's unmanaged
            init_debugger_Audio.KillAudio();
            Exit();
        }
    }
}

using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System;
using System.Linq;

namespace FF8
{
    internal class module_main_menu_debug
    {
        enum MainMenuStates
        {
            MainLobby,
            NewGameChoosed,
            LoadGameLoading,
            LoadGameScreen,
            DebugScreen
        }

        private static MainMenuStates State = 0;
        private static float Fade = 0.0f;
        private static Texture2D start00;
        private static Texture2D start01;

        internal static void Update()
        {
            switch(State)
            {
                case MainMenuStates.MainLobby:
                    LobbyUpdate();
                    break;
                default:
                    break;
            }
        }

        private static void LobbyUpdate()
        {
            if (start00 == null)
                start00 = GetTexture(0);
            if (start01 == null)
                start01 = GetTexture(1);
        }

        private static Texture2D GetTexture(int v)
        {
            ArchiveWorker aw = new ArchiveWorker(Memory.Archives.A_MENU);
            TEX tex;
            string filename = "";
            switch (v)
            {
                case 0:
                    filename = aw.GetListOfFiles().Where(x => x.ToLower().Contains("start00")).First();
                    break;
                case 1:
                    filename = aw.GetListOfFiles().Where(x => x.ToLower().Contains("start01")).First();
                    break;
            }
            tex = new TEX(ArchiveWorker.GetBinaryFile(Memory.Archives.A_MENU, filename));
            return tex.GetTexture();
        }

        internal static void Draw()
        {
            Memory.graphics.GraphicsDevice.Clear(Color.Black);
            switch (State)
            {
                case MainMenuStates.MainLobby:
                    DrawMainLobby();
                    break;
                default:
                    break;
            }
        }

        private static void DrawMainLobby()
        {
            //draw start00+01
            if (start00 == null || start01 == null)
                return;
            if (Fade < 1.0f)
                Fade += Memory.gameTime.ElapsedGameTime.Milliseconds / 1000.0f * 3;
            int vpWidth = Memory.graphics.GraphicsDevice.Viewport.Width;
            int vpHeight = Memory.graphics.GraphicsDevice.Viewport.Width;
            float zoom = 0.65f;
            Memory.SpriteBatchStartAlpha();
            Memory.spriteBatch.Draw(start00, new Rectangle(0, 0, (int)(vpWidth*zoom), (int)(vpHeight*(zoom-0.1f))), null, Color.White * Fade);
            Memory.spriteBatch.Draw(start01, new Rectangle((int)(vpWidth * zoom), 0, vpWidth/3, (int)(vpHeight * (zoom-0.1f))), Color.White * Fade);
            Memory.SpriteBatchEnd();
        }
    }
}
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System;
using System.Linq;
using Microsoft.Xna.Framework.Input;

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

        private static float[] choiseHeights = new float[] { 0.35f, 0.40f, 0.45f };


        static int msDelay = 0;
        static int msDelayLimit = 100;
        static bool bLimitInput = false;

        private static int choosenOption = 0;

        internal static void Update()
        {
            switch(State)
            {
                case MainMenuStates.MainLobby:
                    LobbyUpdate();
                    break;
                case MainMenuStates.DebugScreen:
                    DebugUpdate();
                    break;
                default:
                    break;
            }
        }

        private static void DebugUpdate()
        {
            int availableOptions = 7;
            if (bLimitInput)
                bLimitInput = (msDelay += Memory.gameTime.ElapsedGameTime.Milliseconds) < msDelayLimit;
            if (Keyboard.GetState().IsKeyDown(Keys.Down) && !bLimitInput)
            {
                bLimitInput = true;
                msDelay = 0;
                //play sound;
                choosenOption = choosenOption >= availableOptions ? 1 : choosenOption + 1;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.Up) && !bLimitInput)
            {
                bLimitInput = true;
                msDelay = 0;
                //play sound;
                choosenOption = choosenOption <= 1 ? availableOptions : choosenOption - 1;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.Enter) && !bLimitInput && choosenOption == 1)
            {
                bLimitInput = true;
                msDelay = 0;
                //play sound;
                choosenOption = 0;
                Fade = 0.0f;
                State = MainMenuStates.MainLobby;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.Enter) && !bLimitInput && choosenOption == 2)
            {
                bLimitInput = true;
                msDelay = 0;
                //play sound;
                choosenOption = 0;
                Fade = 0.0f;
                State = MainMenuStates.MainLobby;
                module_overture_debug.ResetModule();
            }

            if (Keyboard.GetState().IsKeyDown(Keys.Enter) && !bLimitInput && choosenOption == 5)
            {
                bLimitInput = true;
                msDelay = 0;
                //play sound;
                Memory.musicIndex++;
                init_debugger_Audio.PlayMusic();
            }
            if (Keyboard.GetState().IsKeyDown(Keys.Enter) && !bLimitInput && choosenOption == 6)
            {
                bLimitInput = true;
                msDelay = 0;
                //play sound;
                Memory.musicIndex--;
                init_debugger_Audio.PlayMusic();
            }
            if (Keyboard.GetState().IsKeyDown(Keys.Enter) && !bLimitInput && choosenOption == 7)
            {
                bLimitInput = true;
                msDelay = 0;
                //play sound;
                init_debugger_Audio.StopAudio();
            }
        }

        private static void LobbyUpdate()
        {
            if (start00 == null)
                start00 = GetTexture(0);
            if (start01 == null)
                start01 = GetTexture(1);
            if (bLimitInput)
                bLimitInput = (msDelay += Memory.gameTime.ElapsedGameTime.Milliseconds) < msDelayLimit;
            if (Keyboard.GetState().IsKeyDown(Keys.Down) && !bLimitInput)
            {
                bLimitInput = true;
                msDelay = 0;
                //play sound;
                choosenOption = choosenOption >= 2 ? 0 : choosenOption + 1;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.Up) && !bLimitInput)
            {
                bLimitInput = true;
                msDelay = 0;
                //play sound;
                choosenOption = choosenOption <= 0 ? 2 : choosenOption - 1;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.Enter) && !bLimitInput && choosenOption == 2)
            {
                bLimitInput = true;
                msDelay = 0;
                //play sound;
                choosenOption = 1;
                State = MainMenuStates.DebugScreen;
            }
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
                case MainMenuStates.DebugScreen:
                    DebugScreenLobby();
                    break;
                default:
                    break;
            }
        }

        private static void DebugScreenLobby()
        {
            float fScaleWidth = (float)Memory.graphics.GraphicsDevice.Viewport.Width / Memory.PreferredViewportWidth;
            float fScaleHeight = (float)Memory.graphics.GraphicsDevice.Viewport.Height / Memory.PreferredViewportHeight;
            int vpWidth = Memory.graphics.GraphicsDevice.Viewport.Width;
            int vpHeight = Memory.graphics.GraphicsDevice.Viewport.Width;
            float zoom = 0.65f;
            Memory.SpriteBatchStartAlpha();
            //string cCnCRtn = Font.CipherDirty("OpenVIII debug tools"); //SnclZMMM bc`se \0rmmjq
            Memory.font.RenderBasicText(Font.CipherDirty("Reset Main Menu state".Replace("\0", "")), (int)(vpWidth * 0.10f), (int)(vpHeight * 0.05f), 1f, 2f, 0, 1);
            Memory.font.RenderBasicText(Font.CipherDirty("Play Overture".Replace("\0", "")), (int)(vpWidth * 0.10f), (int)(vpHeight * 0.08f), 1f, 2f, 0, 1);
            Memory.font.RenderBasicText(Font.CipherDirty("Battle map fly".Replace("\0", "")), (int)(vpWidth * 0.10f), (int)(vpHeight * 0.11f), 1f, 2f, 0, 1);
            Memory.font.RenderBasicText(Font.CipherDirty("Field debug render".Replace("\0", "")), (int)(vpWidth * 0.10f), (int)(vpHeight * 0.14f), 1f, 2f, 0, 1);
            Memory.font.RenderBasicText(Font.CipherDirty("Play next music".Replace("\0", "")), (int)(vpWidth * 0.10f), (int)(vpHeight * 0.17f), 1f, 2f, 0, 1);
            Memory.font.RenderBasicText(Font.CipherDirty("Play previous music".Replace("\0", "")), (int)(vpWidth * 0.10f), (int)(vpHeight * 0.20f), 1f, 2f, 0, 1);
            Memory.font.RenderBasicText(Font.CipherDirty("Stop music".Replace("\0", "")), (int)(vpWidth * 0.10f), (int)(vpHeight * 0.23f), 1f, 2f, 0, 1);

            Memory.spriteBatch.Draw(Memory.iconsTex[2], new Rectangle((int)(vpWidth * 0.05f), (int)(vpHeight * ((choosenOption*0.03f) + 0.02f)+0.05f*100), (int)(24 * 2 * fScaleWidth), (int)(16 * 2 * fScaleWidth)),
                new Rectangle(232, 0, 23, 15),
                Color.White);
            Memory.SpriteBatchEnd();
        }

        private static void DrawMainLobby()
        {
            //draw start00+01
            float fScaleWidth = (float)Memory.graphics.GraphicsDevice.Viewport.Width / Memory.PreferredViewportWidth;
            float fScaleHeight = (float)Memory.graphics.GraphicsDevice.Viewport.Height / Memory.PreferredViewportHeight;

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
            //string cCnCRtn = Font.CipherDirty("OpenVIII debug tools"); //SnclZMMM bc`se \0rmmjq
            Memory.font.RenderBasicText("RI[ KEQI", (int)(vpWidth *0.42f), (int)(vpHeight * choiseHeights[0]),2f,3f,0,1);
            Memory.font.RenderBasicText("Gmlrglsc", (int)(vpWidth * 0.42f), (int)(vpHeight * choiseHeights[1]),2f,3f,0,1);
            Memory.font.RenderBasicText("SnclZMMM bc`se rmmjq", (int)(vpWidth * 0.42f), (int)(vpHeight * choiseHeights[2]),2f,3f,0,1);

            Memory.spriteBatch.Draw(Memory.iconsTex[2], new Rectangle((int)(vpWidth*0.37f), (int)(vpHeight * choiseHeights[choosenOption]+0.01f), (int)(24*2 * fScaleWidth), (int)(16*2 * fScaleWidth)),
                new Rectangle(232,0, 23 ,15),
                Color.White);
            Memory.SpriteBatchEnd();
        }
    }
}
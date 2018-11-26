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

        private static int debug_choosedBS = 0;
        private static int debug_choosedAudio = 0;


        static int msDelay = 0;
        static int msDelayLimit = 100;
        static bool bLimitInput = false;

        private static int choosenOption = 0;
        private static int debug_fieldPointer = 90;
        private static string debug_choosedField = Memory.FieldHolder.fields[debug_fieldPointer];

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
                case MainMenuStates.NewGameChoosed:
                    NewGameUpdate();
                    break;
                default:
                    break;
            }
        }

        private static void NewGameUpdate()
        {
            if (Fade > 0.0f)
                return;
            /*reverse engineering notes:
             * 
             * we should happen to reset wm2field values
             * also the basic party of Squall is now set: SG_PARTY_FIELD1 = 0, and other members are 0xFF
             */
            Memory.FieldHolder.FieldID = 74; //RE: startup stage ID is hardcoded. Probably we would want to change it for modding
            //the module changes to 1 now
            module_field_debug.ResetField();
            Memory.module = Memory.MODULE_FIELD_DEBUG;
        }

        private static void DebugUpdate()
        {
            int availableOptions = 8;
            if (bLimitInput)
                bLimitInput = (msDelay += Memory.gameTime.ElapsedGameTime.Milliseconds) < msDelayLimit;
            if (Keyboard.GetState().IsKeyDown(Keys.Down) && !bLimitInput)
            {
                bLimitInput = true;
                msDelay = 0;
                init_debugger_Audio.PlaySound(0);
                choosenOption = choosenOption >= availableOptions ? 1 : choosenOption + 1;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.Up) && !bLimitInput)
            {
                bLimitInput = true;
                msDelay = 0;
                init_debugger_Audio.PlaySound(0);
                choosenOption = choosenOption <= 1 ? availableOptions : choosenOption - 1;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.Enter) && !bLimitInput && choosenOption == 1)
            {
                bLimitInput = true;
                msDelay = 0;
                init_debugger_Audio.PlaySound(0);
                choosenOption = 0;
                Fade = 0.0f;
                State = MainMenuStates.MainLobby;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.Enter) && !bLimitInput && choosenOption == 2)
            {
                bLimitInput = true;
                msDelay = 0;
                init_debugger_Audio.PlaySound(0);
                choosenOption = 0;
                Fade = 0.0f;
                State = MainMenuStates.MainLobby;
                module_overture_debug.ResetModule();
            }
            if (Keyboard.GetState().IsKeyDown(Keys.Left) && !bLimitInput && choosenOption == 3)
            {
                bLimitInput = true;
                msDelay = 0;
                init_debugger_Audio.PlaySound(0);
                if (debug_choosedBS <= 0) return;
                debug_choosedBS--;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.Right) && !bLimitInput && choosenOption == 3)
            {
                bLimitInput = true;
                msDelay = 0;
                init_debugger_Audio.PlaySound(0);
                if (debug_choosedBS >= 162) return;
                debug_choosedBS++;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.Left) && !bLimitInput && choosenOption == 4)
            {
                bLimitInput = true;
                msDelay = 0;
                init_debugger_Audio.PlaySound(0);
                if (debug_fieldPointer <= 0) return;
                debug_fieldPointer--;
                debug_choosedField = Memory.FieldHolder.fields[debug_fieldPointer];
            }
            if (Keyboard.GetState().IsKeyDown(Keys.Right) && !bLimitInput && choosenOption == 4)
            {
                bLimitInput = true;
                msDelay = 0;
                init_debugger_Audio.PlaySound(0);
                if (debug_fieldPointer >= Memory.FieldHolder.fields.Length) return;
                debug_fieldPointer++;
                debug_choosedField = Memory.FieldHolder.fields[debug_fieldPointer];
            }
            if (Keyboard.GetState().IsKeyDown(Keys.Enter) && !bLimitInput && choosenOption == 4)
            {
                bLimitInput = true;
                msDelay = 0;
                init_debugger_Audio.PlaySound(0);
                Memory.FieldHolder.FieldID = (ushort)debug_fieldPointer;
                module_field_debug.ResetField();
                Memory.module = Memory.MODULE_FIELD_DEBUG;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.Enter) && !bLimitInput && choosenOption == 5)
            {
                bLimitInput = true;
                msDelay = 0;
                init_debugger_Audio.PlaySound(0);
                Memory.musicIndex++;
                init_debugger_Audio.PlayMusic();
            }
            if (Keyboard.GetState().IsKeyDown(Keys.Enter) && !bLimitInput && choosenOption == 3)
            {
                bLimitInput = true;
                msDelay = 0;
                init_debugger_Audio.PlaySound(0);
                Memory.bat_sceneID = debug_choosedBS;
                module_battle_debug.ResetState();
                Memory.module = Memory.MODULE_BATTLE_DEBUG;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.Enter) && !bLimitInput && choosenOption == 6)
            {
                bLimitInput = true;
                msDelay = 0;
                init_debugger_Audio.PlaySound(0);
                Memory.musicIndex--;
                init_debugger_Audio.PlayMusic();
            }
            if (Keyboard.GetState().IsKeyDown(Keys.Enter) && !bLimitInput && choosenOption == 7)
            {
                bLimitInput = true;
                msDelay = 0;
                init_debugger_Audio.PlaySound(0);
                init_debugger_Audio.StopAudio();
            }
            if (Keyboard.GetState().IsKeyDown(Keys.Left) && !bLimitInput && choosenOption == 8)
            {
                bLimitInput = true;
                msDelay = 0;
                init_debugger_Audio.PlaySound(0);
                if (debug_choosedAudio <= 0) return;
                    debug_choosedAudio--;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.Right) && !bLimitInput && choosenOption == 8)
            {
                bLimitInput = true;
                msDelay = 0;
                init_debugger_Audio.PlaySound(0);
                if (debug_choosedAudio >= init_debugger_Audio.soundEntriesCount) return;
                debug_choosedAudio++;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.Enter) && !bLimitInput && choosenOption == 8)
            {
                bLimitInput = true;
                msDelay = 0;
                init_debugger_Audio.PlaySound(0);
                init_debugger_Audio.PlaySound(debug_choosedAudio);
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
                init_debugger_Audio.PlaySound(0);
                choosenOption = choosenOption >= 2 ? 0 : choosenOption + 1;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.Up) && !bLimitInput)
            {
                bLimitInput = true;
                msDelay = 0;
                init_debugger_Audio.PlaySound(0);
                choosenOption = choosenOption <= 0 ? 2 : choosenOption - 1;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.Enter) && !bLimitInput && choosenOption == 2)
            {
                bLimitInput = true;
                msDelay = 0;
                init_debugger_Audio.PlaySound(0);
                choosenOption = 1;
                State = MainMenuStates.DebugScreen;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.Enter) && !bLimitInput && choosenOption == 0)
            {
                bLimitInput = true;
                msDelay = 0;
                init_debugger_Audio.PlaySound(28);
                State = MainMenuStates.NewGameChoosed;
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
                case MainMenuStates.NewGameChoosed:
                    NewGameDraw();
                    break;
                default:
                    break;
            }
        }

        private static void NewGameDraw()
        {
            DrawMainLobby();
            Fade -= Memory.gameTime.ElapsedGameTime.Milliseconds / 1000.0f/2;
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
            Memory.font.RenderBasicText(Font.CipherDirty($"Battle map fly: a0stg{debug_choosedBS.ToString("D3")}".Replace("\0", "")), (int)(vpWidth * 0.10f), (int)(vpHeight * 0.11f), 1f, 2f, 0, 1);
            Memory.font.RenderBasicText(Font.CipherDirty($"Field debug render: {debug_choosedField}".Replace("\0", "")), (int)(vpWidth * 0.10f), (int)(vpHeight * 0.14f), 1f, 2f, 0, 1);
            Memory.font.RenderBasicText(Font.CipherDirty("Play next music".Replace("\0", "")), (int)(vpWidth * 0.10f), (int)(vpHeight * 0.17f), 1f, 2f, 0, 1);
            Memory.font.RenderBasicText(Font.CipherDirty("Play previous music".Replace("\0", "")), (int)(vpWidth * 0.10f), (int)(vpHeight * 0.20f), 1f, 2f, 0, 1);
            Memory.font.RenderBasicText(Font.CipherDirty("Stop music".Replace("\0", "")), (int)(vpWidth * 0.10f), (int)(vpHeight * 0.23f), 1f, 2f, 0, 1);
            Memory.font.RenderBasicText(Font.CipherDirty($"Play audio.dat: {debug_choosedAudio}".Replace("\0", "")), (int)(vpWidth * 0.10f), (int)(vpHeight * 0.26f), 1f, 2f, 0, 1);

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
            if (Fade < 1.0f && State != MainMenuStates.NewGameChoosed)
                Fade += Memory.gameTime.ElapsedGameTime.Milliseconds / 1000.0f * 3;
            int vpWidth = Memory.graphics.GraphicsDevice.Viewport.Width;
            int vpHeight = Memory.graphics.GraphicsDevice.Viewport.Width;
            float zoom = 0.65f;
            Memory.SpriteBatchStartAlpha();
            Memory.spriteBatch.Draw(start00, new Rectangle(0, 0, (int)(vpWidth*zoom), (int)(vpHeight*(zoom-0.1f))), null, Color.White * Fade);
            Memory.spriteBatch.Draw(start01, new Rectangle((int)(vpWidth * zoom), 0, vpWidth/3, (int)(vpHeight * (zoom-0.1f))), Color.White * Fade);
            //string cCnCRtn = Font.CipherDirty("OpenVIII debug tools"); //SnclZMMM bc`se \0rmmjq
            Memory.font.RenderBasicText("RI[ KEQI", (int)(vpWidth *0.42f), (int)(vpHeight * choiseHeights[0]),2f,3f,0,1, Fade);
            Memory.font.RenderBasicText("Gmlrglsc", (int)(vpWidth * 0.42f), (int)(vpHeight * choiseHeights[1]),2f,3f,0, 1, Fade);
            Memory.font.RenderBasicText("SnclZMMM bc`se rmmjq", (int)(vpWidth * 0.42f), (int)(vpHeight * choiseHeights[2]),2f,3f,0,1, Fade);

            Memory.spriteBatch.Draw(Memory.iconsTex[2], new Rectangle((int)(vpWidth*0.37f), (int)(vpHeight * choiseHeights[choosenOption]+0.01f), (int)(24*2 * fScaleWidth), (int)(16*2 * fScaleWidth)),
                new Rectangle(232,0, 23 ,15),
                Color.White * Fade);
            Memory.SpriteBatchEnd();
        }
    }
}
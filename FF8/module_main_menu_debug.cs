using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;

namespace FF8
{
    internal static class Module_main_menu_debug
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
        private static float Fade;
        private static Texture2D start00;
        private static Texture2D start01;

        private static readonly float[] choiseHeights = { 0.35f, 0.40f, 0.45f };

        private static int debug_choosedBS, debug_choosedAudio;



        private static int debug_fieldPointer = 90, availableOptions = 9;
        private static Mitems Mchoose;
        private static Ditems Dchoose;

        private static string debug_choosedField = Memory.FieldHolder.fields[debug_fieldPointer];

        private static int debug_moviePointer = 0;
        private static string debug_choosedMovie = Path.GetFileNameWithoutExtension(Module_movie_test.Movies[debug_moviePointer]);
        public static int FieldPointer
        {
            get { return debug_fieldPointer; }
            set
            {
                if (value >= Memory.FieldHolder.fields.Length)
                    value = 0;
                else if (value < 0)
                    value = Memory.FieldHolder.fields.Length - 1;
                debug_fieldPointer = value;
                Memory.FieldHolder.FieldID = (ushort)value;
                debug_choosedField = Memory.FieldHolder.fields[value];
            }
        }
        public static int MoviePointer
        {
            get { return debug_moviePointer; }
            set
            {
                if (value >= Module_movie_test.Movies.Count)
                    value = 0;
                else if (value < 0)
                    value = Module_movie_test.Movies.Count-1;
                debug_moviePointer = value;
                Module_movie_test.Index = value;
                debug_choosedMovie = Path.GetFileNameWithoutExtension(Module_movie_test.Movies[value]);
            }
        }

        internal static void Update()
        {
            switch (State)
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
                case MainMenuStates.LoadGameLoading:
                    break;
                case MainMenuStates.LoadGameScreen:
                    break;
                default:
                    goto case 0;
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
            FieldPointer = 74; //RE: startup stage ID is hardcoded. Probably we would want to change it for modding
            //the module changes to 1 now
            Module_field_debug.ResetField();

            Module_movie_test.Index = 30;
            Module_movie_test.ReturnState = Memory.MODULE_FIELD_DEBUG;
            Memory.module = Memory.MODULE_MOVIETEST;
        }

        private static void DebugUpdate()
        {

            //if (bLimitInput)
            //    bLimitInput = (Input.msDelay += Memory.gameTime.ElapsedGameTime.Milliseconds) < Input.msDelayLimit;


            if (Input.Button(Buttons.Down))
            {
                Input.ResetInputLimit();
                init_debugger_Audio.PlaySound(0);
                Dchoose = Dchoose >= (Ditems)Enum.GetValues(typeof(Ditems)).Cast<int>().Max() ? 0 : Dchoose + 1;
            }
            if (Input.Button(Buttons.Up))
            {
                Input.ResetInputLimit();
                init_debugger_Audio.PlaySound(0);
                Dchoose = Dchoose <= 0 ? (Ditems)Enum.GetValues(typeof(Ditems)).Cast<int>().Max() : Dchoose - 1;
            }
            if (Input.Button(Buttons.Okay) && Dchoose == Ditems.Reset || Input.Button(Buttons.Cancel))
            {
                Input.ResetInputLimit();
                init_debugger_Audio.PlaySound(8);
                init_debugger_Audio.StopAudio();
                Dchoose = 0;
                Fade = 0.0f;
                State = MainMenuStates.MainLobby;

            }
            else if (Input.Button(Buttons.Okay))
            {
                Input.ResetInputLimit();
                if(Ditems.Sounds!=Dchoose)
                    init_debugger_Audio.PlaySound(0);
                switch (Dchoose)
                {
                    case Ditems.Overture:
                        Dchoose = 0;
                        Fade = 0.0f;
                        State = MainMenuStates.MainLobby;
                        Module_overture_debug.ResetModule();
                        Memory.module = Memory.MODULE_OVERTURE_DEBUG;
                        break;
                    case Ditems.Feild:
                        Module_field_debug.ResetField();
                        Memory.module = Memory.MODULE_FIELD_DEBUG;
                        break;
                    case Ditems.MusicNext:
                        if (Memory.MusicIndex >= ushort.MaxValue || Memory.MusicIndex >= Memory.dicMusic.Keys.Max())
                            Memory.MusicIndex = 0;
                        else
                            Memory.MusicIndex++;
                        init_debugger_Audio.PlayMusic();
                        break;
                    case Ditems.MusicPrev:

                        if (Memory.MusicIndex <= ushort.MinValue)
                            Memory.MusicIndex = ushort.MaxValue;
                        else
                            Memory.MusicIndex--;
                        init_debugger_Audio.PlayMusic();
                        break;
                    case Ditems.Battle:
                        Memory.battle_encounter = debug_choosedBS;
                        Module_battle_debug.ResetState();
                        Memory.module = Memory.MODULE_BATTLE_DEBUG;
                        break;
                    case Ditems.MusicStop:
                        init_debugger_Audio.StopAudio();
                        break;
                    case Ditems.Sounds:
                        init_debugger_Audio.PlaySound(debug_choosedAudio);
                        break;
                    case Ditems.Movie:
                        Fade = 0.0f;
                        MoviePointer = MoviePointer; //makes movieindex in player match the moviepointer, it is set when ever this is.
                        Memory.module = Memory.MODULE_MOVIETEST;
                        Module_movie_test.MovieState = 0;
                        break;
                    case Ditems.World:
                        Memory.module = Memory.MODULE_WORLD_DEBUG;
                        break;
                }
            }
            else if (Input.Button(Buttons.Left))
            {
                Input.ResetInputLimit();
                init_debugger_Audio.PlaySound(0);
                switch (Dchoose)
                {
                    case Ditems.Sounds:
                        if (debug_choosedAudio > 0)
                            debug_choosedAudio--;
                        break;
                    case Ditems.Battle:
                        if (debug_choosedBS <= 0) return;
                        debug_choosedBS--;
                        break;
                    case Ditems.Feild:
                        FieldPointer--;
                        break;
                    case Ditems.Movie:
                        MoviePointer--;
                        break;
                }
            }
            else if (Input.Button(Buttons.Right))
            {
                Input.ResetInputLimit();
                init_debugger_Audio.PlaySound(0);
                switch (Dchoose)
                {
                    case Ditems.Sounds:
                        if (debug_choosedAudio < init_debugger_Audio.soundEntriesCount)
                            debug_choosedAudio++;
                        break;
                    case Ditems.Battle:
                        if (debug_choosedBS < Memory.encounters.Length) 
                            debug_choosedBS++;
                        break;

                    case Ditems.Feild:
                        FieldPointer++;
                        break;
                    case Ditems.Movie:
                        MoviePointer++;
                        break;
                }
            }
        }
        private static void LobbyUpdate()
        {
            if (start00 == null)
                start00 = GetTexture(0);
            if (start01 == null)
                start01 = GetTexture(1);

            if (Input.Button(Buttons.Down))
            {
                Input.ResetInputLimit();
                init_debugger_Audio.PlaySound(0);
                Mchoose = Mchoose >= (Mitems)Enum.GetValues(typeof(Mitems)).Cast<int>().Max() ? 0 : Mchoose + 1;
            }
            if (Input.Button(Buttons.Up))
            {
                Input.ResetInputLimit();
                init_debugger_Audio.PlaySound(0);
                Mchoose = Mchoose <= 0 ? (Mitems)Enum.GetValues(typeof(Mitems)).Cast<int>().Max() : Mchoose - 1;
            }
            if (Input.Button(Buttons.Okay))
            {
                Input.ResetInputLimit();
                switch (Mchoose)
                {
                    case Mitems.New:
                        init_debugger_Audio.PlaySound(28);
                        State = MainMenuStates.NewGameChoosed;
                        break;
                    //case Mitems.Load:
                    //    init_debugger_Audio.PlaySound(0);
                    //    State = MainMenuStates.LoadGameScreen;
                    //    break;
                    case Mitems.Debug:
                        init_debugger_Audio.PlaySound(0);
                        Mchoose = 0;
                        Dchoose = 0;
                        State = MainMenuStates.DebugScreen;
                        break;
                }
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
                    filename = aw.GetListOfFiles().First(x => x.ToLower().Contains("start00"));
                    break;
                case 1:
                    filename = aw.GetListOfFiles().First(x => x.ToLower().Contains("start01"));
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
                case MainMenuStates.LoadGameLoading:
                    break;
                case MainMenuStates.LoadGameScreen:
                    break;
            }
        }

        private static void NewGameDraw()
        {
            DrawMainLobby();
            Fade -= Memory.gameTime.ElapsedGameTime.Milliseconds / 1000.0f / 2;
        }
        enum Mitems
        {
            New,
            Load,
            Debug
        }
        enum Ditems
        {
            Reset,
            Overture,
            Battle,
            Feild,
            Movie,
            MusicNext,
            MusicPrev,
            MusicStop,
            Sounds,
            World
        }
        private static Dictionary<Ditems, string> DebugMenu = new Dictionary<Ditems, string>()
            {
                { Ditems.Reset, "Reset Main Menu state"},
                { Ditems.Overture, "Play Overture"},
                { Ditems.Battle, "Battle encounter: {0}"},
                { Ditems.Feild, "Field debug render: {1}"},
                { Ditems.Movie, "Movie debug render: {2}"},
                { Ditems.MusicNext,"Play next music"},
                { Ditems.MusicPrev,"Play previous music"},
                { Ditems.MusicStop,"Stop music"},
                { Ditems.Sounds,"Play audio.dat: {3}"},
                { Ditems.World,"Jump to World Map"}
            };

        private static void DebugScreenLobby()
        {
            string[] info = new string[] //fills in {0} threw {3}
            {
                debug_choosedBS.ToString("D4"),
                debug_choosedField,
                debug_choosedMovie,
                $"{debug_choosedAudio}",
            };
            float fScaleWidth = (float)Memory.graphics.GraphicsDevice.Viewport.Width / Memory.PreferredViewportWidth;
            float fScaleHeight = (float)Memory.graphics.GraphicsDevice.Viewport.Height / Memory.PreferredViewportHeight;
            int vpWidth = Memory.graphics.GraphicsDevice.Viewport.Width;
            int vpHeight = Memory.graphics.GraphicsDevice.Viewport.Width;
            Memory.SpriteBatchStartAlpha();
            //string cCnCRtn = Font.CipherDirty("OpenVIII debug tools"); //SnclZMMM bc`se \0rmmjq
            float vpSpace = vpHeight * 0.03f;
            float item = 0;

            foreach (KeyValuePair<Ditems, string> e in DebugMenu)
            {
                Memory.font.RenderBasicText(Font.CipherDirty(string.Format(e.Value, info[0], info[1], info[2], info[3]).Replace("\0", "")),
                    (int)(vpWidth * 0.10f), (int)(vpHeight * 0.05f + vpSpace * item++), 1f, 2f, 0, 1);
            }
            Memory.spriteBatch.Draw(Memory.iconsTex[2], new Rectangle(
                (int)(vpWidth * 0.05f), (int)(vpHeight * 0.05f + vpSpace * ((float)Dchoose)),
                (int)(24 * 2 * fScaleWidth), (int)(16 * 2 * fScaleWidth)),
                new Rectangle(232, 0, 23, 15), Color.White);
            availableOptions = (int)item;
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
            Memory.spriteBatch.Draw(start00, new Rectangle(0, 0, (int)(vpWidth * zoom), (int)(vpHeight * (zoom - 0.1f))), null, Color.White * Fade);
            Memory.spriteBatch.Draw(start01, new Rectangle((int)(vpWidth * zoom), 0, vpWidth / 3, (int)(vpHeight * (zoom - 0.1f))), Color.White * Fade);
            //string cCnCRtn = Font.CipherDirty("OpenVIII debug tools"); //SnclZMMM bc`se \0rmmjq

            //Memory.font.RenderBasicText("RI[ KEQI", (int)(vpWidth * 0.42f), (int)(vpHeight * choiseHeights[0]), 2f, 3f, 0, 1, Fade);
            Memory.font.RenderBasicText(Font.CipherDirty(Memory.MainMenuLines[0]), (int)(vpWidth * 0.42f), (int)(vpHeight * choiseHeights[0]), 2f, 3f, 0, 1, Fade);
            //Memory.font.RenderBasicText("Gmlrglsc", (int)(vpWidth * 0.42f), (int)(vpHeight * choiseHeights[1]), 2f, 3f, 0, 1, Fade);
            Memory.font.RenderBasicText(Font.CipherDirty(Memory.MainMenuLines[1]), (int)(vpWidth * 0.42f), (int)(vpHeight * choiseHeights[1]), 2f, 3f, 0, 1, Fade);
            //Memory.font.RenderBasicText("SnclZMMM bc`se rmmjq", (int)(vpWidth * 0.42f), (int)(vpHeight * choiseHeights[2]), 2f, 3f, 0, 1, Fade);
            Memory.font.RenderBasicText(Font.CipherDirty(Memory.MainMenuLines[2]), (int)(vpWidth * 0.42f), (int)(vpHeight * choiseHeights[2]), 2f, 3f, 0, 1, Fade);

            Memory.spriteBatch.Draw(Memory.iconsTex[2], new Rectangle((int)(vpWidth * 0.37f), (int)(vpHeight * choiseHeights[(int)Mchoose] + 0.01f), (int)(24 * 2 * fScaleWidth), (int)(16 * 2 * fScaleWidth)),
                new Rectangle(232, 0, 23, 15),
                Color.White * Fade);
            Memory.SpriteBatchEnd();
        }
    }
}
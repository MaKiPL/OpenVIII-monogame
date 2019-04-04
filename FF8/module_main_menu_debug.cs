using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FF8
{
    internal static class Module_main_menu_debug
    {
        #region Fields

        /// <summary>
        /// Strings for the debug menu
        /// </summary>
        private static readonly Dictionary<Ditems, string> strDebugLobby = new Dictionary<Ditems, string>()
        {
            { Ditems.Reset, "Reset Main Menu state"},
            { Ditems.Overture, "Play Overture"},
            { Ditems.Battle, "Battle encounter: {0}"},
            { Ditems.Feild, "Field debug render: {0}"},
            { Ditems.Movie, "Movie debug render: {0}"},
            { Ditems.Music, "Play/Stop music : {0}"},
            { Ditems.Sounds, "Play audio.dat: {0}"},
            { Ditems.World, "Jump to World Map"}
        };

        /// <summary>
        /// Strings for the main menu
        /// </summary>
        private static readonly Dictionary<Mitems, string> strMainLobby = new Dictionary<Mitems, string>()
        {
            { Mitems.New, "NEW GAME" },
            { Mitems.Load, "Continue" },
            { Mitems.Debug, "OpenVIII debug tools" }
        };

        private static Ditems s_dchoose;
        private static int debug_choosedBS, debug_choosedAudio;
        private static string debug_choosedField = Memory.FieldHolder.fields[debug_fieldPointer];
        private static string debug_choosedMovie = Path.GetFileNameWithoutExtension(Module_movie_test.Movies[debug_moviePointer]);
        private static string debug_choosedMusic = Path.GetFileNameWithoutExtension(Memory.dicMusic[0][0]);
        private static int debug_fieldPointer = 90;
        private static int debug_moviePointer = 0;
        private static float Fade;
        private static float fScaleHeight;
        private static float fScaleWidth;
        private static Texture2D start00;
        private static Texture2D start01;
        private static MainMenuStates State = 0;
        private static int vpHeight;
        private static int vpWidth;
        private static Mitems s_mchoose;

        #endregion Fields

        #region Enums
        /// <summary>
        /// Identifiers and Ordering of debug menu items
        /// </summary>
        private enum Ditems
        {
            Reset,
            Overture,
            Battle,
            Feild,
            Movie,
            Music,
            Sounds,
            World
        }

        /// <summary>
        /// What state the menus are in.
        /// </summary>
        private enum MainMenuStates
        {
            Init,
            MainLobby,
            NewGameChoosed,
            LoadGameLoading,
            LoadGameScreen,
            DebugScreen
        }

        /// <summary>
        /// Identifiers and Ordering of main menu items
        /// </summary>
        private enum Mitems
        {
            New,
            Load,
            Debug
        }

        #endregion Enums

        #region Properties

        /// <summary>
        /// Current choice on main menu
        /// </summary>
        private static Mitems Mchoose
        {
            get => s_mchoose;
            set
            {
                if (value > s_mchoose && value > (Mitems)Enum.GetValues(typeof(Mitems)).Cast<int>().Max())
                {
                    value = 0;
                }
                else if (value < s_mchoose && s_mchoose <= 0)
                {
                    value = (Mitems)Enum.GetValues(typeof(Mitems)).Cast<int>().Max();
                }
                s_mchoose = value;
            }
        }

        /// <summary>
        /// Current choice on debug menu
        /// </summary>
        private static Ditems Dchoose
        {
            get => s_dchoose; set
            {
                if (value > s_dchoose && value > (Ditems)Enum.GetValues(typeof(Ditems)).Cast<int>().Max())
                {
                    value = 0;
                }
                else if (value < s_dchoose && s_dchoose <= 0)
                {
                    value = (Ditems)Enum.GetValues(typeof(Ditems)).Cast<int>().Max();
                }
                s_dchoose = value;
            }
        }

        /// <summary>
        /// Currently selected Field
        /// </summary>
        public static int FieldPointer
        {
            get => debug_fieldPointer;
            set
            {
                if (value >= Memory.FieldHolder.fields.Length)
                {
                    value = 0;
                }
                else if (value < 0)
                {
                    value = Memory.FieldHolder.fields.Length - 1;
                }

                debug_fieldPointer = value;
                Memory.FieldHolder.FieldID = (ushort)value;
                debug_choosedField = Memory.FieldHolder.fields[value];
            }
        }

        /// <summary>
        /// Currently selected Movie
        /// </summary>
        public static int MoviePointer
        {
            get => debug_moviePointer;
            set
            {
                if (value >= Module_movie_test.Movies.Count)
                {
                    value = 0;
                }
                else if (value < 0)
                {
                    value = Module_movie_test.Movies.Count - 1;
                }

                debug_moviePointer = value;
                Module_movie_test.Index = value;
                debug_choosedMovie = Path.GetFileNameWithoutExtension(Module_movie_test.Movies[value]);
            }
        }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Triggers functions depending on state
        /// </summary>
        internal static void Update()
        {
            switch (State)
            {
                case MainMenuStates.Init:
                    State++;
                    Init();
                    break;

                case MainMenuStates.MainLobby:
                    if (!UpdateMainLobby() && (Fade >= 1f || Fade < 0))
                    {
                        Memory.SuppressDraw = true;
                    }

                    break;

                case MainMenuStates.DebugScreen:
                    if (!UpdateDebugLobby() && (Fade >= 1f || Fade < 0))
                    {
                        Memory.SuppressDraw = true;
                    }

                    break;

                case MainMenuStates.NewGameChoosed:
                    UpdateNewGame();
                    break;

                case MainMenuStates.LoadGameLoading:
                    break;

                case MainMenuStates.LoadGameScreen:
                    break;

                default:
                    goto case 0;
            }
            if (Fade < 1.0f && State != MainMenuStates.NewGameChoosed)
            {
                Fade += Memory.gameTime.ElapsedGameTime.Milliseconds / 1000.0f * 3;
            }
        }

        /// <summary>
        /// Update Main Lobby
        /// </summary>
        /// <returns>true on change</returns>
        private static bool UpdateMainLobby()
        {
            bool ret = false;
            if (Input.Button(Buttons.Down))
            {
                Input.ResetInputLimit();
                init_debugger_Audio.PlaySound(0);
                //Mchoose = Mchoose >= (Mitems)Enum.GetValues(typeof(Mitems)).Cast<int>().Max() ? 0 : Mchoose + 1;
                Mchoose++;
                ret = true;
            }
            if (Input.Button(Buttons.Up))
            {
                Input.ResetInputLimit();
                init_debugger_Audio.PlaySound(0);
                Mchoose--;
                //Mchoose = Mchoose <= 0 ? (Mitems)Enum.GetValues(typeof(Mitems)).Cast<int>().Max() : Mchoose - 1;
                ret = true;
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
                        Fade = 0.0f;
                        break;
                }
                ret = true;
            }
            return ret;
        }

        /// <summary>
        /// Update Debug Menu
        /// </summary>
        /// <returns>true on change</returns>
        private static bool UpdateDebugLobby()
        {
            bool ret = false;
            if (Input.Button(Buttons.Down))
            {
                Input.ResetInputLimit();
                init_debugger_Audio.PlaySound(0);
                Dchoose++;
                ret = true;
            }
            if (Input.Button(Buttons.Up))
            {
                Input.ResetInputLimit();
                init_debugger_Audio.PlaySound(0);
                Dchoose--;
                ret = true;
            }
            if (Input.Button(Buttons.Okay) && Dchoose == Ditems.Reset || Input.Button(Buttons.Cancel))
            {
                Input.ResetInputLimit();
                init_debugger_Audio.PlaySound(8);
                init_debugger_Audio.StopAudio();
                Dchoose = 0;
                Fade = 0.0f;
                State = MainMenuStates.MainLobby;
                ret = true;
            }
            else if (Input.Button(Buttons.Okay))
            {
                ret = true;
                Input.ResetInputLimit();
                if (Ditems.Sounds != Dchoose)
                {
                    init_debugger_Audio.PlaySound(0);
                }

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

                    case Ditems.Music:
                        Module_field_debug.ResetField();
                        init_debugger_Audio.PlayStopMusic();
                        break;

                    case Ditems.Battle:
                        Memory.battle_encounter = debug_choosedBS;
                        Module_battle_debug.ResetState();
                        Memory.module = Memory.MODULE_BATTLE_DEBUG;
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
                ret = true;
                Input.ResetInputLimit();
                init_debugger_Audio.PlaySound(0);
                switch (Dchoose)
                {
                    case Ditems.Sounds:
                        if (debug_choosedAudio > 0)
                        {
                            debug_choosedAudio--;
                        }

                        break;

                    case Ditems.Battle:
                        if (debug_choosedBS <= 0)
                        {
                            return false;
                        }

                        debug_choosedBS--;
                        break;

                    case Ditems.Feild:
                        FieldPointer--;
                        break;

                    case Ditems.Movie:
                        MoviePointer--;
                        break;

                    case Ditems.Music:

                        if (Memory.MusicIndex <= ushort.MinValue)
                        {
                            Memory.MusicIndex = ushort.MaxValue;
                        }
                        else
                        {
                            Memory.MusicIndex--;
                        }
                        debug_choosedMusic = Path.GetFileNameWithoutExtension(Memory.dicMusic[Memory.MusicIndex][0]);
                        break;
                }
            }
            else if (Input.Button(Buttons.Right))
            {
                ret = true;
                Input.ResetInputLimit();
                init_debugger_Audio.PlaySound(0);
                switch (Dchoose)
                {
                    case Ditems.Sounds:
                        if (debug_choosedAudio < init_debugger_Audio.soundEntriesCount)
                        {
                            debug_choosedAudio++;
                        }

                        break;

                    case Ditems.Battle:
                        if (debug_choosedBS < Memory.encounters.Length)
                        {
                            debug_choosedBS++;
                        }

                        break;

                    case Ditems.Feild:
                        FieldPointer++;
                        break;

                    case Ditems.Movie:
                        MoviePointer++;
                        break;

                    case Ditems.Music:
                        //case Ditems.MusicNext:
                        if (Memory.MusicIndex >= ushort.MaxValue || Memory.MusicIndex >= Memory.dicMusic.Keys.Max())
                        {
                            Memory.MusicIndex = 0;
                        }
                        else
                        {
                            Memory.MusicIndex++;
                        }
                        debug_choosedMusic = Path.GetFileNameWithoutExtension(Memory.dicMusic[Memory.MusicIndex][0]);
                        //init_debugger_Audio.PlayMusic();
                        break;
                }
            }
            return ret;
        }

        /// <summary>
        /// Waits for fade to end then triggers new game.
        /// </summary>
        private static void UpdateNewGame()
        {
            if (Fade > 0.0f)
            {
                Fade -= Memory.gameTime.ElapsedGameTime.Milliseconds / 1000.0f / 2;
                return;
            }
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

        /// <summary>
        /// Init
        /// </summary>
        private static void Init()
        {
            if (start00 == null)
            {
                start00 = GetTexture(0);
            }

            if (start01 == null)
            {
                start01 = GetTexture(1);
            }
        }

        /// <summary>
        /// Get reqeusted texture from Menu archive.
        /// </summary>
        /// <param name="v">0 or 1</param>
        /// <returns>Requested Texture</returns>
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

        /// <summary>
        /// Trigger required draw function.
        /// </summary>
        internal static void Draw()
        {
            Memory.graphics.GraphicsDevice.Clear(Color.Black);
            fScaleWidth = (float)Memory.graphics.GraphicsDevice.Viewport.Width / Memory.PreferredViewportWidth;
            fScaleHeight = (float)Memory.graphics.GraphicsDevice.Viewport.Height / Memory.PreferredViewportHeight;
            vpWidth = Memory.graphics.GraphicsDevice.Viewport.Width;
            vpHeight = Memory.graphics.GraphicsDevice.Viewport.Width;

            switch (State)
            {
                case MainMenuStates.Init:
                case MainMenuStates.MainLobby:
                    DrawMainLobby();
                    break;

                case MainMenuStates.DebugScreen:
                    DrawDebugLobby();
                    break;

                case MainMenuStates.NewGameChoosed:
                    DrawMainLobby();
                    break;

                case MainMenuStates.LoadGameLoading:
                    break;

                case MainMenuStates.LoadGameScreen:
                    break;
            }
        }

        /// <summary>
        /// Draw Main menu
        /// </summary>
        private static void DrawMainLobby()
        {
            float zoom = 0.65f;
            Memory.SpriteBatchStartAlpha();
            Memory.spriteBatch.Draw(start00, new Rectangle(0, 0, (int)(vpWidth * zoom), (int)(vpHeight * (zoom - 0.1f))), null, Color.White * Fade);
            Memory.spriteBatch.Draw(start01, new Rectangle((int)(vpWidth * zoom), 0, vpWidth / 3, (int)(vpHeight * (zoom - 0.1f))), Color.White * Fade);
            float vpSpace = vpHeight * 0.05f;
            float item = 0;
            foreach (Mitems i in (Mitems[])Enum.GetValues(typeof(Mitems)))
            {
                Memory.font.RenderBasicText(Font.CipherDirty(strMainLobby[i]).Replace("\0", ""),
                    (int)(vpWidth * 0.42f), (int)(vpHeight * 0.35f + vpSpace * item++), 2f, 3f, 0, 1, Fade);
            }
            Memory.spriteBatch.Draw(Memory.iconsTex[2], new Rectangle(
                (int)(vpWidth * 0.37f),
                (int)(vpHeight * 0.35f + vpSpace * (float)Mchoose),
                (int)(24 * 2 * fScaleWidth),
                (int)(16 * 2 * fScaleWidth)),
                new Rectangle(232, 0, 23, 15), Color.White * Fade);
            Memory.SpriteBatchEnd();
        }

        /// <summary>
        /// Draw Debug Menu
        /// </summary>
        private static void DrawDebugLobby()
        {
            Memory.SpriteBatchStartAlpha();
            float vpSpace = vpHeight * 0.03f;
            float item = 0;

            //foreach (KeyValuePair<Ditems, string> e in DebugMenu)
            foreach (Ditems i in (Ditems[])Enum.GetValues(typeof(Ditems)))
            {
                Memory.font.RenderBasicText(Font.CipherDirty(string.Format(strDebugLobby[i], InfoDebugLobby(i)).Replace("\0", "")),
                    (int)(vpWidth * 0.10f), (int)(vpHeight * 0.05f + vpSpace * item++), 1f, 2f, 0, 1, Fade);
            }
            Memory.spriteBatch.Draw(Memory.iconsTex[2], new Rectangle(
                (int)(vpWidth * 0.05f),
                (int)(vpHeight * 0.05f + vpSpace * ((float)Dchoose)),
                (int)(24 * 2 * fScaleWidth), (int)(16 * 2 * fScaleWidth)),
                new Rectangle(232, 0, 23, 15), Color.White * Fade);
            Memory.SpriteBatchEnd();
        }

        /// <summary>
        /// Dynamic info for Ditem that is read at draw time.
        /// </summary>
        /// <param name="i">Ditem being drawn</param>
        /// <returns>Dynamic info for Ditem</returns>
        private static string InfoDebugLobby(Ditems i)
        {
            switch (i)
            {
                case Ditems.Battle:
                    return debug_choosedBS.ToString("D4");

                case Ditems.Feild:
                    return debug_choosedField;

                case Ditems.Movie:
                    return debug_choosedMovie;

                case Ditems.Sounds:
                    return $"{debug_choosedAudio}";

                case Ditems.Music:
                    return $"{debug_choosedMusic}";
            };
            return "";
        }

        #endregion Methods
    }
}
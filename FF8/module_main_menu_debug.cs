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
        private static readonly Dictionary<Ditems, Item> strDebugLobby = new Dictionary<Ditems, Item>()
        {
            { Ditems.Reset, new Item{Text="Reset Main Menu state" } },
            { Ditems.Overture, new Item{Text="Play Overture"} },
            { Ditems.Battle, new Item{Text="Battle encounter: {0}"} },
            { Ditems.Field, new Item{Text="Field debug render: {0}"} },
            { Ditems.Movie, new Item{Text="Movie debug render: {0}"} },
            { Ditems.Music, new Item{Text="Play/Stop music : {0}"} },
            { Ditems.Sounds, new Item{Text="Play audio.dat: {0}"} },
            { Ditems.World, new Item{Text="Jump to World Map"} },
            { Ditems.Faces, new Item{Text="Test Faces"} },
            { Ditems.TestMenu, new Item{Text="Test Menu Loading"} }
        };

        /// <summary>
        /// Strings for the main menu
        /// </summary>
        private static readonly Dictionary<Mitems, Item> strMainLobby = new Dictionary<Mitems, Item>()
        {
            { Mitems.New, new Item{Text="NEW GAME" } },
            { Mitems.Load, new Item{Text="Continue" } },
            { Mitems.Debug, new Item{Text="OpenVIII debug tools" } }
        };

        private static int debug_choosedBS, debug_choosedAudio;
        private static string debug_choosedField = Memory.FieldHolder.fields[debug_fieldPointer];
        private static string debug_choosedMovie = Path.GetFileNameWithoutExtension(Module_movie_test.Movies[debug_moviePointer]);
        private static string debug_choosedMusic = Path.GetFileNameWithoutExtension(Memory.dicMusic[0][0]);
        private static int debug_fieldPointer = 90;
        private static int debug_moviePointer = 0;
        private static float fade;
        private static float fScaleHeight;
        private static float fScaleWidth;
        private static float lastfade;
        private static Ditems s_dchoose;
        private static Mitems s_mchoose;
        private static Texture2D start00;
        private static Texture2D start01;
        private static MainMenuStates State = 0;
        private static int vpHeight;
        private static int vpWidth;

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
            Field,
            Movie,
            Music,
            Sounds,
            World,
            Faces,
            TestMenu
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

        public static float Fade { get => fade; set => fade = value; }

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

        #endregion Properties

        #region Methods
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
        /// Triggers functions depending on state
        /// </summary>
        internal static void Update()
        {
            if (Fade < 1.0f && State != MainMenuStates.NewGameChoosed)
            {
                Fade += Memory.gameTime.ElapsedGameTime.Milliseconds / 1000.0f * 3;
            }
            switch (State)
            {
                case MainMenuStates.Init:
                    State++;
                    Init();
                    break;

                case MainMenuStates.MainLobby:
                    if (!UpdateMainLobby() && (lastfade == fade))
                    {
                        Memory.SuppressDraw = true;
                        Memory.IsMouseVisible = true;
                    }

                    break;

                case MainMenuStates.DebugScreen:
                    if (!UpdateDebugLobby() && (lastfade == fade))
                    {
                        Memory.SuppressDraw = true;
                        Memory.IsMouseVisible = true;
                    }

                    break;

                case MainMenuStates.NewGameChoosed:
                    Memory.IsMouseVisible = false;
                    UpdateNewGame();
                    break;

                case MainMenuStates.LoadGameLoading:
                    break;

                case MainMenuStates.LoadGameScreen:
                    break;

                default:
                    goto case 0;
            }
        }

        /// <summary>
        /// Update Main Lobby
        /// </summary>
        /// <returns>true on change</returns>
        private static bool UpdateMainLobby()
        {
            bool ret = false;
            Point ml = Input.MouseLocation;
            foreach (KeyValuePair<Mitems, Item> entry in strMainLobby)
            {
                if (entry.Value.Loc.Contains(ml))
                {
                    Mchoose = entry.Key;
                    ret = true;

                    if (Input.Button(Buttons.MouseWheelup) || Input.Button(Buttons.MouseWheeldown))
                    {
                        return ret;
                    }
                    break;
                }
            }
            if (Input.Button(Buttons.Down))
            {
                Input.ResetInputLimit();
                init_debugger_Audio.PlaySound(0);
                Mchoose++;
                ret = true;
            }
            if (Input.Button(Buttons.Up))
            {
                Input.ResetInputLimit();
                init_debugger_Audio.PlaySound(0);
                Mchoose--;
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
            State = MainMenuStates.MainLobby;
        }

        /// <summary>
        /// Update Debug Menu
        /// </summary>
        /// <returns>true on change</returns>
        private static bool UpdateDebugLobby()
        {
            bool ret = false;
            Point ml = Input.MouseLocation;
            foreach (KeyValuePair<Ditems, Item> entry in strDebugLobby)
            {
                if (entry.Value.Loc.Contains(ml))
                {
                    Dchoose = entry.Key;
                    ret = true;

                    if (Input.Button(Buttons.MouseWheelup))
                    {
                        return UpdateDebugLobbyLEFT();
                    }
                    if (Input.Button(Buttons.MouseWheeldown))
                    {
                        return UpdateDebugLobbyRIGHT();
                    }
                    break;
                }
            }
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
                ret = UpdateDebugLobbyOKAY();
            }
            else if (Input.Button(Buttons.Left))
            {
                ret = UpdateDebugLobbyLEFT();
            }
            else if (Input.Button(Buttons.Right))
            {
                ret = UpdateDebugLobbyRIGHT();
            }
            return ret;
        }

        private static bool UpdateDebugLobbyLEFT()
        {
            bool ret = true;
            Input.ResetInputLimit();
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

                case Ditems.Field:
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

                default:
                    ret = false;
                    break;
            }
            if (ret)
            {
                init_debugger_Audio.PlaySound(0);
            }

            return ret;
        }

        private static bool UpdateDebugLobbyOKAY()
        {
            bool ret = true;
            Input.ResetInputLimit();
            switch (Dchoose)
            {
                case Ditems.Overture:
                    Dchoose = 0;
                    Fade = 0.0f;
                    State = MainMenuStates.MainLobby;
                    Module_overture_debug.ResetModule();
                    Memory.module = Memory.MODULE_OVERTURE_DEBUG;
                    Memory.IsMouseVisible = false;
                    init_debugger_Audio.PlayStopMusic();
                    break;

                case Ditems.Field:
                    Fade = 0.0f;
                    Module_field_debug.ResetField();
                    Memory.module = Memory.MODULE_FIELD_DEBUG;
                    Memory.IsMouseVisible = false;
                    break;

                case Ditems.Music:
                    Module_field_debug.ResetField();
                    init_debugger_Audio.PlayStopMusic();
                    break;

                case Ditems.Battle:
                    Fade = 0.0f;
                    Memory.battle_encounter = debug_choosedBS;
                    Module_battle_debug.ResetState();
                    Memory.module = Memory.MODULE_BATTLE_DEBUG;
                    Memory.IsMouseVisible = false;
                    break;

                case Ditems.Sounds:
                    init_debugger_Audio.PlaySound(debug_choosedAudio);
                    break;

                case Ditems.Movie:
                    Fade = 0.0f;
                    MoviePointer = MoviePointer; //makes movieindex in player match the moviepointer, it is set when ever this is.
                    Memory.module = Memory.MODULE_MOVIETEST;
                    Module_movie_test.MovieState = 0;
                    Memory.IsMouseVisible = false;
                    break;

                case Ditems.World:
                    Fade = 0.0f;
                    Memory.module = Memory.MODULE_WORLD_DEBUG;
                    Memory.IsMouseVisible = false;
                    break;

                case Ditems.Faces:
                    Fade = 0.0f;
                    Memory.module = Memory.MODULE_FACE_TEST;
                    break;

                case Ditems.TestMenu:
                    Fade = 0.0f;
                    Memory.module = Memory.MODULE_TESTMENU_DEBUG;
                    break;

                default:
                    ret = false;
                    break;
            }
            if (ret && Ditems.Sounds != Dchoose)
            {
                init_debugger_Audio.PlaySound(0);
            }
            return ret;
        }

        private static bool UpdateDebugLobbyRIGHT()
        {
            bool ret = true;
            Input.ResetInputLimit();
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

                case Ditems.Field:
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

                default:
                    ret = false;
                    break;
            }
            if (ret)
            {
                init_debugger_Audio.PlaySound(0);
            }

            return ret;
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
            lastfade = fade;
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
                Item c = strMainLobby[i];
                c.Loc = (Memory.font.RenderBasicText(Font.CipherDirty(c.Text).Replace("\0", ""),
                    (int)(vpWidth * 0.42f), (int)(vpHeight * 0.35f + vpSpace * item++), 2f, 3f, 0, 1, Fade));
                strMainLobby[i] = c;
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
                Item c = strDebugLobby[i];
                c.Loc = (Memory.font.RenderBasicText(Font.CipherDirty(string.Format(c.Text, InfoDebugLobby(i)).Replace("\0", "")),
                    (int)(vpWidth * 0.10f), (int)(vpHeight * 0.05f + vpSpace * item++), 1f, 2f, 0, 1, Fade));
                strDebugLobby[i] = c;
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

                case Ditems.Field:
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

        #region Structs
        /// <summary>
        /// Container for MenuItems containing relevant info.
        /// </summary>
        private struct Item
        {
            #region Properties

            public Rectangle Loc { get; set; }
            public string Text { get; set; }

            #endregion Properties
        }

        #endregion Structs
    }
}
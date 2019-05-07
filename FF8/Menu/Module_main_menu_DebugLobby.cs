using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FF8
{
    internal static partial class Module_main_menu_debug
    {
        #region Fields

        private static int debug_choosedBS, debug_choosedAudio, debug_fieldPointer, debug_moviePointer;

        private static string debug_choosedField, debug_choosedMovie, debug_choosedMusic;

        private static Ditems s_dchoose;

        /// <summary>
        /// Strings for the debug menu
        /// </summary>
        private static Dictionary<Enum, Item> strDebugLobby;

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
            Icons,
            Cards
        }

        #endregion Enums

        #region Properties

        public static Vector2 DFontPos { get; private set; }

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
                if(Module_movie_test.Movies.Count>0)
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

        private static Vector2 Offset { get; set; }

        #endregion Properties

        #region Methods

        private static Rectangle FontBoxCalc<T>(Dictionary<Enum, Item> dict)
        {
            Rectangle dst = new Rectangle();
            int item = 0;
            foreach (Enum i in Enum.GetValues(typeof(T)))
            {
                Item c = dict[i];
                c.Loc = Memory.font.RenderBasicText(FF8String.Combine(strDebugLobby[i].Text, InfoForLobby<Ditems>(i)),
                (int)DFontPos.X, (int)(DFontPos.Y + vpSpace * item++), 2.545454545f, 3.0375f,skipdraw:true);
                if (dst.X == 0 || dst.Y == 0)
                    dst.Location = c.Loc.Location;
                if (c.Loc.Width > dst.Width)
                    dst.Width = c.Loc.Width;
                dst.Height = c.Loc.Y + c.Loc.Height - dst.Y;
                dict[i] = c;
            }
            
            dst.Inflate(vp_per.X * .06f , vp_per.Y * .035f );
            return dst;
        }
        /// <summary>
        /// Draw Debug Menu
        /// </summary>
        private static void DrawDebugLobby()
        {
            
            vpSpace = vp_per.Y * 0.05f ;
            float item = 0;
            Rectangle dst = FontBoxCalc<Ditems>(strDebugLobby);
            Memory.SpriteBatchStartAlpha(SamplerState.PointClamp);
            DrawBox(dst, null, null);
            //Memory.Icons.Draw(Icons.ID.Menu_BG_256, 0, dst, new Vector2(2f), Fade);
            item = 0;
            dst.Offset(4 * 3.5f, 0);
            dst.Size = (Memory.Icons[Icons.ID.DEBUG].GetRectangle.Size.ToVector2()  * 3.5f).ToPoint();
            Memory.Icons.Draw(Icons.ID.DEBUG, 2, dst, Vector2.Zero, fade);
            dst.Location = DFontPos.ToPoint();
            dst.Size = new Point((int)(24 * 2 ), (int)(16 * 2 ));
            dst.Offset(-(dst.Width + 10 ), 6  + vpSpace * ((float)Dchoose));
            //want to use new function brain hurts on this screen lol
            //DrawPointer(new Point((int)(textStart.X), (int)((((textSize.Y + vpSpace) * (float)Mchoose) + textStart.Y + (6 * textSize.Y)) )));
            Memory.Icons.Draw(Icons.ID.Finger_Right, 2, dst, Vector2.Zero, fade);
            //Memory.SpriteBatchEnd();
            //pointclamp looks bad on default fonts.
            //Memory.SpriteBatchStartAlpha();
            foreach (Ditems i in (Ditems[])Enum.GetValues(typeof(Ditems)))
            {
                Memory.font.RenderBasicText(FF8String.Combine(strDebugLobby[i].Text, InfoForLobby<Ditems>(i)),
                    (int)(DFontPos.X), (int)(DFontPos.Y + vpSpace * item++), 2.545454545f, 3.0375f, 0, Fade, lineSpacing: 1);
            }
            //Memory.spriteBatch.Draw(Memory.iconsTex[2], dst,
            //    new Rectangle(232, 0, 23, 15), Color.White * Fade);
            Memory.SpriteBatchEnd();
        }

        /// <summary>
        /// Dynamic info for Ditem that is read at draw time.
        /// </summary>
        /// <param name="i">Ditem being drawn</param>
        /// <returns>Dynamic info for Ditem</returns>
        private static string InfoForLobby<T>(Enum i)
        {
            switch (typeof(T).Name)
            {
                case "Ditems":
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
                    break;
            };
            return "";
        }

        private static void InitDebug()
        {
            strDebugLobby = new Dictionary<Enum, Item>()
            {
                { Ditems.Reset, new Item{Text=new FF8String("Reset Main Menu state") } },
                { Ditems.Overture, new Item{Text=new FF8String("Play Overture")} },
                { Ditems.Battle, new Item{Text=new FF8String("Battle encounter: ")} },
                { Ditems.Field, new Item{Text=new FF8String("Field debug render: ")} },
                { Ditems.Movie, new Item{Text=new FF8String("Movie debug render: ")} },
                { Ditems.Music, new Item{Text=new FF8String("Play/Stop music: ")} },
                { Ditems.Sounds, new Item{Text=new FF8String("Play audio.dat: ")} },
                { Ditems.World, new Item{Text=new FF8String("Jump to World Map")} },
                { Ditems.Faces, new Item{Text=new FF8String("Test Faces")} },
                { Ditems.Icons, new Item{Text=new FF8String("Test Icons")} },
                { Ditems.Cards, new Item{Text=new FF8String("Test Cards")} },
            };
            debug_choosedField = Memory.FieldHolder.fields[debug_fieldPointer];
            if(Module_movie_test.Movies.Count>0)
                debug_choosedMovie = Path.GetFileNameWithoutExtension(Module_movie_test.Movies[debug_moviePointer]);

            if(Memory.dicMusic.Count>0 && Memory.dicMusic[0].Count>0)
                debug_choosedMusic = Path.GetFileNameWithoutExtension(Memory.dicMusic[0][0]);

            debug_fieldPointer = 90;

            debug_moviePointer = 0;
    }

        /// <summary>
        /// Update Debug Menu
        /// </summary>
        /// <returns>true on change</returns>
        private static bool UpdateDebugLobby()
        {
            bool ret = false;
            Point ml = Input.MouseLocation;
            foreach (KeyValuePair<Enum, Item> entry in strDebugLobby)
            {
                if (entry.Value.Loc.Contains(ml))
                {
                    Dchoose = (Ditems)entry.Key;
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

                    if (Memory.dicMusic.Count > 0)
                    {
                        if (Memory.MusicIndex <= ushort.MinValue)
                        {
                            Memory.MusicIndex = ushort.MaxValue;
                        }
                        else
                        {
                            Memory.MusicIndex--;
                        }
                        debug_choosedMusic = Path.GetFileNameWithoutExtension(Memory.dicMusic[Memory.MusicIndex][0]);
                    }
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

                case Ditems.Icons:
                    Fade = 0.0f;
                    Memory.module = Memory.MODULE_ICON_TEST;
                    break;

                case Ditems.Cards:
                    Fade = 0.0f;
                    Memory.module = Memory.MODULE_CARD_TEST;
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
                    if (Memory.dicMusic.Count > 0)
                    {
                        if (Memory.MusicIndex >= ushort.MaxValue || Memory.MusicIndex >= Memory.dicMusic.Keys.Max())
                        {
                            Memory.MusicIndex = 0;
                        }
                        else
                        {
                            Memory.MusicIndex++;
                        }
                        debug_choosedMusic = Path.GetFileNameWithoutExtension(Memory.dicMusic[Memory.MusicIndex][0]);
                    }
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

        #endregion Methods
    }
}
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;

namespace OpenVIII.IGMData
{
    public class DebugChoose : Base
    {
        #region Fields

        /// <summary>
        /// Strings
        /// </summary>
        private static Dictionary<Ditems, FF8String> strDebugLobby = new Dictionary<Ditems, FF8String>()
        {
            { Ditems.Reset, new FF8String("Reset Main Menu state") },
            { Ditems.Overture, new FF8String("Play Overture") },
            { Ditems.Battle, new FF8String("Battle encounter: ") },
            { Ditems.Field, new FF8String("Field debug render: ") },
            { Ditems.Movie, new FF8String("Movie debug render: ") },
            { Ditems.Music, new FF8String("Play/Stop music: ") },
            { Ditems.Sounds, new FF8String("Play audio.dat: ") },
            { Ditems.World, new FF8String("Jump to World Map") },
            { Ditems.Faces, new FF8String("Test Faces") },
            { Ditems.Icons, new FF8String("Test Icons") },
            { Ditems.Cards, new FF8String("Test Cards") },
        };

        private int debug_choosedAudio;

        /// <summary>
        /// Dynamic String Values
        /// </summary>
        private Dictionary<Ditems, Func<FF8String>> dynamicDebugStrings;

        private Dictionary<Ditems, Func<bool>> inputsOKAY;

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
            Cards,

            /// <summary>
            /// Number of values. Make sure this is last.
            /// </summary>
            Count
        }

        #endregion Enums

        #region Methods

        public static DebugChoose Create(Rectangle pos) => Create<DebugChoose>((int)Ditems.Count, 1, new IGMDataItem.Box { Pos = pos }, 1, (int)Ditems.Count);

        public override bool Inputs()
        {
            Cursor_Status |= Cursor_Status.Enabled;
            Cursor_Status &= ~Cursor_Status.Blinking;
            return base.Inputs();
        }

        public override bool Inputs_CANCEL()
        {
            base.Inputs_CANCEL();
            CURSOR_SELECT = 0;
            Module_main_menu_debug.State = Module_main_menu_debug.MainMenuStates.MainLobby;
            Menu.FadeIn();
            return true;
        }

        public override bool Inputs_OKAY()
        {
            if (inputsOKAY.TryGetValue((Ditems)CURSOR_SELECT, out Func<bool> f))
            {
                return f.Invoke() && base.Inputs_OKAY();
            }
            return false;
        }

        public override void Refresh()
        {
            base.Refresh();
            for (int i = 0; i < Count; i++)
            {
                if (dynamicDebugStrings.TryGetValue((Ditems)i, out Func<FF8String> f))
                {
                    ((IGMDataItem.Text)ITEM[i, 0]).Data = f();
                }
            }
        }

        protected override void Init()
        {
            base.Init();
            for (int i = 0; i < Count; i++)
            {
                if (strDebugLobby.TryGetValue((Ditems)i, out FF8String str))
                {
                    ITEM[i, 0] = new IGMDataItem.Text { Data = str, Pos = SIZE[i] };
                }
            }
            inputsOKAY = new Dictionary<Ditems, Func<bool>>()
            {
                { Ditems.Reset, ()=> {
                    return Inputs_CANCEL();
                } },
                { Ditems.Overture, ()=> {
                    Menu.FadeIn();
                    Module_main_menu_debug.State = Module_main_menu_debug.MainMenuStates.MainLobby;
                    Module_overture_debug.ResetModule();
                    Memory.module = MODULE.OVERTURE_DEBUG;
                    Memory.IsMouseVisible = false;
                    init_debugger_Audio.StopMusic();
                    return true;
                } },
                { Ditems.Battle, ()=> {
                    Menu.FadeIn();
                    Module_battle_debug.ResetState();
                    Memory.module = MODULE.BATTLE_DEBUG;
                    Memory.IsMouseVisible = false;
                    return true;
                } },
                { Ditems.Field, ()=> {
                    Menu.FadeIn();
                    Module_field_debug.ResetField();
                    Memory.module = MODULE.FIELD_DEBUG;
                    Memory.IsMouseVisible = false;
                    return true;
                }  },
                { Ditems.Movie, ()=> {
                    Menu.FadeIn();
                    Memory.module = MODULE.MOVIETEST;
                    Module_movie_test.MovieState = 0;
                    Memory.IsMouseVisible = false;
                    return true;
                }  },
                { Ditems.Music, ()=> {
                    Module_field_debug.ResetField();
                    init_debugger_Audio.PlayStopMusic();
                    return true;
                }  },
                { Ditems.Sounds, ()=> {
                    init_debugger_Audio.PlaySound(debug_choosedAudio);
                    return true;
                }  },
                { Ditems.World, ()=> {
                    Memory.module = MODULE.WORLD_DEBUG;
                    Memory.IsMouseVisible = false;
                    return true;
                }  },
                { Ditems.Faces, ()=> {
                    Memory.module = MODULE.FACE_TEST;
                    Module_face_test.Show();
                    return true;
                }  },
                { Ditems.Icons, ()=> {
                    Memory.module = MODULE.ICON_TEST;
                    Module_icon_test.Show();
                    return true;
                }  },
                { Ditems.Cards, ()=> {
                    Memory.module = MODULE.CARD_TEST;
                    Module_card_test.Show();
                    return false;
                }  },
            };
            dynamicDebugStrings = new Dictionary<Ditems, Func<FF8String>>
            {
                { Ditems.Battle, ()=> {return strDebugLobby[Ditems.Battle].Clone().Append(Memory.battle_encounter.ToString("D4")); } },
                { Ditems.Field, ()=> {return strDebugLobby[Ditems.Field].Clone().Append(Memory.FieldHolder.FieldID.ToString("D4")); } },
                { Ditems.Movie, ()=> {return strDebugLobby[Ditems.Movie].Clone().Append(Path.GetFileNameWithoutExtension(Module_movie_test.Movies[Module_movie_test.Index])); } },
                { Ditems.Music, ()=> {return strDebugLobby[Ditems.Music].Clone().Append(Path.GetFileNameWithoutExtension(Memory.dicMusic[Memory.MusicIndex][0])); } },
                { Ditems.Sounds, ()=> {return strDebugLobby[Ditems.Sounds].Clone().Append(debug_choosedAudio.ToString("D4")); } }
            };
        }

        protected override void InitShift(int i, int col, int row)
        {
            base.InitShift(i, col, row);
            SIZE[i].Inflate(-22, -8);
            //SIZE[i].Offset(0, 12 + (-8 * row));
        }

        #endregion Methods
    }
}
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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
            { Ditems.FieldModelTest, new FF8String("Test field models") },
        };

        private int debug_choosedAudio;

        /// <summary>
        /// Dynamic String Values
        /// </summary>
        private Dictionary<Ditems, Func<FF8String>> dynamicDebugStrings;

        private Dictionary<Ditems, Func<bool>> inputsLeft;
        private Dictionary<Ditems, Func<bool>> inputsOKAY;
        private Dictionary<Ditems, Func<bool>> inputsRight;

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
            FieldModelTest,

            /// <summary>
            /// Number of values. Make sure this is last.
            /// </summary>
            Count
        }

        #endregion Enums

        #region Methods

        public static DebugChoose Create(Rectangle pos) => Create<DebugChoose>((int)Ditems.Count + 1, 1, new IGMDataItem.Box { Pos = pos, Title = Icons.ID.DEBUG }, 1, (int)Ditems.Count);

        public override bool Inputs()
        {
            Cursor_Status |= Cursor_Status.Enabled; //Cursor_Status |= Cursor_Status.Horizontal;
            if (ITEM[Count - 1, 0].Enabled)
            {
                Cursor_Status |= Cursor_Status.Blinking;
                return ITEM[Count - 1, 0].Inputs();
            }
            else
            {
                Cursor_Status &= ~Cursor_Status.Blinking;
                return base.Inputs();
            }
        }

        public override bool Inputs_CANCEL()
        {
            base.Inputs_CANCEL();
            CURSOR_SELECT = 0;
            Menu.Module.State = MenuModule.Mode.MainLobby;
            Menu.FadeIn();
            return true;
        }

        public override void Inputs_Left()
        {
            if (inputsLeft.TryGetValue((Ditems)CURSOR_SELECT, out Func<bool> f) && f.Invoke())
            {
                base.Inputs_Left();
                Refresh();
            }
        }

        public override bool Inputs_OKAY()
        {
            if (inputsOKAY.TryGetValue((Ditems)CURSOR_SELECT, out Func<bool> f))
            {
                return f.Invoke() && base.Inputs_OKAY();
            }
            return false;
        }

        public override void Inputs_Right()
        {
            if (inputsRight.TryGetValue((Ditems)CURSOR_SELECT, out Func<bool> f) && f.Invoke())
            {
                base.Inputs_Right();
                Refresh();
            }
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
            foreach (int i in Enumerable.Range(0, (int)Ditems.Count))
            {
                if (strDebugLobby.TryGetValue((Ditems)i, out FF8String str))
                {
                    ITEM[i, 0] = new IGMDataItem.Text { Data = str, Pos = SIZE[i] };
                }
            }
            ITEM[Count - 1, 0] = DebugSelectPool<Battle.Encounter>.Create(CONTAINER.Pos, Memory.Encounters, SetEncounterOKAYBattle);
            ITEM[Count - 1, 0].Refresh();
            PointerZIndex = Count - 1;
            inputsOKAY = new Dictionary<Ditems, Func<bool>>()
            {
                { Ditems.Reset, ()=> {
                    return Inputs_CANCEL();
                } },
                { Ditems.Overture, ()=> {
                    Menu.FadeIn();
                    Menu.Module.State = MenuModule.Mode.MainLobby;
                    Module_overture_debug.ResetModule();
                    Memory.Module = MODULE.OVERTURE_DEBUG;
                    Memory.IsMouseVisible = false;
                    init_debugger_Audio.StopMusic();
                    return true;
                } },
                { Ditems.Battle, ()=> {
                    //Menu.FadeIn();
                    //Module_battle_debug.ResetState();
                    //Menu.BattleMenus.CameFrom();
                    //Memory.Module = MODULE.BATTLE_DEBUG;
                    ////Extended.postBackBufferDelegate = BattleSwirl.Init;
                    ////Extended.RequestBackBuffer();
                    //Memory.IsMouseVisible = false;
                    ITEM[Count-1,0].Show();
                    return true;
                } },
                { Ditems.Field, ()=> {
                    Menu.FadeIn();
                    Fields.Module.ResetField();
                    Memory.Module = MODULE.FIELD_DEBUG;
                    Memory.IsMouseVisible = false;
                    return true;
                }  },
                { Ditems.Movie, ()=> {
                    Menu.FadeIn();
                    Memory.Module = MODULE.MOVIETEST;
                    Module_movie_test.Play();
                    Memory.IsMouseVisible = false;
                    return true;
                }  },
                { Ditems.Music, ()=> {
                    Fields.Module.ResetField();
                    init_debugger_Audio.PlayStopMusic();
                    return true;
                }  },
                { Ditems.Sounds, ()=> {
                    init_debugger_Audio.PlaySound(debug_choosedAudio);
                    skipsnd = true;
                    return true;
                }  },
                { Ditems.World, ()=> {
                    Menu.FadeIn();
                    Memory.Module = MODULE.WORLD_DEBUG;
                    Memory.IsMouseVisible = false;
                    return true;
                }  },
                { Ditems.Faces, ()=> {
                    Menu.FadeIn();
                    Memory.Module = MODULE.FACE_TEST;
                    Module_face_test.Show();
                    return true;
                }  },
                { Ditems.Icons, ()=> {
                    Menu.FadeIn();
                    Memory.Module = MODULE.ICON_TEST;
                    Module_icon_test.Show();
                    return true;
                }  },
                { Ditems.Cards, ()=> {
                    Menu.FadeIn();
                    Memory.Module = MODULE.CARD_TEST;
                    Module_card_test.Show();
                    return true;
                }  },
                { Ditems.FieldModelTest, ()=> {
                    Menu.FadeIn();
                    Memory.Module = MODULE.FIELD_MODEL_TEST;
                    Module_card_test.Show();
                    return true;
                }  },
            };
            bool SetEncounterOKAYBattle(Battle.Encounter encounter)
            {
                Memory.Encounters.ID = encounter.ID;
                Menu.FadeIn();
                Module_battle_debug.ResetState();
                Menu.BattleMenus.CameFrom();
                Memory.Module = MODULE.BATTLE_DEBUG;
                //Extended.postBackBufferDelegate = BattleSwirl.Init;
                //Extended.RequestBackBuffer();
                Memory.IsMouseVisible = false;
                return true;
            }
            inputsLeft = new Dictionary<Ditems, Func<bool>>()
            {
                { Ditems.Battle, ()=> {
                    Memory.Encounters.Previous();
                    return true;
                } },
                { Ditems.Field, ()=> {
                    if( Memory.FieldHolder.FieldID>0)
                         Memory.FieldHolder.FieldID--;
                    else
                        Memory.FieldHolder.FieldID = checked((ushort)((Memory.FieldHolder.fields?.Length??1) - 1));
                    return true;
                }  },
                { Ditems.Movie, ()=> {
                    if(Module_movie_test.Index>0)
                        Module_movie_test.Index--;
                    else
                        Module_movie_test.Index = Movie.Files.Count - 1;
                    return true;
                }  },
                { Ditems.Music, ()=> {
                    if(Memory.MusicIndex >0)
                        Memory.MusicIndex --;
                    else
                        Memory.MusicIndex = Memory.dicMusic.Keys.Max();
                    return true;
                }  },
                { Ditems.Sounds, ()=> {
                    if (debug_choosedAudio > 0)
                        debug_choosedAudio--;
                    else
                        debug_choosedAudio = init_debugger_Audio.soundEntriesCount-1;
                    return true;
                }  }
            };

            inputsRight = new Dictionary<Ditems, Func<bool>>()
            {
                { Ditems.Battle, ()=> {
                    Memory.Encounters.Next();
                    return true;
                } },
                { Ditems.Field, ()=> {
                    if( Memory.FieldHolder.FieldID<checked((ushort)((Memory.FieldHolder.fields?.Length??1) - 1)))
                         Memory.FieldHolder.FieldID++;
                    else
                        Memory.FieldHolder.FieldID = 0;
                    return true;
                }  },
                { Ditems.Movie, ()=> {
                    if(Module_movie_test.Index<Movie.Files.Count - 1)
                        Module_movie_test.Index++;
                    else
                        Module_movie_test.Index = 0;
                    return true;
                }  },
                { Ditems.Music, ()=> {
                    if(Memory.MusicIndex <Memory.dicMusic.Keys.Max())
                        Memory.MusicIndex ++;
                    else
                        Memory.MusicIndex = 0;
                    return true;
                }  },
                { Ditems.Sounds, ()=> {
                    if (debug_choosedAudio < init_debugger_Audio.soundEntriesCount-1)
                        debug_choosedAudio++;
                    else
                        debug_choosedAudio = 0;
                    return true;
                }  }
            };

            dynamicDebugStrings = new Dictionary<Ditems, Func<FF8String>>
            {
                { Ditems.Battle, ()=> {
                    string end=$"{Memory.Encounters.ID.ToString("D4")} - {Memory.Encounters.Filename.ToUpper()}";
                    if(strDebugLobby[Ditems.Battle]!=null)
                        return strDebugLobby[Ditems.Battle].Clone().Append(end);
                    else
                        return end; } },
                { Ditems.Field, ()=> {
                    string end=$"{Memory.FieldHolder.FieldID.ToString("D3")} - {Memory.FieldHolder.GetString()?.ToUpper()}";
                    if(strDebugLobby[Ditems.Field]!= null)
                        return strDebugLobby[Ditems.Field].Clone().Append(end);
                    else
                        return end; } },
                { Ditems.Movie, ()=> {
                    if (Movie.Files.Count<=Module_movie_test.Index)
                        Module_movie_test.Index=0;
                    if(Movie.Files.Count ==0)
                        return "";
                    Movie.Files Files;
                    if(Movie.Files.Count >Module_movie_test.Index)
                    {
                        string end=Path.GetFileNameWithoutExtension(Files[Module_movie_test.Index]);
                        if(strDebugLobby[Ditems.Movie]!=null)
                            return strDebugLobby[Ditems.Movie].Clone().Append(end);
                        else
                            return end;
                    }
                    return ""; }},
                { Ditems.Music, ()=> {
                    if(Memory.dicMusic.Count > Memory.MusicIndex && Memory.dicMusic[Memory.MusicIndex].Count>0)
                    {
                        string end=Path.GetFileNameWithoutExtension(Memory.dicMusic[Memory.MusicIndex][0]);
                        if(strDebugLobby[Ditems.Music]!=null)
                            return strDebugLobby[Ditems.Music].Clone().Append(end);
                        else
                            return end;
                    }
                    return "";
                } },
                { Ditems.Sounds, ()=> {return strDebugLobby[Ditems.Sounds].Clone().Append(debug_choosedAudio.ToString("D4")); } }
            };
        }

        protected override void InitShift(int i, int col, int row)
        {
            base.InitShift(i, col, row);
            SIZE[i].Inflate(-22, -8);
            SIZE[i].Offset(0, 12 + (-8 * row));
        }

        #endregion Methods
    }
}
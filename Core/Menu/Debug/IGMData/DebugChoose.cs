using Microsoft.Xna.Framework;
using OpenVIII.Movie;
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
        private static readonly Dictionary<DebugItems, FF8String> StrDebugLobby = new Dictionary<DebugItems, FF8String>()
        {
            { DebugItems.Reset, new FF8String("Reset Main Menu state") },
            { DebugItems.Overture, new FF8String("Play Overture") },
            { DebugItems.Battle, new FF8String("Battle encounter") },
            { DebugItems.Field, new FF8String("Field debug render") },
            { DebugItems.Movie, new FF8String("Movie debug render: ") },
            { DebugItems.Music, new FF8String("Play/Stop music: ") },
            { DebugItems.Sounds, new FF8String("Play audio.dat: ") },
            { DebugItems.World, new FF8String("Jump to World Map") },
            { DebugItems.Faces, new FF8String("Test Faces") },
            { DebugItems.Icons, new FF8String("Test Icons") },
            { DebugItems.Cards, new FF8String("Test Cards") },
            { DebugItems.FieldModelTest, new FF8String("Test field models") },
        };

        private int _debugChosenAudio;

        /// <summary>
        /// Dynamic String Values
        /// </summary>
        private Dictionary<DebugItems, Func<FF8String>> _dynamicDebugStrings;

        private Dictionary<DebugItems, Func<bool>> _inputsLeft;
        private Dictionary<DebugItems, Func<bool>> _inputsOkay;
        private Dictionary<DebugItems, Func<bool>> _inputsRight;

        #endregion Fields

        #region Enums

        /// <summary>
        /// Identifiers and Ordering of debug menu items
        /// </summary>
        private enum DebugItems
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
            Count,

            BattlePool = Count,
            FieldPool = Count + 1,
        }

        #endregion Enums

        #region Methods

        public static DebugChoose Create(Rectangle pos) => Create<DebugChoose>((int)DebugItems.Count + 2, 1, new IGMDataItem.Box { Pos = pos, Title = Icons.ID.DEBUG }, 1, (int)DebugItems.Count);

        public override bool Inputs()
        {
            Cursor_Status |= Cursor_Status.Enabled; //Cursor_Status |= Cursor_Status.Horizontal;
            if (ITEM[(int)DebugItems.BattlePool, 0].Enabled)
            {
                Cursor_Status |= Cursor_Status.Blinking;
                return ITEM[(int)DebugItems.BattlePool, 0].Inputs();
            }
            else if (ITEM[(int)DebugItems.FieldPool, 0].Enabled)
            {
                Cursor_Status |= Cursor_Status.Blinking;
                return ITEM[(int)DebugItems.FieldPool, 0].Inputs();
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
            if (_inputsLeft.TryGetValue((DebugItems)CURSOR_SELECT, out Func<bool> f) && f.Invoke())
            {
                base.Inputs_Left();
                Refresh();
            }
        }

        public override bool Inputs_OKAY()
        {
            if (_inputsOkay.TryGetValue((DebugItems)CURSOR_SELECT, out Func<bool> f))
            {
                return f.Invoke() && base.Inputs_OKAY();
            }
            return false;
        }

        public override void Inputs_Right()
        {
            if (_inputsRight.TryGetValue((DebugItems)CURSOR_SELECT, out Func<bool> f) && f.Invoke())
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
                if (_dynamicDebugStrings.TryGetValue((DebugItems)i, out Func<FF8String> f))
                {
                    ((IGMDataItem.Text)ITEM[i, 0]).Data = f();
                }
            }
        }

        protected override void Init()
        {
            base.Init();
            foreach (int i in Enumerable.Range(0, (int)DebugItems.Count))
            {
                if (StrDebugLobby.TryGetValue((DebugItems)i, out FF8String str))
                {
                    ITEM[i, 0] = new IGMDataItem.Text { Data = str, Pos = SIZE[i] };
                }
            }
            Rectangle rect = CONTAINER.Pos;
            rect.Inflate(-12, -60);
            rect.Offset(12, 60);
            ITEM[(int)DebugItems.BattlePool, 0] = DebugSelectPool<Battle.Encounter>.Create(rect, Memory.Encounters, SetEncounterOkayBattle, FilterEncounters);
            ITEM[(int)DebugItems.BattlePool, 0].Refresh();
            ITEM[(int)DebugItems.FieldPool, 0] = DebugSelectPool<string>.Create(rect, Memory.FieldHolder.fields, SetFieldsOkayBattle, FilterFields,4);
            ITEM[(int)DebugItems.FieldPool, 0].Refresh();
            PointerZIndex = Count - 1;
            _inputsOkay = new Dictionary<DebugItems, Func<bool>>()
            {
                { DebugItems.Reset, Inputs_CANCEL },
                { DebugItems.Overture, ()=> {
                    Menu.FadeIn();
                    Menu.Module.State = MenuModule.Mode.MainLobby;
                    Module_overture_debug.ResetModule();
                    Memory.Module = MODULE.OVERTURE_DEBUG;
                    Memory.IsMouseVisible = false;
                    AV.Music.Stop();
                    return true;
                } },
                { DebugItems.Battle, ()=> {
                    ITEM[(int)DebugItems.BattlePool,0].Show();
                    return true;
                } },
                { DebugItems.Field, ()=> {
                    ITEM[(int)DebugItems.FieldPool,0].Show();
                    return true;
                }  },
                { DebugItems.Movie, ()=> {
                    Menu.FadeIn();
                    Memory.Module = MODULE.MOVIETEST;
                    ModuleMovieTest.Play();
                    Memory.IsMouseVisible = false;
                    return true;
                }  },
                { DebugItems.Music, ()=> {
                    Fields.Module.ResetField();
                    AV.Music.PlayStop();
                    return true;
                }  },
                { DebugItems.Sounds, ()=> {
                    AV.Sound.Play(_debugChosenAudio);
                    skipsnd = true;
                    return true;
                }  },
                { DebugItems.World, ()=> {
                    Menu.FadeIn();
                    Module_world_debug.playerPosition = new Vector3(-9105f, 30f, -4466); //reset for sake of debugging
                    Memory.Module = MODULE.WORLD_DEBUG;
                    Memory.IsMouseVisible = false;
                    return true;
                }  },
                { DebugItems.Faces, ()=> {
                    Menu.FadeIn();
                    Memory.Module = MODULE.FACE_TEST;
                    Module_face_test.Show();
                    return true;
                }  },
                { DebugItems.Icons, ()=> {
                    Menu.FadeIn();
                    Memory.Module = MODULE.ICON_TEST;
                    Module_icon_test.Show();
                    return true;
                }  },
                { DebugItems.Cards, ()=> {
                    Menu.FadeIn();
                    Memory.Module = MODULE.CARD_TEST;
                    Module_card_test.Show();
                    return true;
                }  },
                { DebugItems.FieldModelTest, ()=> {
                    Menu.FadeIn();
                    Memory.Module = MODULE.FIELD_MODEL_TEST;
                    Module_card_test.Show();
                    return true;
                }  },
            };
            Files files = Files.Instance;
            _inputsLeft = new Dictionary<DebugItems, Func<bool>>()
            {
                { DebugItems.Movie, ()=> {
                    if(ModuleMovieTest.Index>0)
                        ModuleMovieTest.Index--;
                    else
                        ModuleMovieTest.Index = files.Count - 1;
                    return true;
                }  },
                { DebugItems.Music, ()=> {
                    if(Memory.MusicIndex >0)
                        Memory.MusicIndex --;
                    else
                        Memory.MusicIndex = (ushort)Memory.dicMusic.Keys.Max();
                    return true;
                }  },
                { DebugItems.Sounds, ()=> {
                    if (_debugChosenAudio > 0)
                        _debugChosenAudio--;
                    else
                        _debugChosenAudio = AV.Sound.EntriesCount-1;
                    return true;
                }  }
            };

            _inputsRight = new Dictionary<DebugItems, Func<bool>>()
            {
                { DebugItems.Movie, ()=> {
                    if(ModuleMovieTest.Index<files.Count - 1)
                        ModuleMovieTest.Index++;
                    else
                        ModuleMovieTest.Index = 0;
                    return true;
                }  },
                { DebugItems.Music, ()=> {
                    if(Memory.MusicIndex <(ushort)Memory.dicMusic.Keys.Max())
                        Memory.MusicIndex ++;
                    else
                        Memory.MusicIndex = 0;
                    return true;
                }  },
                { DebugItems.Sounds, ()=> {
                    if (_debugChosenAudio < AV.Sound.EntriesCount-1)
                        _debugChosenAudio++;
                    else
                        _debugChosenAudio = 0;
                    return true;
                }  }
            };

            _dynamicDebugStrings = new Dictionary<DebugItems, Func<FF8String>>
            {
                { DebugItems.Movie, ()=> {
                    if (files.Count<=ModuleMovieTest.Index)
                        ModuleMovieTest.Index=0;
                    if(files.Count ==0)
                        return "";
                    if (files.Count <= ModuleMovieTest.Index) return "";
                    string end=Path.GetFileNameWithoutExtension(files[ModuleMovieTest.Index]);
                    if(StrDebugLobby[DebugItems.Movie]!=null)
                        return StrDebugLobby[DebugItems.Movie].Clone().Append(end);
                    return end;
                }},
                { DebugItems.Music, ()=> {
                    if (Memory.dicMusic.Count <= Memory.MusicIndex ||
                        Memory.dicMusic[(MusicId) Memory.MusicIndex].Count <= 0) return "";
                    string end=Path.GetFileNameWithoutExtension(Memory.dicMusic[(MusicId)Memory.MusicIndex][0]);
                    if(StrDebugLobby[DebugItems.Music]!=null)
                        return StrDebugLobby[DebugItems.Music].Clone().Append(end);
                    return end;
                } },
                { DebugItems.Sounds, ()=> StrDebugLobby[DebugItems.Sounds].Clone().Append(_debugChosenAudio.ToString("D4"))}
            };
        }

        protected override void InitShift(int i, int col, int row)
        {
            base.InitShift(i, col, row);
            SIZE[i].Inflate(-22, -8);
            SIZE[i].Offset(0, 12 + (-8 * row));
        }

        private void FilterEncounters(string filter) => ((DebugSelectPool<Battle.Encounter>)ITEM[(int)DebugItems.BattlePool, 0]).Refresh(Memory.Encounters.Where(x => x.ToString().IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0));

        private void FilterFields(string filter) => ((DebugSelectPool<string>)ITEM[(int)DebugItems.FieldPool, 0]).Refresh(Memory.FieldHolder.fields?.Where(x => x.ToString().IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0));

        private static bool SetEncounterOkayBattle(Battle.Encounter encounter)
        {
            Memory.Encounters.ID = encounter.ID;
            Menu.FadeIn();
            Module_battle_debug.ResetState();
            Menu.BattleMenus.CameFrom();
            Memory.Module = MODULE.BATTLE_DEBUG;
            Memory.IsMouseVisible = false;
            return true;
        }
        private static bool SetFieldsOkayBattle(string arg)
        {
            if (Memory.FieldHolder.fields == null) return true;
            Memory.FieldHolder.FieldID = (ushort)Memory.FieldHolder.fields.ToList().FindIndex(x => x == arg);
            Menu.FadeIn();
            Fields.Module.ResetField();
            Memory.Module = MODULE.FIELD_DEBUG;
            Memory.IsMouseVisible = false;
            return true;
        }

        #endregion Methods

    }
}
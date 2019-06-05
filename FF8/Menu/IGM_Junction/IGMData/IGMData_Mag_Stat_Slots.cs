using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace FF8
{
    public partial class Module_main_menu_debug
    {
        private partial class IGM_Junction
        {
            private class IGMData_Mag_Stat_Slots : IGMData
            {
                public IGMData_Mag_Stat_Slots() : base( 10, 5, new IGMDataItem_Box(pos: new Rectangle(0, 414, 840, 216)), 2, 5)
                {
                }

                /// <summary>
                /// Convert stat to correct icon id.
                /// </summary>
                private static Dictionary<Kernel_bin.Stat, Icons.ID> Stat2Icon = new Dictionary<Kernel_bin.Stat, Icons.ID>
                {
                    { Kernel_bin.Stat.HP, Icons.ID.Stats_Hit_Points },
                    { Kernel_bin.Stat.STR, Icons.ID.Stats_Strength },
                    { Kernel_bin.Stat.VIT, Icons.ID.Stats_Vitality },
                    { Kernel_bin.Stat.MAG, Icons.ID.Stats_Magic },
                    { Kernel_bin.Stat.SPR, Icons.ID.Stats_Spirit },
                    { Kernel_bin.Stat.SPD, Icons.ID.Stats_Speed },
                    { Kernel_bin.Stat.EVA, Icons.ID.Stats_Evade },
                    { Kernel_bin.Stat.LUCK, Icons.ID.Stats_Luck },
                    { Kernel_bin.Stat.HIT, Icons.ID.Stats_Hit_Percent },
                };

                public new Saves.CharacterData PrevSetting { get; private set; }
                public new Saves.CharacterData Setting { get; private set; }
                public Kernel_bin.Stat[] Contents { get; private set; }

                /// <summary>
                /// Things that may of changed before screen loads or junction is changed.
                /// </summary>
                public override void ReInit()
                {
                    if (Memory.State.Characters != null)
                    {
                        Setting = Memory.State.Characters[Character];
                        Contents = new Kernel_bin.Stat[Count];
                        Contents = Array.ConvertAll(Contents, c => c = Kernel_bin.Stat.None);
                        base.ReInit();

                        if (Memory.State.Characters != null)
                        {
                            List<Kernel_bin.Abilities> unlocked = Setting.UnlockedGFAbilities;
                            ITEM[5, 0] = new IGMDataItem_Icon(Icons.ID.Icon_Status_Attack, new Rectangle(SIZE[5].X + 200, SIZE[5].Y, 0, 0),
                                (byte)(unlocked.Contains(Kernel_bin.Abilities.ST_Atk_J) ? 2 : 7));
                            ITEM[5, 1] = new IGMDataItem_Icon(Icons.ID.Icon_Status_Defense, new Rectangle(SIZE[5].X + 240, SIZE[5].Y, 0, 0),
                                (byte)(unlocked.Contains(Kernel_bin.Abilities.ST_Def_Jx1) ||
                                unlocked.Contains(Kernel_bin.Abilities.ST_Def_Jx2) ||
                                unlocked.Contains(Kernel_bin.Abilities.ST_Def_Jx4) ? 2 : 7));
                            ITEM[5, 2] = new IGMDataItem_Icon(Icons.ID.Icon_Elemental_Attack, new Rectangle(SIZE[5].X + 280, SIZE[5].Y, 0, 0),
                                (byte)(unlocked.Contains(Kernel_bin.Abilities.Elem_Atk_J) ? 2 : 7));
                            ITEM[5, 3] = new IGMDataItem_Icon(Icons.ID.Icon_Elemental_Defense, new Rectangle(SIZE[5].X + 320, SIZE[5].Y, 0, 0),
                                (byte)(unlocked.Contains(Kernel_bin.Abilities.Elem_Def_Jx1) ||
                                unlocked.Contains(Kernel_bin.Abilities.Elem_Def_Jx2) ||
                                unlocked.Contains(Kernel_bin.Abilities.Elem_Def_Jx4) ? 2 : 7));
                            BLANKS[5] = true;
                            foreach (Kernel_bin.Stat stat in (Kernel_bin.Stat[])Enum.GetValues(typeof(Kernel_bin.Stat)))
                            {
                                if (Stat2Icon.ContainsKey(stat))
                                {
                                    int pos = (int)stat;
                                    if (pos >= 5) pos++;
                                    Contents[pos] = stat;
                                    FF8String name = Kernel_bin.MagicData[Setting.Stat_J[stat]].Name;
                                    if (name.Length == 0) name = Misc[Items._];

                                    ITEM[pos, 0] = new IGMDataItem_Icon(Stat2Icon[stat], new Rectangle(SIZE[pos].X, SIZE[pos].Y, 0, 0), 2);
                                    ITEM[pos, 1] = new IGMDataItem_String(name, new Rectangle(SIZE[pos].X + 80, SIZE[pos].Y, 0, 0));
                                    if (!unlocked.Contains(Kernel_bin.Stat2Ability[stat]))
                                    {
                                        ((IGMDataItem_Icon)ITEM[pos, 0]).Pallet = ((IGMDataItem_Icon)ITEM[pos, 0]).Faded_Pallet = 7;
                                        ((IGMDataItem_String)ITEM[pos, 1]).Colorid = Font.ColorID.Grey;
                                    }

                                    ITEM[pos, 2] = new IGMDataItem_Int(Setting.TotalStat(stat, VisableCharacter), new Rectangle(SIZE[pos].X + 152, SIZE[pos].Y, 0, 0), 2, Icons.NumType.sysFntBig, spaces: 10);
                                    ITEM[pos, 3] = stat == Kernel_bin.Stat.HIT || stat == Kernel_bin.Stat.EVA
                                        ? new IGMDataItem_String(Misc[Items.Percent], new Rectangle(SIZE[pos].X + 350, SIZE[pos].Y, 0, 0))
                                        : null;
                                    if (PrevSetting == null || PrevSetting.Stat_J[stat] == Setting.Stat_J[stat] || PrevSetting.TotalStat(stat, VisableCharacter) == Setting.TotalStat(stat, VisableCharacter))
                                    {
                                        ITEM[pos, 4] = null;
                                    }
                                    else if (PrevSetting.TotalStat(stat, VisableCharacter) > Setting.TotalStat(stat, VisableCharacter))
                                    {
                                        ((IGMDataItem_Icon)ITEM[pos, 0]).Pallet = 5;
                                        ((IGMDataItem_Icon)ITEM[pos, 0]).Faded_Pallet = 5;
                                        ((IGMDataItem_String)ITEM[pos, 1]).Colorid = Font.ColorID.Red;
                                        ((IGMDataItem_Int)ITEM[pos, 2]).Colorid = Font.ColorID.Red;
                                        if (ITEM[pos, 3] != null)
                                            ((IGMDataItem_String)ITEM[pos, 3]).Colorid = Font.ColorID.Red;
                                        ITEM[pos, 4] = new IGMDataItem_Icon(Icons.ID.Arrow_Down, new Rectangle(SIZE[pos].X + 250, SIZE[pos].Y, 0, 0), 16);
                                    }
                                    else
                                    {
                                        ((IGMDataItem_Icon)ITEM[pos, 0]).Pallet = 6;
                                        ((IGMDataItem_Icon)ITEM[pos, 0]).Faded_Pallet = 6;
                                        ((IGMDataItem_String)ITEM[pos, 1]).Colorid = Font.ColorID.Yellow;
                                        ((IGMDataItem_Int)ITEM[pos, 2]).Colorid = Font.ColorID.Yellow;
                                        if (ITEM[pos, 3] != null)
                                            ((IGMDataItem_String)ITEM[pos, 3]).Colorid = Font.ColorID.Yellow;
                                        ITEM[pos, 4] = new IGMDataItem_Icon(Icons.ID.Arrow_Up, new Rectangle(SIZE[pos].X + 250, SIZE[pos].Y, 0, 0), 17);
                                    }
                                }
                            }
                        }
                    }
                }

                public override void Inputs_Left()
                {
                    base.Inputs_Left();
                    if (CURSOR_SELECT < Count / cols)
                    {
                        InGameMenu_Junction.mode = Mode.Mag_EL_A_D;
                        InGameMenu_Junction.Data[SectionName.Mag_Group].Show();
                    }
                    else
                    {
                        CURSOR_SELECT -= Count / cols;
                    }
                }

                public override void Inputs_Right()
                {
                    base.Inputs_Left();
                    if (CURSOR_SELECT < Count / cols)
                    {
                        if (CURSOR_SELECT == 0) CURSOR_SELECT++;
                        CURSOR_SELECT += Count / cols;
                    }
                    else
                    {
                        InGameMenu_Junction.mode = Mode.Mag_ST_A_D;
                        InGameMenu_Junction.Data[SectionName.Mag_Group].Show();
                    }
                }

                public override bool Update()
                {
                    bool ret = base.Update();
                    if (InGameMenu_Junction != null && InGameMenu_Junction.mode == Mode.Mag_Stat && Enabled)
                    {
                        Cursor_Status |= Cursor_Status.Enabled;
                        Cursor_Status &= ~Cursor_Status.Horizontal;
                        Cursor_Status |= Cursor_Status.Vertical;
                        Cursor_Status &= ~Cursor_Status.Blinking;
                    }
                    else if (InGameMenu_Junction != null && InGameMenu_Junction.mode == Mode.Mag_Pool_Stat && Enabled)
                    {
                        Cursor_Status |= Cursor_Status.Blinking;
                    }
                    else
                    {
                        Cursor_Status &= ~Cursor_Status.Enabled;
                    }
                    return ret;
                }

                public override void BackupSetting() => PrevSetting = Setting.Clone();

                public override void UndoChange()
                {
                    //override this use it to take value of prevSetting and restore the setting unless default method works
                    if (PrevSetting != null)
                    {
                        Setting = PrevSetting.Clone();
                        Memory.State.Characters[Character] = Setting;
                    }
                }

                public override void ConfirmChange() =>
                    //set backupuped change to null so it no longer exists.
                    PrevSetting = null;

                public override void Inputs_OKAY()
                {
                    base.Inputs_OKAY();
                    InGameMenu_Junction.mode = Mode.Mag_Pool_Stat;
                    BackupSetting();
                }

                public override void Inputs_CANCEL()
                {
                    base.Inputs_CANCEL();
                    InGameMenu_Junction.mode = Mode.TopMenu_Junction;
                    InGameMenu_Junction.Data[SectionName.Mag_Group].Hide();
                }

                protected override void InitShift(int i, int col, int row)
                {
                    base.InitShift(i, col, row);
                    SIZE[i].Inflate(-22, -8);
                    SIZE[i].Offset(0, 4 + (-2 * row));
                }

                /// <summary>
                /// Things fixed at startup.
                /// </summary>
                protected override void Init() => base.Init();
            }
        }
    }
}
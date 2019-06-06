using Microsoft.Xna.Framework;

namespace FF8
{
    public partial class Module_main_menu_debug
    {
        private partial class IGM_Junction
        {
            private class IGMData_Mag_ST_A_D_Slots : IGMData
            {
                public IGMData_Mag_ST_A_D_Slots() : base( 5, 2, new IGMDataItem_Box(pos: new Rectangle(0, 414, 840, 216)), 1, 5)
                {
                }

                protected override void Init()
                {
                    Contents = new Kernel_bin.Stat[Count];
                    base.Init();
                }
                protected override void InitShift(int i, int col, int row)
                {
                    base.InitShift(i, col, row);
                    SIZE[i].Inflate(-30, -6);
                    SIZE[i].Y -= row * 2;
                }

                public override void ReInit()
                {
                    if (Memory.State.Characters != null)
                    {
                        Pool = (IGMData_Mag_Pool)((IGMDataItem_IGMData)((IGMData_Mag_Group)InGameMenu_Junction.Data[SectionName.Mag_Group]).ITEM[2, 0]).Data;
                        Contents[0] = Kernel_bin.Stat.ST_Atk;
                        ITEM[0, 0] = new IGMDataItem_Icon(Icons.ID.Icon_Status_Attack, new Rectangle(SIZE[0].X, SIZE[0].Y, 0, 0));
                        ITEM[0, 1] = new IGMDataItem_String(Kernel_bin.MagicData[Memory.State.Characters[Character].Stat_J[Kernel_bin.Stat.ST_Atk]].Name, new Rectangle(SIZE[0].X + 60, SIZE[0].Y, 0, 0));
                        BLANKS[0] = false;
                        for (byte pos = 1; pos < Count; pos++)
                        {
                            Contents[pos] = Kernel_bin.Stat.ST_Def_1 + pos - 1;
                            ITEM[pos, 0] = new IGMDataItem_Icon(Icons.ID.Icon_Status_Defense, new Rectangle(SIZE[pos].X, SIZE[pos].Y, 0, 0));
                            ITEM[pos, 1] = new IGMDataItem_String(Kernel_bin.MagicData[Memory.State.Characters[Character].Stat_J[Kernel_bin.Stat.ST_Def_1 + pos - 1]].Name, new Rectangle(SIZE[pos].X + 60, SIZE[pos].Y, 0, 0));
                            BLANKS[pos] = false;
                        }
                        base.ReInit();
                    }
                }

                public new Saves.CharacterData PrevSetting { get; private set; }
                public new Saves.CharacterData Setting { get; private set; }
                public Kernel_bin.Stat[] Contents { get; private set; }
                public IGMData_Mag_Pool Pool { get; private set; }

                public override void Inputs_Left()
                {
                    base.Inputs_Left();
                    InGameMenu_Junction.mode = Mode.Mag_Stat;
                    InGameMenu_Junction.Data[SectionName.Mag_Group].Show();
                }

                public override void Inputs_Right()
                {
                    base.Inputs_Left();
                    InGameMenu_Junction.mode = Mode.Mag_EL_A;
                    InGameMenu_Junction.Data[SectionName.Mag_Group].Show();
                }

                public override bool Update()
                {
                    bool ret = base.Update();
                    if (InGameMenu_Junction != null && InGameMenu_Junction.mode == Mode.Mag_ST_A && Enabled)
                    {
                        Cursor_Status |= Cursor_Status.Enabled;
                        Cursor_Status &= ~Cursor_Status.Horizontal;
                        Cursor_Status |= Cursor_Status.Vertical;
                        Cursor_Status &= ~Cursor_Status.Blinking;
                        if (CURSOR_SELECT > 0)
                        {
                            ((IGMDataItem_IGMData)((IGMData_Mag_Group)InGameMenu_Junction.Data[SectionName.Mag_Group]).ITEM[7, 0]).Data.Hide();
                            ((IGMDataItem_IGMData)((IGMData_Mag_Group)InGameMenu_Junction.Data[SectionName.Mag_Group]).ITEM[8, 0]).Data.Show();
                        }
                        else
                        {
                            ((IGMDataItem_IGMData)((IGMData_Mag_Group)InGameMenu_Junction.Data[SectionName.Mag_Group]).ITEM[7, 0]).Data.Show();
                            ((IGMDataItem_IGMData)((IGMData_Mag_Group)InGameMenu_Junction.Data[SectionName.Mag_Group]).ITEM[8, 0]).Data.Hide();
                        }
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

                public override void BackupSetting()
                {
                    if (Setting == null)
                        Setting = Memory.State.Characters[Character];
                    PrevSetting = Setting.Clone();
                }

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
                    InGameMenu_Junction.mode = CURSOR_SELECT == 0 ? Mode.Mag_Pool_ST_A : Mode.Mag_Pool_ST_D;
                    Pool.ReInit();
                    BackupSetting();
                }

                public override void Inputs_CANCEL()
                {
                    base.Inputs_CANCEL();
                    InGameMenu_Junction.mode = Mode.TopMenu_Junction;
                    InGameMenu_Junction.Data[SectionName.Mag_Group].Hide();
                }
            }
        }
    }
}
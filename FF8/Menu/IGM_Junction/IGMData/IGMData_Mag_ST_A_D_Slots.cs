using Microsoft.Xna.Framework;

namespace FF8
{
    public partial class Module_main_menu_debug
    {
        private partial class IGM_Junction
        {
            private class IGMData_Mag_ST_A_D_Slots : IGMData_Slots<Kernel_bin.Stat, Saves.CharacterData>
            {
                public IGMData_Mag_ST_A_D_Slots() : base(5, 2, new IGMDataItem_Box(pos: new Rectangle(0, 414, 840, 216)), 1, 5)
                {
                }

                protected override void Init()
                {
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
                        CheckMode();
                        base.ReInit();
                    }
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
                        Memory.State.Characters[Character] = Setting = PrevSetting.Clone();
                    }
                }

                public override void Inputs_Left()
                {
                    base.Inputs_Left();
                    InGameMenu_Junction.SetMode(Mode.Mag_Stat);
                }

                public override void Inputs_Right()
                {
                    base.Inputs_Left();
                    InGameMenu_Junction.SetMode(Mode.Mag_EL_A);
                }

                public override void CheckMode(bool cursor = true) =>
                    CheckMode(0, Mode.Mag_ST_A, Mode.Mag_ST_D,
                        InGameMenu_Junction != null && (InGameMenu_Junction.GetMode() == Mode.Mag_ST_A || InGameMenu_Junction.GetMode() == Mode.Mag_ST_D),
                        InGameMenu_Junction != null && (InGameMenu_Junction.GetMode() == Mode.Mag_Pool_ST_A || InGameMenu_Junction.GetMode() == Mode.Mag_Pool_ST_D),
                        cursor);
                



                public override void Inputs_OKAY()
                {
                    base.Inputs_OKAY();
                    InGameMenu_Junction.SetMode(CURSOR_SELECT == 0 ? Mode.Mag_Pool_ST_A : Mode.Mag_Pool_ST_D);
                    BackupSetting();
                }

                public override void Inputs_CANCEL()
                {
                    base.Inputs_CANCEL();
                    InGameMenu_Junction.SetMode(Mode.TopMenu_Junction);
                }
            }

            public abstract class IGMData_Slots<T,T2> : IGMData
            {
                public T[] Contents { get; protected set; }

                public IGMData_Slots(int count, int depth, IGMDataItem container = null, int? cols = null, int? rows = null) : base(count, depth, container, cols, rows)
                {

                    Contents = new T[Count];
                }
                public T2 Setting { get; protected set; }
                public T2 PrevSetting { get; protected set; }
                public virtual void CheckMode(bool cursor = true) => CheckMode(0, Mode.None, Mode.None, false, false, cursor);
                public void CheckMode(int pos, Mode one, Mode two, bool slots, bool pools, bool cursor = true)
                {
                    if (InGameMenu_Junction != null && slots && Enabled)
                    {
                        Cursor_Status &= ~Cursor_Status.Horizontal;
                        Cursor_Status |= Cursor_Status.Vertical;
                        Cursor_Status &= ~Cursor_Status.Blinking;
                        if (CURSOR_SELECT > pos)
                            InGameMenu_Junction.SetMode(two);
                        else
                            InGameMenu_Junction.SetMode(one);
                    }
                    else if (InGameMenu_Junction != null && pools && Enabled)
                    {
                        Cursor_Status |= Cursor_Status.Blinking;
                    }
                    if (cursor)
                        Cursor_Status |= Cursor_Status.Enabled;
                    else
                        Cursor_Status &= ~Cursor_Status.Enabled;
                }

                public virtual void ConfirmChange()
                {
                    PrevSetting = default;
                }
                public abstract void BackupSetting();
                public abstract void UndoChange();

                public override bool Inputs()
                {
                    bool ret = base.Inputs();
                    if (ret) CheckMode();
                    return ret;
                }
            }
        }
    }
}
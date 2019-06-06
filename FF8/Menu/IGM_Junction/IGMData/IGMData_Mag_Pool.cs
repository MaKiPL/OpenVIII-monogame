using Microsoft.Xna.Framework;
using System.Linq;

namespace FF8
{
    public partial class Module_main_menu_debug
    {
        private partial class IGM_Junction
        {
            private class IGMData_Mag_Pool : IGMData_Pool<Saves.CharacterData, byte>
            {
                public IGMData_Mag_Stat_Slots Stat_Slots { get; private set; }
                public IGMData_Mag_EL_A_D_Slots EL_A_D_Slots { get; private set; }
                public IGMData_Mag_ST_A_D_Slots ST_A_D_Slots { get; private set; }
                public IGMData_Mag_EL_A_Values EL_A_Values { get; private set; }
                public IGMData_Mag_EL_D_Values EL_D_Values { get; private set; }
                public IGMData_Mag_ST_A_Values ST_A_Values { get; private set; }
                public IGMData_Mag_ST_D_Values ST_D_Values { get; private set; }
                public Mode SortMode { get; private set; }

                public IGMData_Mag_Pool() : base(5, 3, new IGMDataItem_Box(pos: new Rectangle(135, 150, 300, 192), title: Icons.ID.MAGIC), 4, 13)
                {
                }

                protected override void InitShift(int i, int col, int row)
                {
                    base.InitShift(i, col, row);
                    SIZE[i].Inflate(-22, -8);
                    SIZE[i].Offset(0, 12 + (-8 * row));
                }

                private void addMagic(ref int pos, Kernel_bin.Magic_Data spell, Font.ColorID color = Font.ColorID.White)
                {
                    ITEM[pos, 0] = new IGMDataItem_String(spell.Name, SIZE[pos], color);
                    ITEM[pos, 1] = color != Font.ColorID.White ? new IGMDataItem_Icon(Icons.ID.JunctionSYM, new Rectangle(SIZE[pos].X + SIZE[pos].Width - 75, SIZE[pos].Y, 0, 0)) : null;
                    ITEM[pos, 2] = new IGMDataItem_Int(Source.Magics[spell.ID], new Rectangle(SIZE[pos].X + SIZE[pos].Width - 50, SIZE[pos].Y, 0, 0), spaces: 3);
                    BLANKS[pos] = color == Font.ColorID.Dark_Gray ? true : false;
                    Contents[pos] = spell.ID;
                    pos++;
                }

                protected override void Init()
                {
                    base.Init();
                    SIZE[rows] = SIZE[0];
                    SIZE[rows].Y = Y;
                    ITEM[rows, 2] = new IGMDataItem_Icon(Icons.ID.NUM_, new Rectangle(SIZE[rows].X + SIZE[rows].Width - 45, SIZE[rows].Y, 0, 0), scale: new Vector2(2.5f));
                    BLANKS[rows] = true;
                }

                public override void ReInit()
                {
                    if (Memory.State.Characters != null)
                    {
                        if(SortMode == Mode.None)
                        SortMode = InGameMenu_Junction.mode;

                        switch (InGameMenu_Junction.mode)
                        {
                            default:
                            case Mode.Mag_Stat:
                            case Mode.Mag_Pool_Stat:
                                    SortMode = Mode.Mag_Pool_Stat;
                                break;
                            case Mode.Mag_ST_D:
                            case Mode.Mag_Pool_ST_D:
                                SortMode = Mode.Mag_Pool_ST_D;
                                break;
                            case Mode.Mag_ST_A:
                            case Mode.Mag_Pool_ST_A:
                                    SortMode = Mode.Mag_Pool_ST_A;
                                break;
                            case Mode.Mag_EL_D:
                            case Mode.Mag_Pool_EL_D:
                                SortMode = Mode.Mag_Pool_EL_D;
                                break;
                            case Mode.Mag_EL_A:
                            case Mode.Mag_Pool_EL_A:
                                    SortMode = Mode.Mag_Pool_EL_A;
                                break;

                        }
                        Cursor_Status &= ~Cursor_Status.Horizontal;
                        Cursor_Status |= Cursor_Status.Vertical;
                        Cursor_Status &= ~Cursor_Status.Blinking;
                        Stat_Slots = (IGMData_Mag_Stat_Slots)((IGMDataItem_IGMData)((IGMData_Mag_Group)InGameMenu_Junction.Data[SectionName.Mag_Group]).ITEM[0, 0]).Data;
                        EL_A_D_Slots = (IGMData_Mag_EL_A_D_Slots)((IGMDataItem_IGMData)((IGMData_Mag_Group)InGameMenu_Junction.Data[SectionName.Mag_Group]).ITEM[3, 0]).Data;
                        ST_A_D_Slots = (IGMData_Mag_ST_A_D_Slots)((IGMDataItem_IGMData)((IGMData_Mag_Group)InGameMenu_Junction.Data[SectionName.Mag_Group]).ITEM[6, 0]).Data;
                        EL_A_Values = (IGMData_Mag_EL_A_Values)((IGMDataItem_IGMData)((IGMData_Mag_Group)InGameMenu_Junction.Data[SectionName.Mag_Group]).ITEM[4, 0]).Data;
                        EL_D_Values = (IGMData_Mag_EL_D_Values)((IGMDataItem_IGMData)((IGMData_Mag_Group)InGameMenu_Junction.Data[SectionName.Mag_Group]).ITEM[5, 0]).Data;
                        ST_A_Values = (IGMData_Mag_ST_A_Values)((IGMDataItem_IGMData)((IGMData_Mag_Group)InGameMenu_Junction.Data[SectionName.Mag_Group]).ITEM[7, 0]).Data;
                        ST_D_Values = (IGMData_Mag_ST_D_Values)((IGMDataItem_IGMData)((IGMData_Mag_Group)InGameMenu_Junction.Data[SectionName.Mag_Group]).ITEM[8, 0]).Data;
                        Source = Memory.State.Characters[Character];
                        System.Collections.Generic.IEnumerable<Kernel_bin.Magic_Data> sort = null;
                        switch (SortMode)
                        {
                            //case Mode.Mag_Pool_Stat:
                            //    //TODO sort by selected stat.
                            //    break;
                            case Mode.Mag_Pool_EL_D:
                                sort = Kernel_bin.MagicData.OrderBy(x => -x.Elem_J_def_val * x.Elem_J_def.Count());
                                break;
                            case Mode.Mag_Pool_EL_A:
                                sort = Kernel_bin.MagicData.OrderBy(x => -x.Elem_J_atk_val * x.Elem_J_atk.Count());
                                break;
                            case Mode.Mag_Pool_ST_D:
                                sort = Kernel_bin.MagicData.OrderBy(x => -x.Stat_J_def_val * x.Stat_J_def.Count());
                                break;
                            case Mode.Mag_Pool_ST_A:
                                sort = Kernel_bin.MagicData.OrderBy(x => -x.Stat_J_atk_val * x.Stat_J_atk.Count());
                                break;
                            default:
                                sort = Kernel_bin.MagicData.AsEnumerable();
                                break;

                        }
                        int pos = 0;
                        int skip = Page * rows;
                        foreach(var i in sort)
                        {
                            if (pos >= rows) break;
                            if (Source.Magics.ContainsKey(i.ID) && skip-- <= 0 && i.ID > 0)
                            {
                                if (Source.Stat_J.ContainsValue(i.ID))
                                    addMagic(ref pos, i, Font.ColorID.Grey);
                                else
                                    switch (SortMode)
                                    {
                                        //case Mode.Mag_Pool_Stat:
                                        //    //TODO sort by selected stat.
                                        //    break;
                                        case Mode.Mag_Pool_EL_D:
                                            if(i.Elem_J_def_val * i.Elem_J_def.Count() == 0)
                                                addMagic(ref pos, i, Font.ColorID.Dark_Gray);
                                            else
                                                addMagic(ref pos, i, Font.ColorID.White);
                                            break;
                                        case Mode.Mag_Pool_EL_A:
                                            if (i.Elem_J_atk_val * i.Elem_J_atk.Count() == 0)
                                                addMagic(ref pos, i, Font.ColorID.Dark_Gray);
                                            else
                                                addMagic(ref pos, i, Font.ColorID.White);
                                            break;
                                        case Mode.Mag_Pool_ST_D:
                                            if (i.Stat_J_def_val * i.Stat_J_def.Count() == 0)
                                                addMagic(ref pos, i, Font.ColorID.Dark_Gray);
                                            else
                                                addMagic(ref pos, i, Font.ColorID.White);
                                            break;
                                        case Mode.Mag_Pool_ST_A:
                                            if (i.Stat_J_atk_val * i.Stat_J_atk.Count() == 0)
                                                addMagic(ref pos, i, Font.ColorID.Dark_Gray);
                                            else
                                                addMagic(ref pos, i, Font.ColorID.White);
                                            break;
                                        default:
                                            addMagic(ref pos, i, Font.ColorID.White);
                                            break;
                                    }

                            }
                        }
                        for (; pos < rows; pos++)
                        {
                            ITEM[pos, 0] = null;
                            ITEM[pos, 1] = null;
                            ITEM[pos, 2] = null;
                            BLANKS[pos] = true;
                        }
                        base.ReInit();
                        UpdateTitle();
                    }
                }

                public override void UpdateTitle()
                {
                    base.UpdateTitle();
                    if (Pages == 1)
                    {
                        ((IGMDataItem_Box)CONTAINER).Title = Icons.ID.MAGIC;
                        ITEM[Count - 1, 0] = ITEM[Count - 2, 0] = null;
                    }
                    else
                        if (Page < Pages)
                        ((IGMDataItem_Box)CONTAINER).Title = (Icons.ID)((int)Icons.ID.MAGIC_PG1 + Page);
                }

                public override bool Update()
                {
                    if (InGameMenu_Junction != null && Enabled)
                    {
                        Kernel_bin.Stat stat = Kernel_bin.Stat.None;
                        IGMData Slots = null;
                        IGMData Values = null;
                        if (InGameMenu_Junction.mode == Mode.Mag_Pool_Stat)
                        {
                            stat = Stat_Slots.Contents[Stat_Slots.CURSOR_SELECT];
                            Slots = Stat_Slots;
                        }
                        else if (InGameMenu_Junction.mode == Mode.Mag_Pool_EL_A || InGameMenu_Junction.mode == Mode.Mag_Pool_EL_D)
                        {
                            stat = EL_A_D_Slots.Contents[EL_A_D_Slots.CURSOR_SELECT];
                            Slots = EL_A_D_Slots;
#pragma warning disable IDE0045 // Convert to conditional expression
                            if (InGameMenu_Junction.mode == Mode.Mag_Pool_EL_A)
                                Values = EL_A_Values;
                            else
                                Values = EL_D_Values;
#pragma warning restore IDE0045 // Convert to conditional expression
                        }
                        else if (InGameMenu_Junction.mode == Mode.Mag_Pool_ST_A || InGameMenu_Junction.mode == Mode.Mag_Pool_ST_D)
                        {
                            stat = ST_A_D_Slots.Contents[ST_A_D_Slots.CURSOR_SELECT];
                            Slots = ST_A_D_Slots;
#pragma warning disable IDE0045 // Convert to conditional expression
                            if (InGameMenu_Junction.mode == Mode.Mag_Pool_ST_A)
                                Values = ST_A_Values;
                            else
                                Values = ST_D_Values;
#pragma warning restore IDE0045 // Convert to conditional expression
                        }
                    
                        if (Slots!= null && stat != Kernel_bin.Stat.None && CURSOR_SELECT < Contents.Length)
                        {
                            Cursor_Status |= Cursor_Status.Enabled;
                            if (Source.Stat_J[stat] != Contents[CURSOR_SELECT])
                            {
                                Slots.UndoChange();
                                if (Memory.State.Characters != null)
                                {
                                    Source = Memory.State.Characters[Character];
                                }
                                if (Source.Stat_J.ContainsValue(Contents[CURSOR_SELECT]))
                                {
                                    Kernel_bin.Stat key = Source.Stat_J.FirstOrDefault(x => x.Value == Contents[CURSOR_SELECT]).Key;
                                    Source.Stat_J[key] = 0;
                                }
                                Source.Stat_J[stat] = Contents[CURSOR_SELECT];
                                Slots.ReInit();
                                if (Values != null) Values.ReInit();
                            }
                        }
                    }
                    else
                    {
                        Cursor_Status &= ~Cursor_Status.Enabled;
                    }
                    return base.Update();
                }

                protected override void PAGE_PREV()
                {
                    base.PAGE_PREV();
                    ReInit();
                }

                protected override void PAGE_NEXT()
                {
                    base.PAGE_NEXT();
                    ReInit();
                }

                public override void Inputs_CANCEL()
                {
                    if (Memory.State.Characters != null)
                    {
                        base.Inputs_CANCEL();
                        //TODO have pool return to correct screen as there will be 3 possible return modes.
                        if (InGameMenu_Junction.mode == Mode.Mag_Pool_Stat)
                        {
                            InGameMenu_Junction.mode = Mode.Mag_Stat;
                            Stat_Slots.UndoChange();
                            Stat_Slots.ConfirmChange();
                            Stat_Slots.ReInit();
                        }
                        else if (InGameMenu_Junction.mode == Mode.Mag_Pool_EL_A || InGameMenu_Junction.mode == Mode.Mag_Pool_EL_D)
                        {
                            InGameMenu_Junction.mode = Mode.Mag_EL_A;
                            EL_A_D_Slots.UndoChange();
                            EL_A_D_Slots.ConfirmChange();
                            EL_A_D_Slots.ReInit();
                        }
                        else if (InGameMenu_Junction.mode == Mode.Mag_Pool_ST_A || InGameMenu_Junction.mode == Mode.Mag_Pool_ST_D)
                        {
                            InGameMenu_Junction.mode = Mode.Mag_ST_A;
                            ST_A_D_Slots.UndoChange();
                            ST_A_D_Slots.ConfirmChange();
                            ST_A_D_Slots.ReInit();
                        }
                        Cursor_Status &= ~Cursor_Status.Enabled;
                        Source = Memory.State.Characters[Character];
                    }
                }

                public override void Inputs_OKAY()
                {
                    if (Memory.State.Characters != null)
                    {
                        skipsnd = true;
                        init_debugger_Audio.PlaySound(31);
                        base.Inputs_OKAY();
                        if (InGameMenu_Junction.mode == Mode.Mag_Pool_Stat)
                        {
                            InGameMenu_Junction.mode = Mode.Mag_Stat;
                            Stat_Slots.ConfirmChange();
                        }
                        else if (InGameMenu_Junction.mode == Mode.Mag_Pool_EL_A || InGameMenu_Junction.mode == Mode.Mag_Pool_EL_D)
                        {
                            InGameMenu_Junction.mode = Mode.Mag_EL_A;
                            EL_A_D_Slots.ConfirmChange();
                        }
                        else if (InGameMenu_Junction.mode == Mode.Mag_Pool_ST_A || InGameMenu_Junction.mode == Mode.Mag_Pool_ST_D)
                        {
                            InGameMenu_Junction.mode = Mode.Mag_ST_A;
                            ST_A_D_Slots.ConfirmChange();
                        }
                        Cursor_Status &= ~Cursor_Status.Enabled;
                        InGameMenu_Junction.ReInit();
                    }
                }
            }
        }
    }
}
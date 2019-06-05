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

                public IGMData_Mag_Pool() : base(5, 3, new IGMDataItem_Box(pos: new Rectangle(135, 150, 300, 192), title: Icons.ID.MAGIC), 4, 13)
                {
                }

                protected override void InitShift(int i, int col, int row)
                {
                    base.InitShift(i, col, row);
                    SIZE[i].Inflate(-22, -8);
                    SIZE[i].Offset(0, 12 + (-8 * row));
                }

                private void addMagic(ref int pos, byte spell, Font.ColorID color = Font.ColorID.White)
                {
                    ITEM[pos, 0] = new IGMDataItem_String(Kernel_bin.MagicData[spell].Name, SIZE[pos], color);
                    ITEM[pos, 1] = color != Font.ColorID.White ? new IGMDataItem_Icon(Icons.ID.JunctionSYM, new Rectangle(SIZE[pos].X + SIZE[pos].Width - 75, SIZE[pos].Y, 0, 0)) : null;
                    ITEM[pos, 2] = new IGMDataItem_Int(Source.Magics[spell], new Rectangle(SIZE[pos].X + SIZE[pos].Width - 50, SIZE[pos].Y, 0, 0), spaces: 3);
                    BLANKS[pos] = false;
                    Contents[pos] = spell;
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
                        Stat_Slots = (IGMData_Mag_Stat_Slots)((IGMDataItem_IGMData)((IGMData_Mag_Group)InGameMenu_Junction.Data[SectionName.Mag_Group]).ITEM[0, 0]).Data;
                        EL_A_D_Slots = (IGMData_Mag_EL_A_D_Slots)((IGMDataItem_IGMData)((IGMData_Mag_Group)InGameMenu_Junction.Data[SectionName.Mag_Group]).ITEM[3, 0]).Data;
                        ST_A_D_Slots = (IGMData_Mag_ST_A_D_Slots)((IGMDataItem_IGMData)((IGMData_Mag_Group)InGameMenu_Junction.Data[SectionName.Mag_Group]).ITEM[6, 0]).Data;
                        Source = Memory.State.Characters[Character];

                        int pos = 0;
                        int skip = Page * rows;
                        for (byte i = 1; i < Kernel_bin.MagicData.Length && pos < rows; i++)
                        {
                            if (Source.Magics.ContainsKey(i) && skip-- <= 0)
                            {
                                if (Source.Stat_J.ContainsValue(i))
                                    addMagic(ref pos, i, Font.ColorID.Grey);
                                else
                                    addMagic(ref pos, i, Font.ColorID.White);
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
                    Cursor_Status |= Cursor_Status.Enabled;
                    Cursor_Status &= ~Cursor_Status.Horizontal;
                    Cursor_Status |= Cursor_Status.Vertical;
                    Cursor_Status &= ~Cursor_Status.Blinking;
                    if (InGameMenu_Junction != null && Enabled)
                    {
                        Kernel_bin.Stat stat = Kernel_bin.Stat.None;
                        IGMData Slots = null;
                        if (InGameMenu_Junction.mode == Mode.Mag_Pool_Stat)
                        {
                            stat = Stat_Slots.Contents[Stat_Slots.CURSOR_SELECT];
                            Slots = Stat_Slots;
                        }
                        else if (InGameMenu_Junction.mode == Mode.Mag_Pool_EL_A || InGameMenu_Junction.mode == Mode.Mag_Pool_EL_D)
                        {
                            stat = EL_A_D_Slots.Contents[Stat_Slots.CURSOR_SELECT];
                            Slots = EL_A_D_Slots;
                        }
                        else if (InGameMenu_Junction.mode == Mode.Mag_Pool_ST_A || InGameMenu_Junction.mode == Mode.Mag_Pool_ST_D)
                        {
                            stat = ST_A_D_Slots.Contents[Stat_Slots.CURSOR_SELECT];
                            Slots = ST_A_D_Slots;
                        }
                        if (stat != Kernel_bin.Stat.None && CURSOR_SELECT < Contents.Length)
                        {
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
                            InGameMenu_Junction.mode = Mode.Mag_Stat;
                            EL_A_D_Slots.UndoChange();
                            EL_A_D_Slots.ConfirmChange();
                            EL_A_D_Slots.ReInit();
                        }
                        else if (InGameMenu_Junction.mode == Mode.Mag_Pool_ST_A || InGameMenu_Junction.mode == Mode.Mag_Pool_ST_D)
                        {
                            InGameMenu_Junction.mode = Mode.Mag_Stat;
                            EL_A_D_Slots.UndoChange();
                            EL_A_D_Slots.ConfirmChange();
                            EL_A_D_Slots.ReInit();
                        }
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
                            InGameMenu_Junction.mode = Mode.Mag_Stat;
                            EL_A_D_Slots.ConfirmChange();
                        }
                        else if (InGameMenu_Junction.mode == Mode.Mag_Pool_ST_A || InGameMenu_Junction.mode == Mode.Mag_Pool_ST_D)
                        {
                            InGameMenu_Junction.mode = Mode.Mag_Stat;
                            EL_A_D_Slots.ConfirmChange();
                        }
                        InGameMenu_Junction.ReInit();
                    }
                }
            }
        }
    }
}
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FF8
{
    public partial class Module_main_menu_debug
    {

        #region Classes

        private partial class IGM_Junction
        {

            #region Classes

            private class IGMData_Mag_Pool : IGMData_Pool<Saves.CharacterData, byte>
            {

                #region Fields

                public static EventHandler<Mode> SlotConfirmListener;
                public static EventHandler<Mode> SlotReinitListener;
                public static EventHandler<Mode> SlotUndoListener;
                public static EventHandler<Kernel_bin.Stat> StatEventListener;
                private bool eventAdded = false;

                #endregion Fields

                #region Constructors

                public IGMData_Mag_Pool() : base(5, 3, new IGMDataItem_Box(pos: new Rectangle(135, 150, 300, 192), title: Icons.ID.MAGIC), 4, 13)
                {
                }

                #endregion Constructors

                #region Properties

                public Characters LastCharacter { get; private set; }
                public Mode LastMode { get; private set; }
                public int LastPage { get; private set; }
                public Kernel_bin.Stat LastStat { get; private set; }
                public IEnumerable<Kernel_bin.Magic_Data> Sort { get; private set; }
                public Mode SortMode { get; private set; }

                public Kernel_bin.Stat Stat { get; private set; }

                #endregion Properties

                #region Methods

                public void FillMagic()
                {
                    int pos = 0;
                    int skip = Page * rows;
                    if (Sort != null)
                        foreach (Kernel_bin.Magic_Data i in Sort)
                        {
                            if (pos >= rows) break;
                            if (Source.Magics.ContainsKey(i.ID) && skip-- <= 0 && i.ID > 0)
                            {
                                switch (SortMode)
                                {
                                    case Mode.Mag_Pool_Stat:
                                        if (i.J_Val[Stat] == 0)
                                            addMagic(ref pos, i, Font.ColorID.Dark_Gray);
                                        else
                                            addMagic(ref pos, i, Font.ColorID.White);
                                        break;

                                    case Mode.Mag_Pool_EL_D:
                                        if (i.J_Val[Stat] * i.Elem_J_def.Count() == 0)
                                            addMagic(ref pos, i, Font.ColorID.Dark_Gray);
                                        else
                                            addMagic(ref pos, i, Font.ColorID.White);
                                        break;

                                    case Mode.Mag_Pool_EL_A:
                                        if (i.J_Val[Stat] * i.Elem_J_atk.Count() == 0)
                                            addMagic(ref pos, i, Font.ColorID.Dark_Gray);
                                        else
                                            addMagic(ref pos, i, Font.ColorID.White);
                                        break;

                                    case Mode.Mag_Pool_ST_D:
                                        if (i.J_Val[Stat] * i.Stat_J_def.Count() == 0)
                                            addMagic(ref pos, i, Font.ColorID.Dark_Gray);
                                        else
                                            addMagic(ref pos, i, Font.ColorID.White);
                                        break;

                                    case Mode.Mag_Pool_ST_A:
                                        if (i.J_Val[Stat] * i.Stat_J_atk.Count() == 0)
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
                        Contents[pos] = 0;
                    }
                }

                public void Generate_Preview(bool skipundo = false)
                {
                    if (Stat != Kernel_bin.Stat.None && CURSOR_SELECT < Contents.Length)
                    {
                        Cursor_Status |= Cursor_Status.Enabled;
                        if (Source.Stat_J[Stat] != Contents[CURSOR_SELECT])
                        {
                            if (!skipundo)
                            {
                                Undo();
                                skipundo = false;
                            }
                            Source.JunctionSpell(Stat, Contents[CURSOR_SELECT]);
                            SlotReinitListener?.Invoke(this, InGameMenu_Junction.GetMode());
                        }
                    }
                }
                public void Get_Slots_Values() =>
                    Source = Memory.State.Characters[Character];

                public void Get_Sort()
                {
                    switch (SortMode)
                    {
                        case Mode.Mag_Pool_Stat:
                            if (Stat != Kernel_bin.Stat.None)
                                Sort = Kernel_bin.MagicData.OrderBy(x => (-x.J_Val[Stat] * (Source.Magics.ContainsKey(x.ID) ? Source.Magics[x.ID] : 0)) / 100);
                            break;

                        case Mode.Mag_Pool_EL_D:
                            Sort = Kernel_bin.MagicData.OrderBy(x => (-x.J_Val[Kernel_bin.Stat.EL_Def_1] * x.Elem_J_def.Count() * (Source.Magics.ContainsKey(x.ID) ? Source.Magics[x.ID] : 0)) / 100);
                            break;

                        case Mode.Mag_Pool_EL_A:
                            Sort = Kernel_bin.MagicData.OrderBy(x => (-x.J_Val[Kernel_bin.Stat.EL_Atk] * x.Elem_J_atk.Count() * (Source.Magics.ContainsKey(x.ID) ? Source.Magics[x.ID] : 0)) / 100);
                            break;

                        case Mode.Mag_Pool_ST_D:
                            Sort = Kernel_bin.MagicData.OrderBy(x => (-x.J_Val[Kernel_bin.Stat.ST_Def_1] * x.Stat_J_def.Count() * (Source.Magics.ContainsKey(x.ID) ? Source.Magics[x.ID] : 0)) / 100);
                            break;

                        case Mode.Mag_Pool_ST_A:
                            Sort = Kernel_bin.MagicData.OrderBy(x => (-x.J_Val[Kernel_bin.Stat.ST_Atk] * x.Stat_J_atk.Count() * (Source.Magics.ContainsKey(x.ID) ? Source.Magics[x.ID] : 0)) / 100);
                            break;

                        default:
                            Sort = Kernel_bin.MagicData.AsEnumerable();
                            break;
                    }
                }

                public override void Inputs_CANCEL()
                {
                    if (Memory.State.Characters != null)
                    {
                        base.Inputs_CANCEL();
                        SlotUndoListener?.Invoke(this, InGameMenu_Junction.GetMode());
                        SlotConfirmListener?.Invoke(this, InGameMenu_Junction.GetMode());
                        SlotReinitListener?.Invoke(this, InGameMenu_Junction.GetMode());
                        switch(SortMode)
                        {
                            case Mode.Mag_Pool_Stat:
                                InGameMenu_Junction.SetMode(Mode.Mag_Stat);
                                break;
                            case Mode.Mag_Pool_EL_A:
                                InGameMenu_Junction.SetMode(Mode.Mag_EL_A);
                                break;
                            case Mode.Mag_Pool_EL_D:
                                InGameMenu_Junction.SetMode(Mode.Mag_EL_D);
                                break;
                            case Mode.Mag_Pool_ST_A:
                                InGameMenu_Junction.SetMode(Mode.Mag_ST_A);
                                break;
                            case Mode.Mag_Pool_ST_D:
                                InGameMenu_Junction.SetMode(Mode.Mag_ST_D);
                                break;
                        }

                        Cursor_Status &= ~Cursor_Status.Enabled;
                        Source = Memory.State.Characters[Character];
                    }
                }

                public override void Inputs_OKAY()
                {
                    if (Memory.State.Characters != null)
                    {
                        if (!BLANKS[CURSOR_SELECT])
                        {
                            skipsnd = true;
                            init_debugger_Audio.PlaySound(31);
                            base.Inputs_OKAY();
                            SlotConfirmListener?.Invoke(this, InGameMenu_Junction.GetMode());
                            if (InGameMenu_Junction.GetMode() == Mode.Mag_Pool_Stat)
                            {
                                InGameMenu_Junction.SetMode(Mode.Mag_Stat);
                            }
                            else if (InGameMenu_Junction.GetMode() == Mode.Mag_Pool_EL_A || InGameMenu_Junction.GetMode() == Mode.Mag_Pool_EL_D)
                            {
                                InGameMenu_Junction.SetMode(Mode.Mag_EL_A);
                            }
                            else if (InGameMenu_Junction.GetMode() == Mode.Mag_Pool_ST_A || InGameMenu_Junction.GetMode() == Mode.Mag_Pool_ST_D)
                            {
                                InGameMenu_Junction.SetMode(Mode.Mag_ST_A);
                            }
                            Cursor_Status &= ~Cursor_Status.Enabled;
                            InGameMenu_Junction.ReInit();
                        }
                    }
                }

                //public IGMData Values { get; private set; } = null;
                public override void ReInit()
                {
                    if (!eventAdded)
                    {
                        ModeChangeEventListener += ModeChangeEvent;
                        StatEventListener += StatChangeEvent;
                        eventAdded = true;
                    }
                    base.ReInit();
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

                protected override void Init()
                {
                    base.Init();
                    SIZE[rows] = SIZE[0];
                    SIZE[rows].Y = Y;
                    ITEM[rows, 2] = new IGMDataItem_Icon(Icons.ID.NUM_, new Rectangle(SIZE[rows].X + SIZE[rows].Width - 45, SIZE[rows].Y, 0, 0), scale: new Vector2(2.5f));
                    BLANKS[rows] = true;
                    Cursor_Status &= ~Cursor_Status.Horizontal;
                    Cursor_Status |= Cursor_Status.Vertical;
                    Cursor_Status &= ~Cursor_Status.Blinking;
                }

                //public override bool Inputs()
                //{
                //    bool ret = base.Inputs();
                //    if(ret)
                //    {
                //        return ret;
                //    }
                //    return false;
                //}
                protected override void InitShift(int i, int col, int row)
                {
                    base.InitShift(i, col, row);
                    SIZE[i].Inflate(-22, -8);
                    SIZE[i].Offset(0, 12 + (-8 * row));
                }

                protected override void PAGE_NEXT()
                {
                    base.PAGE_NEXT();
                    UpdateOnEvent(this);
                    while (Contents[0] == 0 && Page > 0)
                    {
                        skipsnd = true;
                        base.PAGE_NEXT();
                        UpdateOnEvent(this);
                    }
                }

                protected override void PAGE_PREV()
                {
                    base.PAGE_PREV();
                    UpdateOnEvent(this);
                    while (Contents[0] == 0 && Page > 0)
                    {
                        skipsnd = true;
                        base.PAGE_PREV();
                        UpdateOnEvent(this);
                    }
                }

                protected override void SetCursor_select(int value)
                {
                    if (value != GetCursor_select())
                    {
                        base.SetCursor_select(value);
                        UpdateOnEvent(this);
                    }
                }

                private void addMagic(ref int pos, Kernel_bin.Magic_Data spell, Font.ColorID color = Font.ColorID.White)
                {
                    if (color == Font.ColorID.White && Source.Stat_J.ContainsValue(spell.ID))
                    {
                        //spell is junctioned
                        color = Font.ColorID.Grey;
                    }
                    ITEM[pos, 0] = new IGMDataItem_String(spell.Name, SIZE[pos], color);
                    ITEM[pos, 1] = color != Font.ColorID.White ? new IGMDataItem_Icon(Icons.ID.JunctionSYM, new Rectangle(SIZE[pos].X + SIZE[pos].Width - 75, SIZE[pos].Y, 0, 0)) : null;
                    ITEM[pos, 2] = new IGMDataItem_Int(Source.Magics[spell.ID], new Rectangle(SIZE[pos].X + SIZE[pos].Width - 50, SIZE[pos].Y, 0, 0), spaces: 3);
                    BLANKS[pos] = color == Font.ColorID.Dark_Gray ? true : false;
                    Contents[pos] = spell.ID;
                    pos++;
                }

                private void Get_Sort_Stat()
                {
                    SortMode = InGameMenu_Junction.GetMode();
                    switch (SortMode)
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
                            Stat = Kernel_bin.Stat.ST_Atk;
                            break;

                        case Mode.Mag_EL_D:
                        case Mode.Mag_Pool_EL_D:
                            SortMode = Mode.Mag_Pool_EL_D;
                            break;

                        case Mode.Mag_EL_A:
                        case Mode.Mag_Pool_EL_A:
                            SortMode = Mode.Mag_Pool_EL_A;
                            Stat = Kernel_bin.Stat.EL_Atk;
                            break;
                    }
                }

                private void ModeChangeEvent(object sender, Mode e) => UpdateOnEvent(sender, e);

                private void StatChangeEvent(object sender, Kernel_bin.Stat e) => UpdateOnEvent(sender, null, e);

                private bool Undo()
                {
                    SlotUndoListener?.Invoke(this, InGameMenu_Junction.GetMode());
                    if (Memory.State.Characters != null)
                        Source = Memory.State.Characters[Character];
                    return true;
                }

                private void UpdateOnEvent(object sender, Mode? mode = null, Kernel_bin.Stat? stat = null)
                {
                    mode = mode ?? InGameMenu_Junction.GetMode();
                    if (
                        mode != Mode.Mag_Pool_ST_A &&
                        mode != Mode.Mag_Pool_ST_D &&
                        mode != Mode.Mag_Pool_EL_A &&
                        mode != Mode.Mag_Pool_EL_D &&
                        mode != Mode.Mag_Pool_Stat)
                    {
                        Cursor_Status &= ~Cursor_Status.Enabled;
                    }
                    else
                    {
                        Cursor_Status |= Cursor_Status.Enabled;
                    }
                    if (Memory.State.Characters != null)
                    {
                        Get_Sort_Stat();
                        Stat = stat ?? Stat;
                        Get_Slots_Values();
                        if (SortMode != LastMode || this.Stat != LastStat || Character != LastCharacter)
                            Get_Sort();
                        bool skipundo = false;
                        if (!(SortMode == LastMode && Character == LastCharacter && this.Stat == LastStat && Page == LastPage))
                        {
                            LastCharacter = Character;
                            LastStat = this.Stat;
                            LastPage = Page;
                            LastMode = SortMode;
                            skipundo = Undo();
                            FillMagic();
                            UpdateTitle();
                        }
                        if (
                            mode == Mode.Mag_Pool_ST_A ||
                            mode == Mode.Mag_Pool_ST_D ||
                            mode == Mode.Mag_Pool_EL_A ||
                            mode == Mode.Mag_Pool_EL_D ||
                            mode == Mode.Mag_Pool_Stat)
                        {
                            Generate_Preview(skipundo);
                        }
                    }
                }

                #endregion Methods
            }

            #endregion Classes

        }

        #endregion Classes

    }
}
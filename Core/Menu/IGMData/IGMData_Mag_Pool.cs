using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenVIII
{
    public class IGMData_Mag_Pool : IGMData_Pool<Saves.CharacterData, byte>
    {
        #region Fields

        private const Font.ColorID @default = Font.ColorID.White;
        private const Font.ColorID junctioned = Font.ColorID.Grey;
        private const Font.ColorID nostat = Font.ColorID.Dark_Gray;
        private bool Battle = false;
        private bool eventAdded = false;
        private bool skipReinit = false;

        #endregion Fields

        #region Properties

        private int Targets_Window => Rows;

        #endregion Properties

        #region Methods

        private void addMagic(ref int pos, Kernel_bin.Magic_Data spell, Font.ColorID color = @default)
        {
            bool j = false;
            if (color == @default && Source.Stat_J.ContainsValue(spell.ID))
            {
                //spell is junctioned
                if (!Battle)
                    color = junctioned;
                j = true;
            }
            ITEM[pos, 0] = new IGMDataItem_String(spell.Name, SIZE[pos], color);
            ITEM[pos, 1] = j ? new IGMDataItem_Icon(Icons.ID.JunctionSYM, new Rectangle(SIZE[pos].X + SIZE[pos].Width - 75, SIZE[pos].Y, 0, 0)) : null;
            ITEM[pos, 2] = new IGMDataItem_Int(Source.Magics[spell.ID], new Rectangle(SIZE[pos].X + SIZE[pos].Width - 50, SIZE[pos].Y, 0, 0), spaces: 3);
            //makes it so you cannot junction a magic to a stat that does nothing.
            BLANKS[pos] = color == nostat ? true : false;
            Contents[pos] = spell.ID;
            pos++;
        }

        private void Get_Sort_Stat()
        {
            if (Battle)
            {
                //SortMode = IGM_Junction.Mode.Mag_Pool_Stat;
            }
            else
            {
                SortMode = (IGM_Junction.Mode)Menu.IGM_Junction.GetMode();
                switch (SortMode)
                {
                    default:
                    case IGM_Junction.Mode.Mag_Stat:
                    case IGM_Junction.Mode.Mag_Pool_Stat:
                        SortMode = IGM_Junction.Mode.Mag_Pool_Stat;
                        break;

                    case IGM_Junction.Mode.Mag_ST_D:
                    case IGM_Junction.Mode.Mag_Pool_ST_D:
                        SortMode = IGM_Junction.Mode.Mag_Pool_ST_D;
                        break;

                    case IGM_Junction.Mode.Mag_ST_A:
                    case IGM_Junction.Mode.Mag_Pool_ST_A:
                        SortMode = IGM_Junction.Mode.Mag_Pool_ST_A;
                        Stat = Kernel_bin.Stat.ST_Atk;
                        break;

                    case IGM_Junction.Mode.Mag_EL_D:
                    case IGM_Junction.Mode.Mag_Pool_EL_D:
                        SortMode = IGM_Junction.Mode.Mag_Pool_EL_D;
                        break;

                    case IGM_Junction.Mode.Mag_EL_A:
                    case IGM_Junction.Mode.Mag_Pool_EL_A:
                        SortMode = IGM_Junction.Mode.Mag_Pool_EL_A;
                        Stat = Kernel_bin.Stat.EL_Atk;
                        break;
                }
            }
        }

        private void StatChangeEvent(object sender, Kernel_bin.Stat e) => UpdateOnEvent(sender, null, e);

        private bool Undo()
        {
            SlotUndoListener?.Invoke(this, (IGM_Junction.Mode)(Menu.IGM_Junction.GetMode()));
            if (Memory.State.Characters != null && Damageable.GetCharacterData(out Saves.CharacterData c))
                Source = c;
            return true;
        }

        private void UpdateOnEvent(object sender, IGM_Junction.Mode? mode = null, Kernel_bin.Stat? stat = null)
        {
            mode = mode ?? (IGM_Junction.Mode)Menu.IGM_Junction.GetMode();
            if ((
                mode == IGM_Junction.Mode.Mag_Pool_ST_A ||
                mode == IGM_Junction.Mode.Mag_Pool_ST_D ||
                mode == IGM_Junction.Mode.Mag_Pool_EL_A ||
                mode == IGM_Junction.Mode.Mag_Pool_EL_D ||
                mode == IGM_Junction.Mode.Mag_Pool_Stat) || Battle && Enabled)

            {
                Cursor_Status |= Cursor_Status.Enabled;
            }
            else
            {
                Cursor_Status &= ~Cursor_Status.Enabled;
            }
            if (Memory.State.Characters != null && Damageable != null)
            {
                Get_Sort_Stat();
                Stat = stat ?? Stat;
                Get_Slots_Values();
                if (SortMode != LastMode || this.Stat != LastStat || Damageable != LastCharacter)
                    Get_Sort();
                bool skipundo = false;
                if (Battle || !(SortMode == LastMode && Damageable == LastCharacter && this.Stat == LastStat && Page == LastPage))
                {
                    // goal of these checks were to avoid updating the whole list if we don't need to.
                    LastCharacter = Damageable;
                    LastStat = this.Stat;
                    LastPage = Page;
                    LastMode = SortMode;
                    skipundo = Undo();
                    FillMagic();
                    UpdateTitle();
                }
                if (!Battle && (
                    mode == IGM_Junction.Mode.Mag_Pool_ST_A ||
                    mode == IGM_Junction.Mode.Mag_Pool_ST_D ||
                    mode == IGM_Junction.Mode.Mag_Pool_EL_A ||
                    mode == IGM_Junction.Mode.Mag_Pool_EL_D ||
                    mode == IGM_Junction.Mode.Mag_Pool_Stat))
                {
                    Generate_Preview(skipundo);
                }
            }
        }

        protected override void DrawITEM(int i, int d)
        {
            if (Targets_Window >= i || !Target_Group.Enabled)
                base.DrawITEM(i, d);
        }

        protected override void Init()
        {
            base.Init();
            SIZE[Rows] = SIZE[0];
            SIZE[Rows].Y = Y;
            ITEM[Rows, 2] = new IGMDataItem_Icon(Icons.ID.NUM_, new Rectangle(SIZE[Rows].X + SIZE[Rows].Width - 45, SIZE[Rows].Y, 0, 0), scale: new Vector2(2.5f));

            ITEM[Targets_Window, 0] = new BattleMenus.IGMData_TargetGroup(Damageable);
            BLANKS[Rows] = true;
            Cursor_Status &= ~Cursor_Status.Horizontal;
            Cursor_Status |= Cursor_Status.Vertical;
            Cursor_Status &= ~Cursor_Status.Blinking;
            PointerZIndex = Rows - 1;
        }

        protected override void InitShift(int i, int col, int row)
        {
            base.InitShift(i, col, row);
            SIZE[i].Inflate(-22, -8);
            SIZE[i].Offset(0, 12 + (-8 * row));
            SIZE[i].Height = (int)(12 * TextScale.Y);
        }

        protected override void ModeChangeEvent(object sender, Enum e)
        {
            if (e.GetType() == typeof(IGM_Junction.Mode))
                UpdateOnEvent(sender, (IGM_Junction.Mode)e);
            else if (e.GetType() == typeof(BattleMenu.Mode))
                UpdateOnEvent(sender, null);
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

        #endregion Methods

        public static EventHandler<IGM_Junction.Mode> SlotConfirmListener;

        public static EventHandler<Damageable> SlotRefreshListener;

        public static EventHandler<IGM_Junction.Mode> SlotUndoListener;

        public static EventHandler<Kernel_bin.Stat> StatEventListener;

        public IGMData_Mag_Pool(Rectangle pos, Damageable damageable, bool battle = false) : base(5, 3, new IGMDataItem.Box(pos: pos, title: Icons.ID.MAGIC), 4, 13, damageable)
        {
            Battle = battle;
            skipReinit = true;
            Refresh();
        }

        public IGMData_Mag_Pool() : base(6, 3, new IGMDataItem.Box(pos: new Rectangle(135, 150, 300, 192), title: Icons.ID.MAGIC), 4, 13)
        {
        }

        public Damageable LastCharacter { get; private set; }

        public IGM_Junction.Mode LastMode { get; private set; }

        public int LastPage { get; private set; }

        public Kernel_bin.Stat LastStat { get; private set; }

        public IEnumerable<Kernel_bin.Magic_Data> Sort { get; private set; }

        public IGM_Junction.Mode SortMode { get; private set; }

        public BattleMenus.IGMData_TargetGroup Target_Group => (BattleMenus.IGMData_TargetGroup)(((IGMData)ITEM[Targets_Window, 0]));

        public Kernel_bin.Stat Stat { get; private set; }

        public void FillMagic()
        {
            int pos = 0;
            int skip = Page * Rows;

            if (Battle || Sort == null)
                for (int i = 0; pos < Rows && i < Source.Magics.Count; i++)
                {
                    // magic id and count
                    KeyValuePair<byte, byte> dat = Source.Magics[i];
                    // if invalid
                    if (dat.Key == 0 || Kernel_bin.MagicData.Count <= dat.Key || dat.Value == 0 || skip-- > 0) continue;
                    addMagic(ref pos, Kernel_bin.MagicData[dat.Key], @default);
                }
            else
                foreach (Kernel_bin.Magic_Data i in Sort)
                {
                    if (pos >= Rows) break;
                    if (skip-- > 0) continue;
                    if (Source.Magics.ContainsKey(i.ID) && i.ID > 0 && skip-- <= 0)
                    {
                        switch (SortMode)
                        {
                            case IGM_Junction.Mode.Mag_Pool_Stat:
                                if (i.J_Val[Stat] == 0)
                                    addMagic(ref pos, i, nostat);
                                else
                                    addMagic(ref pos, i, @default);
                                break;

                            case IGM_Junction.Mode.Mag_Pool_EL_D:
                                if (i.J_Val[Stat] * i.EL_Def.Count() == 0)
                                    addMagic(ref pos, i, nostat);
                                else
                                    addMagic(ref pos, i, @default);
                                break;

                            case IGM_Junction.Mode.Mag_Pool_EL_A:
                                if (i.J_Val[Stat] * i.EL_Atk.Count() == 0)
                                    addMagic(ref pos, i, nostat);
                                else
                                    addMagic(ref pos, i, @default);
                                break;

                            case IGM_Junction.Mode.Mag_Pool_ST_D:
                                if (i.J_Val[Stat] * i.ST_Def.Count() == 0)
                                    addMagic(ref pos, i, nostat);
                                else
                                    addMagic(ref pos, i, @default);
                                break;

                            case IGM_Junction.Mode.Mag_Pool_ST_A:
                                if (i.J_Val[Stat] * i.ST_Atk.Count() == 0)
                                    addMagic(ref pos, i, nostat);
                                else
                                    addMagic(ref pos, i, @default);
                                break;

                            default:
                                addMagic(ref pos, i, @default);
                                break;
                        }
                    }
                }
            for (; pos < Rows; pos++)
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
                    SlotRefreshListener?.Invoke(this,Damageable);
                }
            }
        }

        public void Get_Slots_Values()
        {
            if(Damageable.GetCharacterData(out Saves.CharacterData c))
            Source = c;
        }

        public void Get_Sort()
        {
            if (!Battle)
                switch (SortMode)
                {
                    case IGM_Junction.Mode.Mag_Pool_Stat:
                        if (Stat != Kernel_bin.Stat.None)
                            Sort = Source.SortedMagic(Stat);
                        break;

                    case IGM_Junction.Mode.Mag_Pool_EL_D:
                        Sort = Source.SortedMagic(Kernel_bin.Stat.EL_Def_1);
                        break;

                    case IGM_Junction.Mode.Mag_Pool_EL_A:
                        Sort = Source.SortedMagic(Kernel_bin.Stat.EL_Atk);
                        break;

                    case IGM_Junction.Mode.Mag_Pool_ST_D:
                        Sort = Source.SortedMagic(Kernel_bin.Stat.ST_Def_1);
                        break;

                    case IGM_Junction.Mode.Mag_Pool_ST_A:
                        Sort = Source.SortedMagic(Kernel_bin.Stat.ST_Atk);
                        break;

                    default:
                        Sort = Kernel_bin.MagicData.AsEnumerable();
                        break;
                }
        }

        public override bool Inputs()
        {
            bool ret = false;
            if (InputITEM(Targets_Window, 0, ref ret))
            { }
            else
            {
                Cursor_Status |= Cursor_Status.Enabled;
                return base.Inputs();
            }
            return ret;
        }

        public override bool Inputs_CANCEL()
        {
            if (Memory.State.Characters != null)
            {
                base.Inputs_CANCEL();
                if (Battle)
                {
                    Hide();
                    return true;
                }
                else
                {
                    SlotUndoListener?.Invoke(this, (IGM_Junction.Mode)Menu.IGM_Junction.GetMode());
                    SlotConfirmListener?.Invoke(this, (IGM_Junction.Mode)Menu.IGM_Junction.GetMode());
                    SlotRefreshListener?.Invoke(this, Damageable);
                    switch (SortMode)
                    {
                        case IGM_Junction.Mode.Mag_Pool_Stat:
                            Menu.IGM_Junction.SetMode(IGM_Junction.Mode.Mag_Stat);
                            break;

                        case IGM_Junction.Mode.Mag_Pool_EL_A:
                            Menu.IGM_Junction.SetMode(IGM_Junction.Mode.Mag_EL_A);
                            break;

                        case IGM_Junction.Mode.Mag_Pool_EL_D:
                            Menu.IGM_Junction.SetMode(IGM_Junction.Mode.Mag_EL_D);
                            break;

                        case IGM_Junction.Mode.Mag_Pool_ST_A:
                            Menu.IGM_Junction.SetMode(IGM_Junction.Mode.Mag_ST_A);
                            break;

                        case IGM_Junction.Mode.Mag_Pool_ST_D:
                            Menu.IGM_Junction.SetMode(IGM_Junction.Mode.Mag_ST_D);
                            break;
                    }

                    Cursor_Status &= ~Cursor_Status.Enabled;
                    if(Damageable.GetCharacterData(out Saves.CharacterData c))
                        Source = c;

                    return true;
                }
            }

            return false;
        }

        public override bool Inputs_OKAY()
        {
            if (Battle)
            {
                Target_Group.SelectTargetWindows(Kernel_bin.MagicData[Contents[CURSOR_SELECT]]);
                Target_Group.ShowTargetWindows();
            }
            else
            if (Memory.State.Characters != null)
            {
                if (!BLANKS[CURSOR_SELECT])
                {
                    skipsnd = true;
                    init_debugger_Audio.PlaySound(31);
                    base.Inputs_OKAY();
                    SlotConfirmListener?.Invoke(this, (IGM_Junction.Mode)Menu.IGM_Junction.GetMode());
                    if (Menu.IGM_Junction.GetMode().Equals(IGM_Junction.Mode.Mag_Pool_Stat))
                    {
                        Menu.IGM_Junction.SetMode(IGM_Junction.Mode.Mag_Stat);
                    }
                    else if (Menu.IGM_Junction.GetMode().Equals(IGM_Junction.Mode.Mag_Pool_EL_A) || Menu.IGM_Junction.GetMode().Equals(IGM_Junction.Mode.Mag_Pool_EL_D))
                    {
                        Menu.IGM_Junction.SetMode(IGM_Junction.Mode.Mag_EL_A);
                    }
                    else if (Menu.IGM_Junction.GetMode().Equals(IGM_Junction.Mode.Mag_Pool_ST_A) || Menu.IGM_Junction.GetMode().Equals(IGM_Junction.Mode.Mag_Pool_ST_D))
                    {
                        Menu.IGM_Junction.SetMode(IGM_Junction.Mode.Mag_ST_A);
                    }
                    Cursor_Status &= ~Cursor_Status.Enabled;
                    Menu.IGM_Junction.Refresh();
                    return true;
                }
            }
            return false;
        }

        //public IGMData Values { get; private set; } = null;
        public override void Refresh()
        {
            if (!skipReinit)
            {
                if (!Battle && !eventAdded && Menu.IGM_Junction != null)
                {
                    Menu.IGM_Junction.ModeChangeHandler += ModeChangeEvent;
                    StatEventListener += StatChangeEvent;
                    eventAdded = true;
                }
                base.Refresh();
                UpdateTitle();
            }
            else skipReinit = false;
        }

        public override void Reset()
        {
            HideChildren();
            Hide();
            base.Reset();
        }


        public override void UpdateTitle()
        {
            base.UpdateTitle();
            if (Pages == 1)
            {
                ((IGMDataItem.Box)CONTAINER).Title = Icons.ID.MAGIC;
                //ITEM[Count - 1, 0] = ITEM[Count - 2, 0] = null;
            }
            else
                if (Page < Pages)
                ((IGMDataItem.Box)CONTAINER).Title = (Icons.ID)((int)Icons.ID.MAGIC_PG1 + Page);
        }
    }
}
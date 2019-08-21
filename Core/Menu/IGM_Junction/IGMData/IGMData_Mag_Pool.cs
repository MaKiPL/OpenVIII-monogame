using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenVIII
{
    public class IGMData_Mag_Pool : IGMData_Pool<Saves.CharacterData, byte>
    {
        private const Font.ColorID nostat = Font.ColorID.Dark_Gray;
        private const Font.ColorID @default = Font.ColorID.White;
        private const Font.ColorID junctioned = Font.ColorID.Grey;
        #region Fields

        public static EventHandler<IGM_Junction.Mode> SlotConfirmListener;
        public static EventHandler<IGM_Junction.Mode> SlotReinitListener;
        public static EventHandler<IGM_Junction.Mode> SlotUndoListener;
        public static EventHandler<Kernel_bin.Stat> StatEventListener;
        private bool Battle = false;
        private bool eventAdded = false;
        private bool skipReinit = false;

        #endregion Fields

        #region Constructors

        public IGMData_Mag_Pool(Rectangle pos, Characters character = Characters.Blank, Characters? visablecharacter = null, bool battle = false) : base(5, 3, new IGMDataItem_Box(pos: pos, title: Icons.ID.MAGIC), 4, 13, character, visablecharacter)
        {
            Battle = battle;
            skipReinit = true;
            Refresh();
        }

        public IGMData_Mag_Pool() : base(5, 3, new IGMDataItem_Box(pos: new Rectangle(135, 150, 300, 192), title: Icons.ID.MAGIC), 4, 13)
        {
        }

        #endregion Constructors

        #region Properties

        public Characters LastCharacter { get; private set; }

        public IGM_Junction.Mode LastMode { get; private set; }

        public int LastPage { get; private set; }

        public Kernel_bin.Stat LastStat { get; private set; }

        public IEnumerable<Kernel_bin.Magic_Data> Sort { get; private set; }

        public IGM_Junction.Mode SortMode { get; private set; }

        public Kernel_bin.Stat Stat { get; private set; }

        #endregion Properties

        #region Methods

        public void FillMagic()
        {
            int pos = 0;
            int skip = Page * rows;

            if (Battle || Sort == null)
                for (int i = 0; pos < rows && i < Source.Magics.Count; i++)
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
                    if (pos >= rows) break;
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
                    SlotReinitListener?.Invoke(this, (IGM_Junction.Mode)Menu.IGM_Junction.GetMode());
                }
            }
        }

        public void Get_Slots_Values() =>
            Source = Memory.State.Characters[Character];

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
            Cursor_Status |= Cursor_Status.Enabled;
            return base.Inputs();
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
                    SlotReinitListener?.Invoke(this, (IGM_Junction.Mode)Menu.IGM_Junction.GetMode());
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
                    Source = Memory.State.Characters[Character];

                    return true;
                }
            }

            return false;
        }

        public override bool Inputs_OKAY()
        {
            if (Battle)
            {
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

        public override void UpdateTitle()
        {
            base.UpdateTitle();
            if (Pages == 1)
            {
                ((IGMDataItem_Box)CONTAINER).Title = Icons.ID.MAGIC;
                //ITEM[Count - 1, 0] = ITEM[Count - 2, 0] = null;
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
            if (Memory.State.Characters != null)
                Source = Memory.State.Characters[Character];
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
            if (Memory.State.Characters != null && Character != Characters.Blank)
            {
                Get_Sort_Stat();
                Stat = stat ?? Stat;
                Get_Slots_Values();
                if (SortMode != LastMode || this.Stat != LastStat || Character != LastCharacter)
                    Get_Sort();
                bool skipundo = false;
                if (Battle || !(SortMode == LastMode && Character == LastCharacter && this.Stat == LastStat && Page == LastPage))
                {
                    // goal of these checks were to avoid updating the whole list if we don't need to.
                    LastCharacter = Character;
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

        #endregion Methods
    }
}
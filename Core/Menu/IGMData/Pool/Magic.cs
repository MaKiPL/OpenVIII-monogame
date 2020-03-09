using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenVIII.IGMData.Pool
{
    public partial class Magic : IGMData.Pool.Base<Saves.CharacterData, byte>
    {
        #region Fields

        private const Font.ColorID @default = Font.ColorID.White;

        private const Font.ColorID junctioned = Font.ColorID.Grey;

        private const Font.ColorID nostat = Font.ColorID.Dark_Grey;


        private bool eventAdded = false;

        private bool skipRefresh = false;

        #endregion Fields

        #region Destructors

        ~Magic()
        {
            if (eventAdded)
            {
                if (!Battle)
                {
                    Menu.IGM_Junction.ModeChangeHandler -= ModeChangeEvent;
                    StatEventListener -= StatChangeEvent;
                }
                else if (Damageable != null)
                    Damageable.BattleModeChangeEventHandler -= ModeChangeEvent;
            }
        }

        #endregion Destructors

        #region Events

        public static event EventHandler<IGM_Junction.Mode> SlotConfirmListener;

        public static event EventHandler<Damageable> SlotRefreshListener;

        public static event EventHandler<IGM_Junction.Mode> SlotUndoListener;

        public static event EventHandler<Kernel.Stat> StatEventListener;

        #endregion Events

        #region Properties

        public Damageable LastCharacter { get; private set; }

        public IGM_Junction.Mode LastMode { get; private set; }

        public int LastPage { get; private set; }

        public Kernel.Stat LastStat { get; private set; }


        public IEnumerable<Kernel.MagicData> Sort { get; private set; }


        public IGM_Junction.Mode SortMode { get; private set; }

        public Kernel.Stat Stat { get; private set; }

        public IGMData.Target.Group Target_Group => (IGMData.Target.Group)(((IGMData.Base)ITEM[Targets_Window, 0]));

        private int Targets_Window => Rows;

        #endregion Properties

        #region Methods

        public static void ChangeStat(Kernel.Stat stat) => StatEventListener?.Invoke(null, stat);

        public static Magic Create(Rectangle pos, Damageable damageable, bool battle = false)
        {
            return Create<Magic>(5, 3, new IGMDataItem.Box { Pos = pos, Title = Icons.ID.MAGIC }, 4, 13, damageable,battle:battle);
        }

        public static Magic Create() => Create<Magic>(6, 3, new IGMDataItem.Box { Pos = new Rectangle(135, 150, 300, 192), Title = Icons.ID.MAGIC }, 4, 13);

        public void FillMagic()
        {
            int pos = 0;
            int skip = Page * Rows;

            if (Battle || Sort == null)
                if (Damageable.GetEnemy(out Enemy e))
                {

                    bool add(Kernel.MagicData magic)

                    {
                        if (pos >= Rows)
                            return false;
                        if (skip-- <= 0)
                        {
                            addMagic(ref pos, magic);
                        }
                        return true;
                    }

                    HashSet<Kernel.MagicData> Unique_Magic = new HashSet<Kernel.MagicData>();

                    foreach (var m in e.Abilities.Where(x => x.MAGIC != null))
                        Unique_Magic.Add(m.MAGIC);
                    foreach (var m in e.DrawList.Where(x => x.DATA != null))
                        Unique_Magic.Add(m.DATA);
                    foreach(var m in Unique_Magic)
                    {
                        if (!add(m))
                            break;
                    }
                    ITEM[Rows, 2].Hide();
                    DefaultPages = Unique_Magic.Count / Rows;
                    UpdateTitle();
                }
                else
                    for (int i = 0; pos < Rows && Source?.Magics != null && i < Source.Magics.Count; i++)
                    {
                        // Magic ID and Count
                        KeyValuePair<byte, byte> dat = Source.Magics[i];
                        // if invalid
                        if (dat.Key == 0 || Memory.Kernel_Bin.MagicData.Count <= dat.Key || dat.Value == 0 || skip-- > 0) continue;
                        addMagic(ref pos, Memory.Kernel_Bin.MagicData[dat.Key], @default);
                    }
            else

                foreach (Kernel_bin.Magic_Data i in Sort)


                {
                    if (pos >= Rows) break;
                    if (skip-- > 0) continue;
                    if (Source.Magics.ContainsKey(i.MagicDataID) && i.MagicDataID > 0 && skip-- <= 0)
                    {
                        switch (SortMode)
                        {
                            case IGM_Junction.Mode.Mag_Pool_Stat:
                                if (i.JVal[Stat] == 0)
                                    addMagic(ref pos, i, nostat);
                                else
                                    addMagic(ref pos, i, @default);
                                break;

                            case IGM_Junction.Mode.Mag_Pool_EL_D:
                                if (i.JVal[Stat] * i.ElDef.Count() == 0)
                                    addMagic(ref pos, i, nostat);
                                else
                                    addMagic(ref pos, i, @default);
                                break;

                            case IGM_Junction.Mode.Mag_Pool_EL_A:
                                if (i.JVal[Stat] * i.ElAtk.Count() == 0)
                                    addMagic(ref pos, i, nostat);
                                else
                                    addMagic(ref pos, i, @default);
                                break;

                            case IGM_Junction.Mode.Mag_Pool_ST_D:
                                if (i.JVal[Stat] * i.StDef.Count() == 0)
                                    addMagic(ref pos, i, nostat);
                                else
                                    addMagic(ref pos, i, @default);
                                break;

                            case IGM_Junction.Mode.Mag_Pool_ST_A:
                                if (i.JVal[Stat] * i.StAtk.Count() == 0)
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
                ITEM[pos, 0].Hide();
                ITEM[pos, 1].Hide();
                ITEM[pos, 2].Hide();
                BLANKS[pos] = true;
                Contents[pos] = 0;
            }
        }

        public void Generate_Preview(bool skipundo = false)
        {
            if (Stat != Kernel.Stat.None && CURSOR_SELECT < Contents.Length)
            {
                Cursor_Status |= Cursor_Status.Enabled;
                if (Source.StatJ[Stat] != Contents[CURSOR_SELECT])
                {
                    if (!skipundo)
                    {
                        Undo();
                        skipundo = false;
                    }
                    Source.JunctionSpell(Stat, Contents[CURSOR_SELECT]);
                    SlotRefreshListener?.Invoke(this, Damageable);
                }
            }
        }

        public void Get_Slots_Values()
        {
            if (Damageable.GetCharacterData(out Saves.CharacterData c))
                Source = c;
        }

        public void Get_Sort()
        {
            if (!Battle)
                switch (SortMode)
                {
                    case IGM_Junction.Mode.Mag_Pool_Stat:
                        if (Stat != Kernel.Stat.None)
                            Sort = Source.SortedMagic(Stat);
                        break;

                    case IGM_Junction.Mode.Mag_Pool_EL_D:
                        Sort = Source.SortedMagic(Kernel.Stat.EL_Def_1);
                        break;

                    case IGM_Junction.Mode.Mag_Pool_EL_A:
                        Sort = Source.SortedMagic(Kernel.Stat.EL_Atk);
                        break;

                    case IGM_Junction.Mode.Mag_Pool_ST_D:
                        Sort = Source.SortedMagic(Kernel.Stat.ST_Def_1);
                        break;

                    case IGM_Junction.Mode.Mag_Pool_ST_A:
                        Sort = Source.SortedMagic(Kernel.Stat.ST_Atk);
                        break;

                    default:
                        Sort = Memory.Kernel_Bin.MagicData.AsEnumerable();
                        break;
                }
        }

        public override bool Inputs()
        {
            bool ret = false;
            if (InputITEM(Target_Group, ref ret))
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
                    if (Damageable.GetCharacterData(out Saves.CharacterData c))
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
                Target_Group.SelectTargetWindows(Memory.Kernel_Bin.MagicData[Contents[CURSOR_SELECT]]);
                Target_Group.ShowTargetWindows();
            }
            else
            if (Memory.State.Characters != null)
            {
                if (!BLANKS[CURSOR_SELECT])
                {
                    skipsnd = true;
                    AV.Sound.Play(31);
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

        public override void ModeChangeEvent(object sender, Enum e)
        {
            if (e.GetType() == typeof(IGM_Junction.Mode))
                UpdateOnEvent(sender, (IGM_Junction.Mode)e);
            else if (e.GetType() == typeof(Damageable.BattleMode))
                UpdateOnEvent(sender, null);
        }

        //public IGMData Values { get; private set; } = null;
        public override void Refresh()
        {
            if (!skipRefresh)
            {
                if (!eventAdded && Menu.IGM_Junction != null)
                {
                    if (!Battle)
                    {
                        Menu.IGM_Junction.ModeChangeHandler += ModeChangeEvent;
                        StatEventListener += StatChangeEvent; eventAdded = true;
                    }
                    else if (Damageable != null)
                    {
                        Damageable.BattleModeChangeEventHandler += ModeChangeEvent; eventAdded = true;
                    }
                }
                base.Refresh();
                UpdateTitle();
            }
            else skipRefresh = false;
        }

        public override void Refresh(Damageable damageable)
        {
            if (Battle && eventAdded && damageable != Damageable)
            {
                eventAdded = false;
                if (Damageable != null)
                    Damageable.BattleModeChangeEventHandler -= ModeChangeEvent;
            }
            base.Refresh(damageable);
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
            ITEM[Rows, 2] = new IGMDataItem.Icon { Data = Icons.ID.NUM_, Pos = new Rectangle(SIZE[Rows].X + SIZE[Rows].Width - 45, SIZE[Rows].Y, 0, 0), Scale = new Vector2(2.5f) };

            for (int pos = 0; pos < Rows; pos++)
            {
                ITEM[pos, 0] = new IGMDataItem.Text { Pos = SIZE[pos] };
                ITEM[pos, 0].Hide();
                ITEM[pos, 1] = new IGMDataItem.Icon { Data = Icons.ID.JunctionSYM, Pos = new Rectangle(SIZE[pos].X + SIZE[pos].Width - 75, SIZE[pos].Y, 0, 0) };
                ITEM[pos, 1].Hide();
                ITEM[pos, 2] = new IGMDataItem.Integer { Pos = new Rectangle(SIZE[pos].X + SIZE[pos].Width - 50, SIZE[pos].Y, 0, 0), Spaces = 3 };
                ITEM[pos, 2].Hide();
            }

            ITEM[Targets_Window, 0] = IGMData.Target.Group.Create(Damageable);
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


        private void addMagic(ref int pos, Kernel.MagicData spell, Font.ColorID color = @default)

        {
            if (!Damageable.GetEnemy(out Enemy e))
            {
                e = null;
            }
            bool j = false;

            if (color == @default && e == null && Source != null && Source.StatJ.ContainsValue(spell.MagicDataID))

            {
                //spell is junctioned
                if (!Battle)
                    color = junctioned;
                j = true;
            }
            ((IGMDataItem.Text)ITEM[pos, 0]).Data = spell.Name;
            ((IGMDataItem.Text)ITEM[pos, 0]).FontColor = color;
            ITEM[pos, 0].Show();
            if (j)
                ITEM[pos, 1].Show();
            else
                ITEM[pos, 1].Hide();
            int count = Source?.Magics[spell.MagicDataID] ?? 0;
            ((IGMDataItem.Integer)ITEM[pos, 2]).Data = count;
            if (count <= 0)
                ITEM[pos, 2].Hide();
            else
                ITEM[pos, 2].Show();
            //makes it so you cannot junction a magic to a stat that does nothing.
            BLANKS[pos] = color == nostat ? true : false;
            Contents[pos] = spell.MagicDataID;
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
                        Stat = Kernel.Stat.ST_Atk;
                        break;

                    case IGM_Junction.Mode.Mag_EL_D:
                    case IGM_Junction.Mode.Mag_Pool_EL_D:
                        SortMode = IGM_Junction.Mode.Mag_Pool_EL_D;
                        break;

                    case IGM_Junction.Mode.Mag_EL_A:
                    case IGM_Junction.Mode.Mag_Pool_EL_A:
                        SortMode = IGM_Junction.Mode.Mag_Pool_EL_A;
                        Stat = Kernel.Stat.EL_Atk;
                        break;
                }
            }
        }

        private void StatChangeEvent(object sender, Kernel.Stat e) => UpdateOnEvent(sender, null, e);

        private bool Undo()
        {
            SlotUndoListener?.Invoke(this, (IGM_Junction.Mode)(Menu.IGM_Junction.GetMode()));
            if (Memory.State.Characters != null && Damageable.GetCharacterData(out Saves.CharacterData c))
                Source = c;
            return true;
        }

        private void UpdateOnEvent(object sender, IGM_Junction.Mode? mode = null, Kernel.Stat? stat = null)
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

        #endregion Methods
    }
}

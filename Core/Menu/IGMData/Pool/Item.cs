using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenVIII.IGMData.Pool
{
    public class Item : IGMData.Pool.Base<Saves.Data, Item_In_Menu>
    {
        #region Fields

        private FF8String[] _helpStr;

        private bool eventSet = false;

        #endregion Fields

        #region Events

        public event EventHandler<KeyValuePair<Item_In_Menu, FF8String>> ItemChangeHandler;

        #endregion Events

        #region Properties

        public IReadOnlyList<FF8String> HelpStr => _helpStr;
        public IGMData.Target.Group Target_Group => (IGMData.Target.Group)(((IGMData.Base)ITEM[Targets_Window, 0]));
        private int Targets_Window => Count - 3;

        #endregion Properties

        #region Methods

        public static Item Create(Rectangle pos, Damageable damageable = null, bool battle = false, int count = 4)
        {
            Item r = Create<Item>(count + 1, 3, new IGMDataItem.Box { Pos = pos, Title = Icons.ID.ITEM }, count, 198 / count + 1, damageable,battle:battle);
            if (battle)
                r.ITEM[r.Targets_Window, 0] = IGMData.Target.Group.Create(r.Damageable);
            return r;
        }

        public static Item Create(Damageable damageable = null) => Create(new Rectangle(5, 150, 415, 480), damageable: damageable, battle: false, count: 13);

        public override void Draw() => base.Draw();

        public override void HideChildren()
        {
            if (Enabled)
            {
                if (!skipdata)
                {
                    Target_Group.HideChildren();
                    Target_Group.Hide();
                }
            }
        }

        public override bool Inputs()
        {
            bool ret = false;
            if (InputITEM(Target_Group, ref ret))
            {
            }
            else
            {
                Cursor_Status &= ~Cursor_Status.Blinking;
                Cursor_Status |= Cursor_Status.Enabled;
                return base.Inputs();
            }
            return ret;
        }

        public override bool Inputs_CANCEL()
        {
            if (Battle)
            {
                Hide();
            }
            else
            {
                Menu.IGM_Items.SetMode(IGM_Items.Mode.TopMenu);
                base.Inputs_CANCEL();
            }
            return true;
        }

        public override bool Inputs_OKAY()
        {
            Item_In_Menu item = Contents[CURSOR_SELECT];
            if (Battle)
            {
                Target_Group?.SelectTargetWindows(item);
                Target_Group?.ShowTargetWindows();
            }
            if (item.Target == Item_In_Menu._Target.None)
                return false;
            base.Inputs_OKAY();
            Menu.IGM_Items.SetMode(IGM_Items.Mode.UseItemOnTarget);
            return true;
        }

        public override void ModeChangeEvent(object sender, Enum e)
        {
            if (e.Equals(IGM_Items.Mode.SelectItem) || Battle)
            {
                Cursor_Status |= Cursor_Status.Enabled;
            }
            else if (e.Equals(IGM_Items.Mode.UseItemOnTarget))
            {
                Cursor_Status |= Cursor_Status.Blinking;
            }
            else
            {
                Cursor_Status &= ~Cursor_Status.Enabled;
            }
        }

        public override void Refresh()
        {
            if (!Battle && !eventSet && Menu.IGM_Items != null)
            {
                Menu.IGM_Items.ModeChangeHandler += ModeChangeEvent;
                Menu.IGM_Items.RefreshCompletedHandler += RefreshCompletedEvent;
                eventSet = true;
            }
            base.Refresh();
            Source = Memory.State;
            if (Source != null && Source.Items != null)
            {
                ((IGMDataItem.Box)CONTAINER).Title = Pages <= 1 ? (Icons.ID?)Icons.ID.ITEM : (Icons.ID?)(Icons.ID.ITEM_PG1 + (byte)Page);
                byte pos = 0;
                short skip = checked((short)(Page * Rows));
                Enemy e = null;
                if (Damageable?.GetEnemy(out e) ?? false)
                {
                    sbyte addEnemyItem(Item_In_Menu itemdata)
                    {
                        Saves.Item item = new Saves.Item (itemdata.ID, byte.MaxValue );
                        return AddItem(ref pos, ref skip, item, itemdata);
                    }
                    HashSet<Item_In_Menu> items = new HashSet<Item_In_Menu>();
                    foreach(var a in e.Abilities.Where(x=>x.Item != null))
                        items.Add(a.Item.Value);
                    foreach (var a in e.DropList.Where(x => x.ID != 0 && x.Data != null))
                        items.Add(a.Data.Value);
                    foreach (var a in e.MugList.Where(x => x.ID != 0 && x.Data != null))
                        items.Add(a.Data.Value);
                    foreach (var i in items)
                        if (addEnemyItem(i) == 0) break;
                    NUM_.Hide();
                    DefaultPages = items.Count / Rows;
                }
                else
                for (byte i = 0; pos < Rows && i < Source.Items.Count; i++)
                    {
                        Saves.Item item = Source.Items[i];
                        Item_In_Menu itemdata = item.Data ?? new Item_In_Menu();
                        if (AddItem(ref pos, ref skip, item, itemdata) == 0) break;
                    }
                for (; pos < Rows; pos++)
                {
                    ((IGMDataItem.Integer)(ITEM[pos, 1])).Hide();
                    if (pos == 0) return; // if page turning. this till be enough to trigger a try next page.
                    ((IGMDataItem.Text)(ITEM[pos, 0])).Data = null;
                    ((IGMDataItem.Integer)(ITEM[pos, 1])).Data = 0;
                    ((IGMDataItem.Text)(ITEM[pos, 0])).Icon = Icons.ID.None;
                    BLANKS[pos] = true;
                    ITEM[pos, 1].Hide();
                    ITEM[pos, 0].Hide();
                }
            }
        }

        private sbyte AddItem(ref byte pos, ref short skip, Saves.Item item, Item_In_Menu itemdata)
        {
            if ((pos >= Rows))  //reached max rows.
                return 0;
            if ((item.ID == 0 || item.QTY == 0) || // skip empty values.
                (Battle && itemdata.Battle == null) || // if battle mode skip nonbattle items.
                (itemdata.ID == 0) || // skip empty values.
                (skip-- > 0)) //skip items that are on prev pages.
                return -1;
            Enemy e = null;
            if ((Damageable?.GetEnemy(out e) ?? false))
            {
            }
            Font.ColorID color = Font.ColorID.White;
            byte palette = itemdata.Palette;
            if (!itemdata.ValidTarget(Battle))
            {
                color = Font.ColorID.Grey;
                BLANKS[pos] = true;
                palette = itemdata.Faded_Palette;
            }
            else
                BLANKS[pos] = false;
            ((IGMDataItem.Text)(ITEM[pos, 0])).Data = itemdata.Name;
            ((IGMDataItem.Text)(ITEM[pos, 0])).Icon = itemdata.Icon;
            ((IGMDataItem.Text)(ITEM[pos, 0])).Palette = palette;
            ((IGMDataItem.Text)(ITEM[pos, 0])).FontColor = color;
            ((IGMDataItem.Integer)(ITEM[pos, 1])).Data = item.QTY;
            if (e != null)
                ITEM[pos, 1].Hide();
            else
                ITEM[pos, 1].Show();
            ((IGMDataItem.Integer)(ITEM[pos, 1])).FontColor = color;
            _helpStr[pos] = itemdata.Description;
            Contents[pos] = itemdata;

            ITEM[pos, 0].Show();
            pos++;
            return 1;
        }

        public override void Reset()
        {
            HideChildren();
            Hide();
            base.Reset();
        }

        protected override void DrawITEM(int i, int d)
        {
            if (Targets_Window >= i || !(Target_Group != null && Target_Group.Enabled))
                base.DrawITEM(i, d);
        }

        protected override void Init()
        {
            base.Init();
            _helpStr = new FF8String[Count];
            for (byte pos = 0; pos < Rows; pos++)
            {
                ITEM[pos, 0] = new IGMDataItem.Text { Pos = SIZE[pos] };
                ITEM[pos, 1] = new IGMDataItem.Integer { Pos = new Rectangle(SIZE[pos].X + SIZE[pos].Width - 60, SIZE[pos].Y, 0, 0), NumType = Icons.NumType.SysFntBig, Spaces = 3 };
            }
            NUM_ = new IGMDataItem.Icon { Data = Icons.ID.NUM_, Pos = new Rectangle(SIZE[Rows - 1].X + SIZE[Rows - 1].Width - 60, Y, 0, 0), Scale = new Vector2(2.5f) };
            PointerZIndex = Rows - 1;
        }
        protected IGMDataItem.Icon NUM_ { get
            { return ((IGMDataItem.Icon)ITEM[Count - 1, 2]); } private set { ITEM[Count - 1, 2] = value; } }
        protected override void InitShift(int i, int col, int row)
        {
            base.InitShift(i, col, row);
            //SIZE[i].Inflate(-18, -20);
            //SIZE[i].Y -= 5 * row;
            SIZE[i].Inflate(-22, -8);
            //SIZE[i].Offset(0, 12 + (-8 * row));
            int v = (int)(12 * TextScale.Y);
            SIZE[i].Height = v;
            SIZE[i].Y = Y + 18 + row * ((Height - 16) / Rows);
        }

        protected override void PAGE_NEXT()
        {
            int cnt = Pages;
            do
            {
                base.PAGE_NEXT();
                Refresh();
                skipsnd = true;
            }
            while (cnt-- > 0 && !((IGMDataItem.Integer)(ITEM[0, 1])).Enabled);
            ItemChangeHandler?.Invoke(this, new KeyValuePair<Item_In_Menu, FF8String>(Contents[CURSOR_SELECT], HelpStr[CURSOR_SELECT]));
        }

        protected override void PAGE_PREV()
        {
            int cnt = Pages;
            do
            {
                base.PAGE_PREV();
                Refresh();

                skipsnd = true;
            }
            while (cnt-- > 0 && !((IGMDataItem.Integer)(ITEM[0, 1])).Enabled);
            ItemChangeHandler?.Invoke(this, new KeyValuePair<Item_In_Menu, FF8String>(Contents[CURSOR_SELECT], HelpStr[CURSOR_SELECT]));
        }

        protected override void SetCursor_select(int value)
        {
            if (value != GetCursor_select())
            {
                base.SetCursor_select(value);
                ItemChangeHandler?.Invoke(this, new KeyValuePair<Item_In_Menu, FF8String>(Contents[value], HelpStr[value]));
            }
        }

        private void RefreshCompletedEvent(object sender, EventArgs e) => ItemChangeHandler?.Invoke(this, new KeyValuePair<Item_In_Menu, FF8String>(Contents[CURSOR_SELECT], HelpStr[CURSOR_SELECT]));

        #endregion Methods
    }
}
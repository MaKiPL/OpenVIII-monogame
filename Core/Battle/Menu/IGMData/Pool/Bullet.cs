using System.Linq;
using Microsoft.Xna.Framework;
using OpenVIII.IGMDataItem;

namespace OpenVIII.IGMData.Pool
{
    public class Bullet : Base<Damageable, ItemInMenu>
    {
        #region Properties

        protected Target.Group Target_Group { get => ((Target.Group)ITEM[Count - 3, 0]); private set => ITEM[Count - 3, 0] = value; }

        #endregion Properties

        #region Methods

        public static Bullet Create(Rectangle pos, Damageable damageable = null, bool battle = false, int count = 4)
        {
            var r = Create<Bullet>(count + 1, 2, new Box { Pos = pos, Title = Icons.ID.SPECIAL }, count, 1, damageable, battle: battle);
            if (battle)
                r.Target_Group = Target.Group.Create(r.Damageable);
            return r;
        }

        public override bool Inputs()
        {
            var ret = false;
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
            base.Inputs_CANCEL();
            Hide();
            return true;
        }

        public override bool Inputs_OKAY()
        {
            base.Inputs_OKAY();
            var item = Contents[CURSOR_SELECT];
            if (!BLANKS[CURSOR_SELECT])
            {
                Target_Group?.SelectTargetWindows(item, true);
                Target_Group?.ShowTargetWindows();
            }
            return true;
        }
        public override void Refresh()
        {
            if (Memory.State?.Items != null)
            {
                var ammo = Memory.State.Items.Where(x => x.Data != null && x.QTY > 0 && x.Data?.ItemType == ItemType.Ammo).OrderBy(x => x.ID).ToList();
                var i = 0;
                var skip = Page * Rows;
                bool AddItem(Saves.Item item)
                {
                    if (i >= Rows) return false;
                    if (skip-- <= 0)
                    {
                        ((Text)ITEM[i, 0]).Data = item.Data?.Name ?? null;
                        ((Text)ITEM[i, 0]).Icon = item.Data?.Icon ?? null;
                        ((Integer)ITEM[i, 1]).Data = item.QTY;
                        Contents[i] = item.Data ?? default;
                        BLANKS[i] = false;
                        ITEM[i, 0].Show();
                        ITEM[i, 1].Show();
                        i++;
                    }
                    return true;
                }
                foreach (var bullet in ammo)
                    AddItem(bullet);
                DefaultPages = ammo.Count / Rows;
                for (; i < Rows; i++)
                {
                    BLANKS[i] = true;
                    ITEM[i, 0].Hide();
                    ITEM[i, 1].Hide();
                }
            }
            UpdateTitle();
            base.Refresh();
        }

        public override void Reset()
        {
            Hide();
            base.Reset();
        }

        public override void UpdateTitle()
        {
            base.UpdateTitle();
            if (Pages == 1)
            {
                ((Box)CONTAINER).Title = Icons.ID.SPECIAL;
            }
            else
                switch (Page)
                {
                    case 0:
                        ((Box)CONTAINER).Title = Icons.ID.SPECIAL_PG1;
                        break;

                    case 1:
                        ((Box)CONTAINER).Title = Icons.ID.SPECIAL_PG2;
                        break;

                    case 2:
                        ((Box)CONTAINER).Title = Icons.ID.SPECIAL_PG3;
                        break;

                    case 3:
                        ((Box)CONTAINER).Title = Icons.ID.SPECIAL_PG4;
                        break;

                    default:
                        ((Box)CONTAINER).Title = Icons.ID.SPECIAL;
                        break;
                }
        }

        protected override void Init()
        {
            base.Init();
            const int widthofnumber = 60;
            for (var i = 0; i < Rows; i++)
            {
                ITEM[i, 0] = new Text
                {
                    Pos = SIZE[i]
                };
                ITEM[i, 0].Hide();
                ITEM[i, 1] = new Integer
                {
                    Pos = new Rectangle(SIZE[i].Right - widthofnumber, SIZE[i].Top, widthofnumber, SIZE[i].Height),
                    NumType = Icons.NumType.SysFntBig,
                    Spaces = 3
                };
                ITEM[i, 1].Hide();
            }
            ITEM[Rows, 1] = new Icon
            {
                Data = Icons.ID.NUM_,
                Pos = new Rectangle(SIZE[0].Right - widthofnumber, CONTAINER.Pos.Top, widthofnumber, SIZE[0].Height)
            };
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
            Refresh();
        }

        protected override void PAGE_PREV()
        {
            base.PAGE_PREV();
            Refresh();
        }

        #endregion Methods
    }
}
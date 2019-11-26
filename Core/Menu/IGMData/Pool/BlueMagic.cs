using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace OpenVIII.IGMData.Pool
{
    /// <summary>
    /// </summary>
    /// <see cref="https://www.youtube.com/watch?v=BhgixAEvuu0"/>
    public class BlueMagic : IGMData.Pool.Base<Saves.Data, Kernel_bin.Blue_magic_Quistis_limit_break>
    {
        #region Fields

        private List<Kernel_bin.Blue_Magic> unlocked = new List<Kernel_bin.Blue_Magic>();

        #endregion Fields

        #region Properties

        public IGMData.Target.Group Target_Group => (IGMData.Target.Group)(((IGMData.Base)ITEM[Rows, 0]));

        #endregion Properties

        #region Methods

        public static BlueMagic Create(Rectangle pos, Damageable damageable, bool battle = false) =>
            Create<BlueMagic>(5, 1, new IGMDataItem.Box { Pos = pos, Title = Icons.ID.SPECIAL }, 4, 4, damageable);

        public override bool Inputs()
        {
            if (Target_Group.Enabled)
            {
                Cursor_Status |= Cursor_Status.Enabled | Cursor_Status.Blinking;
                return Target_Group.Inputs();
            }
            else
            {
                Cursor_Status |= Cursor_Status.Enabled;
                Cursor_Status &= ~Cursor_Status.Blinking;
                return base.Inputs();
            }
        }

        public override bool Inputs_CANCEL()
        {
            Hide();
            base.Inputs_CANCEL();
            return true;
        }

        public override bool Inputs_OKAY()
        {
            Kernel_bin.Blue_magic_Quistis_limit_break c = Contents[CURSOR_SELECT];
            //c.Target;
            Target_Group.SelectTargetWindows(c);
            Target_Group.ShowTargetWindows();
            return base.Inputs_OKAY();
        }

        public override void Refresh()
        {
            if (Memory.State == null || Memory.State.LimitBreakQuistis_Unlocked_BlueMagic == null) return;
            Kernel_bin.Blue_Magic bm = 0;
            unlocked = new List<Kernel_bin.Blue_Magic>();
            foreach (bool b in Memory.State.LimitBreakQuistis_Unlocked_BlueMagic)
            {
                if (b)
                    unlocked.Add(bm);
                bm++;
            }

            int skip = Rows * Page;
            int i;
            for (i = skip; i < unlocked.Count && i < Rows + skip; i++)
            {
                int j = i % Rows;
                ITEM[j, 0].Show();
                BLANKS[j] = false;
                Contents[j] = Kernel_bin.BluemagicQuistislimitbreak[unlocked[i]];
                ((IGMDataItem.Text)ITEM[j, 0]).Data = Contents[j].Name;
            }
            for (; i < Rows + skip; i++)
            {
                int j = i % Rows;
                ITEM[j, 0].Hide();
                BLANKS[j] = true;
            }
            if (unlocked.Count / Rows <= 1)
                ((IGMDataItem.Box)CONTAINER).Title = Icons.ID.SPECIAL;
            else
                ((IGMDataItem.Box)CONTAINER).Title = (Icons.ID)((int)(Icons.ID.SPECIAL_PG1) + Page);
            base.Refresh();
        }

        public override void Reset()
        {
            Hide();
            base.Reset();
        }

        protected override void DrawITEM(int i, int d)
        {
            if (Rows >= i || !Target_Group.Enabled)
                base.DrawITEM(i, d);
        }

        protected override void Init()
        {
            base.Init();
            for (int i = 0; i < Rows; i++)
            {
                ITEM[i, 0] = new IGMDataItem.Text { Pos = SIZE[i] };
            }
            ITEM[Rows, 0] = IGMData.Target.Group.Create(Damageable, false);
            PointerZIndex = 0;
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
            while (BLANKS[0] && Page > 0)
            {
                skipsnd = true;
                base.PAGE_NEXT();
            }
            Refresh();
        }

        protected override void PAGE_PREV()
        {
            base.PAGE_PREV();
            while (BLANKS[0] && Page > 0)
            {
                skipsnd = true;
                base.PAGE_PREV();
            }
            Refresh();
        }

        #endregion Methods
    }
}
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;

namespace OpenVIII.IGMData.Pool
{
    public class Enemy_Attacks : Base<Enemy, Kernel.Enemy_Attacks_Data>
    {
        #region Properties

        protected Target.Group Target_Group { get => ((Target.Group)ITEM[Count - 3, 0]); private set => ITEM[Count - 3, 0] = value; }

        #endregion Properties

        #region Methods

        public static Enemy_Attacks Create(Rectangle pos, Damageable damageable = null, bool battle = false, int count = 4)
        {
            Enemy_Attacks r = Create<Enemy_Attacks>(count + 1, 1, new IGMDataItem.Box { Pos = pos, Title = Icons.ID.ABILITY }, count, 1, damageable, battle: battle);
            if (battle)
                r.Target_Group = Target.Group.Create(r.Damageable);
            return r;
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
            base.Inputs_CANCEL();
            Hide();
            return true;
        }

        public override bool Inputs_OKAY()
        {
            base.Inputs_OKAY();
            Kernel.Enemy_Attacks_Data enemy_Attacks_Data = Contents[CURSOR_SELECT];
            if (enemy_Attacks_Data != null)
            {
                Target_Group?.SelectTargetWindows(enemy_Attacks_Data);
                Target_Group?.ShowTargetWindows();
            }
            return true;
        }

        public override void Refresh()
        {
            if (Damageable != null && Damageable.GetEnemy(out Enemy e))
            {
                IEnumerable<Kernel.Enemy_Attacks_Data> enemy_attacks_datas = e.Enemy_Attacks_Datas;
                DefaultPages = enemy_attacks_datas.Count() / Rows;
                int i = 0;
                int skip = Page * Rows;
                foreach (Kernel.Enemy_Attacks_Data enemy_attacks_data in enemy_attacks_datas)
                {
                    if (i >= Rows) break;
                    if (skip-- > 0) continue;
                    ((IGMDataItem.Text)ITEM[i, 0]).Data = enemy_attacks_data.Name;
                    ITEM[i, 0].Show();
                    BLANKS[i] = false;
                    Contents[i] = enemy_attacks_data;
                    i++;
                }

                for (; i < Rows; i++)
                {
                    ITEM[i, 0].Hide();
                    BLANKS[i] = true;
                    Contents[i] = null;
                }
            }
            base.Refresh();
        }

        public override void Reset()
        {
            Hide();
            base.Reset();
        }

        protected override void Init()
        {
            base.Init();
            for (int i = 0; i < Rows; i++)
            {
                ITEM[i, 0] = new IGMDataItem.Text
                {
                    Pos = SIZE[i]
                };
                ITEM[i, 0].Hide();
            }
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
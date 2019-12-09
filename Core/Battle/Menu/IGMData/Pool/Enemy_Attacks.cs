using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;

namespace OpenVIII.IGMData.Pool
{
    public class Enemy_Attacks : Base<Enemy, Kernel_bin.Enemy_Attacks_Data>
    {
        public static Enemy_Attacks Create(Rectangle pos, Damageable damageable = null, bool battle = false, int count = 4)
        {
            Enemy_Attacks r = Create<Enemy_Attacks>(count + 1, 3, new IGMDataItem.Box { Pos = pos, Title = Icons.ID.ABILITY }, count, 1, damageable, battle: battle);
            if (battle)
                r.Target_Group = Target.Group.Create(r.Damageable);
            return r;
        }

        protected Target.Group Target_Group { get => ((Target.Group)ITEM[Count - 3, 0]); private set => ITEM[Count - 3, 0] = value; }

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

        public override void Refresh()
        {
            if (Damageable != null && Damageable.GetEnemy(out Enemy e))
            {
                IEnumerable<Debug_battleDat.Abilities> abilities = e.Abilities.Where(x => x.MONSTER != null);
                DefaultPages = abilities.Count() / Rows;
                int i = 0;
                int skip = Page * Rows;
                foreach (Debug_battleDat.Abilities a in abilities)
                {
                    if (i >= Rows) break;
                    if (skip-- > 0) continue;
                    ((IGMDataItem.Text)ITEM[i, 0]).Data = a.MONSTER.Name;
                    ITEM[i, 0].Show();
                    BLANKS[i] = false;
                    Contents[i] = a.MONSTER;
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
        public override bool Inputs_CANCEL()
        {
            base.Inputs_CANCEL();
            Hide();
            return true;
        }
        public override bool Inputs_OKAY()
        {
            base.Inputs_OKAY();
            Kernel_bin.Enemy_Attacks_Data enemy_Attacks_Data = Contents[CURSOR_SELECT];
            if (enemy_Attacks_Data != null)
            {
                Target_Group?.SelectTargetWindows(enemy_Attacks_Data);
                Target_Group?.ShowTargetWindows();
            }
            return true;
        }
        public override void Reset()
        {
            Hide();
            base.Reset();
        }

        protected override void InitShift(int i, int col, int row)
        {
            base.InitShift(i, col, row);
            SIZE[i].Inflate(-22, -8);
            SIZE[i].Offset(0, 12 + (-8 * row));
            SIZE[i].Height = (int)(12 * TextScale.Y);
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
    }
}
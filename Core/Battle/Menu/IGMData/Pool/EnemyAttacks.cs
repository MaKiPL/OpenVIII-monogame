using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;
using OpenVIII.Kernel;

namespace OpenVIII.IGMData.Pool
{
    public sealed class EnemyAttacks : Base<Enemy, EnemyAttacksData>
    {
        #region Properties

        private Target.Group TargetGroup { get => ((Target.Group)ITEM[Count - 3, 0]);
            set => ITEM[Count - 3, 0] = value; }

        #endregion Properties

        #region Methods

        public static EnemyAttacks Create(Rectangle pos, Damageable damageable = null, bool battle = false, int count = 4)
        {
            EnemyAttacks r = Create<EnemyAttacks>(count + 1, 1, new IGMDataItem.Box { Pos = pos, Title = Icons.ID.ABILITY }, count, 1, damageable, battle: battle);
            if (battle)
                r.TargetGroup = Target.Group.Create(r.Damageable);
            return r;
        }

        public override bool Inputs()
        {
            bool ret = false;
            if (InputITEM(TargetGroup, ref ret))
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

            EnemyAttacksData enemyAttacksData = Contents[CURSOR_SELECT];
            if (enemyAttacksData == null) return true;
            TargetGroup?.SelectTargetWindows(enemyAttacksData);
            TargetGroup?.ShowTargetWindows();
            return true;
        }

        public override void Refresh()
        {
            if (Damageable != null && Damageable.GetEnemy(out Enemy e))
            {
                IEnumerable<EnemyAttacksData> enemyAttacksDatas = e.EnemyAttacksDatas as EnemyAttacksData[] ?? e.EnemyAttacksDatas.ToArray();
                DefaultPages = enemyAttacksDatas.Count() / Rows;
                int i = 0;
                int skip = Page * Rows;
                foreach (EnemyAttacksData enemyAttacksData in enemyAttacksDatas)
                {
                    if (i >= Rows) break;
                    if (skip-- > 0) continue;
                    ((IGMDataItem.Text)ITEM[i, 0]).Data = enemyAttacksData.Name;
                    ITEM[i, 0].Show();
                    BLANKS[i] = false;
                    Contents[i] = enemyAttacksData;
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
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;

namespace OpenVIII.IGMData.Pool
{
    public class Combine : Base<Damageable, Combine.KernelItem>
    {
        #region Properties

        protected Target.Group Target_Group { get => ((Target.Group)ITEM[Count - 3, 0]); private set => ITEM[Count - 3, 0] = value; }

        #endregion Properties

        #region Methods

        public static Combine Create(Rectangle pos, Damageable damageable = null, bool battle = false, int count = 2)
        {
            Combine r = Create<Combine>(count + 1, 1, new IGMDataItem.Box { Pos = pos, Title = Icons.ID.ABILITY }, count, 1, damageable, battle: battle);
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
            //Kernel_bin.Enemy_Attacks_Data enemy_Attacks_Data = Contents[CURSOR_SELECT];
            //if (enemy_Attacks_Data != null)
            //{
            //    Target_Group?.SelectTargetWindows(enemy_Attacks_Data);
            //    Target_Group?.ShowTargetWindows();
            //}
            return true;
        }

        public override void Refresh()
        {
            int i = 0;
            int skip = Page * Rows;
            bool AddAngelo(FF8String str, KernelItem item)
            {
                if (i >= Rows) return false;
                if (skip-- <= 0)
                {
                    ((IGMDataItem.Text)ITEM[i, 0]).Data = str;
                    Contents[i] = item;
                    BLANKS[i] = false;
                    ITEM[i, 0].Show();
                    i++;
                }
                return true;
            }
            foreach (Kernel_bin.Rinoa_limit_breaks_part_1 lb in Kernel_bin.Rinoalimitbreakspart1)
                if (!AddAngelo(lb.Name, new KernelItem
                {
                    rinoa_Limit_Breaks_Part_1 = lb,
                    rinoa_Limit_Breaks_Part_2 = lb.Angelo == Angelo.Angel_Wing ?
                    Kernel_bin.Rinoalimitbreakspart2.First(x => x.Angelo == lb.Angelo) : null
                })) break;
            foreach (Kernel_bin.Rinoa_limit_breaks_part_2 lb in Kernel_bin.Rinoalimitbreakspart2)
                if (lb.Angelo != Angelo.Angel_Wing && !AddAngelo(lb.Name,
                    new KernelItem { rinoa_Limit_Breaks_Part_2 = lb })) break;
            IEnumerable<Kernel_bin.Non_Junctionable_GFs_Attacks_Data> non_Junctionable_GFs_Attacks_Datas =
                Kernel_bin.NonJunctionableGFsAttacksData.Where(x => !x.Angelo.Equals(Angelo.None));
            foreach (Kernel_bin.Non_Junctionable_GFs_Attacks_Data lb in non_Junctionable_GFs_Attacks_Datas)
                if (!AddAngelo(lb.Name,
                    new KernelItem { non_Junctionable_GFs_Attacks_Data = lb })) break;
            DefaultPages = Kernel_bin.Rinoalimitbreakspart1.Count +
                Kernel_bin.Rinoalimitbreakspart2.Count +
                non_Junctionable_GFs_Attacks_Datas.Count() - 1;
            for (; i < Rows; i++)
            {
                BLANKS[i] = true;
                ITEM[i, 0].Hide();
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

        #endregion Methods

        #region Structs

        public struct KernelItem
        {
            #region Fields

            public Kernel_bin.Non_Junctionable_GFs_Attacks_Data non_Junctionable_GFs_Attacks_Data;
            public Kernel_bin.Rinoa_limit_breaks_part_1 rinoa_Limit_Breaks_Part_1;
            public Kernel_bin.Rinoa_limit_breaks_part_2 rinoa_Limit_Breaks_Part_2;

            #endregion Fields
        }

        #endregion Structs
    }
}
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
            Combine r = Create<Combine>(count + 1, 1, new IGMDataItem.Box { Pos = pos, Title = Icons.ID.SPECIAL }, count, 1, damageable, battle: battle);
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
            KernelItem item = Contents[CURSOR_SELECT];
            if (!BLANKS[CURSOR_SELECT])
            {
                Target_Group?.SelectTargetWindows(item);
                Target_Group?.ShowTargetWindows();
            }
            return true;
        }

        public override void Refresh()
        {
            int i = 0;
            int skip = Page * Rows;
            bool AddAngelo(KernelItem item)
            {
                if (i >= Rows) return false;
                if (skip-- <= 0)
                {
                    ((IGMDataItem.Text)ITEM[i, 0]).Data = item.Name;
                    Contents[i] = item;
                    BLANKS[i] = false;
                    ITEM[i, 0].Show();
                    i++;
                }
                return true;
            }
            if (Kernel_bin.Rinoalimitbreakspart1 != null)
                foreach (Kernel_bin.Rinoa_limit_breaks_part_1 lb in Kernel_bin.Rinoalimitbreakspart1)
                    if (!AddAngelo(new KernelItem
                    {
                        rinoa_Limit_Breaks_Part_1 = lb,
                        rinoa_Limit_Breaks_Part_2 = lb.Angelo == Angelo.Angel_Wing ?
                        Kernel_bin.Rinoalimitbreakspart2.First(x => x.Angelo == lb.Angelo) : null
                    })) break;
            if (Kernel_bin.Rinoalimitbreakspart2 != null)
                foreach (Kernel_bin.Rinoa_limit_breaks_part_2 lb in Kernel_bin.Rinoalimitbreakspart2)
                    if (lb.Angelo != Angelo.Angel_Wing && !AddAngelo(
                        new KernelItem { rinoa_Limit_Breaks_Part_2 = lb })) break;
            IEnumerable<Kernel_bin.Non_Junctionable_GFs_Attacks_Data> non_Junctionable_GFs_Attacks_Datas =
                Kernel_bin.NonJunctionableGFsAttacksData?.Where(x => !x.Angelo.Equals(Angelo.None));
            if (non_Junctionable_GFs_Attacks_Datas != null)
                foreach (Kernel_bin.Non_Junctionable_GFs_Attacks_Data lb in non_Junctionable_GFs_Attacks_Datas)
                    if (!AddAngelo(
                        new KernelItem { non_Junctionable_GFs_Attacks_Data = lb })) break;
            DefaultPages = ((Kernel_bin.Rinoalimitbreakspart1?.Count ?? 0) +
                (Kernel_bin.Rinoalimitbreakspart2?.Count ?? 0) +
                (non_Junctionable_GFs_Attacks_Datas?.Count() ?? 1) - 1) / Rows;
            for (; i < Rows; i++)
            {
                BLANKS[i] = true;
                ITEM[i, 0].Hide();
            }
            UpdateTitle();
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

        public override void UpdateTitle()
        {
            base.UpdateTitle();
            if (Pages == 1)
            {
                ((IGMDataItem.Box)CONTAINER).Title = Icons.ID.SPECIAL;
            }
            else
                switch (Page)
                {
                    case 0:
                        ((IGMDataItem.Box)CONTAINER).Title = Icons.ID.SPECIAL_PG1;
                        break;

                    case 1:
                        ((IGMDataItem.Box)CONTAINER).Title = Icons.ID.SPECIAL_PG2;
                        break;

                    case 2:
                        ((IGMDataItem.Box)CONTAINER).Title = Icons.ID.SPECIAL_PG3;
                        break;

                    case 3:
                        ((IGMDataItem.Box)CONTAINER).Title = Icons.ID.SPECIAL_PG4;
                        break;

                    default:
                        ((IGMDataItem.Box)CONTAINER).Title = Icons.ID.SPECIAL;
                        break;
                }
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

            #region Properties

            public int ID
            {
                get
                {
                    if (rinoa_Limit_Breaks_Part_1 != null)
                        return rinoa_Limit_Breaks_Part_1.ID;
                    if (rinoa_Limit_Breaks_Part_2 != null)
                        return rinoa_Limit_Breaks_Part_2.ID;
                    if (non_Junctionable_GFs_Attacks_Data != null)
                        return non_Junctionable_GFs_Attacks_Data.ID;
                    return 0;
                }
            }

            public FF8String Name
            {
                get
                {
                    FF8String replace(FF8String str)
                    {
                        return str.Clone().Replace(new FF8String(new byte[] { 3, 64 }), Memory.State.Angelosname);
                    }
                    if (rinoa_Limit_Breaks_Part_1 != null)
                        return replace(rinoa_Limit_Breaks_Part_1.Name);
                    if (rinoa_Limit_Breaks_Part_2 != null)
                        return replace(rinoa_Limit_Breaks_Part_2.Name);
                    if (non_Junctionable_GFs_Attacks_Data != null)
                        return replace(non_Junctionable_GFs_Attacks_Data.Name);
                    return null;
                }
            }

            public Kernel_bin.Target Target
            {
                get
                {
                    Kernel_bin.Target r = 0;
                    if (rinoa_Limit_Breaks_Part_1 != null)
                        r |= rinoa_Limit_Breaks_Part_1.Target;
                    if (rinoa_Limit_Breaks_Part_2 != null)
                        r |= rinoa_Limit_Breaks_Part_2.Target;
                    // there is no target from njGFs so must be set in battle scripts.
                    // setting a base default for those.
                    if (non_Junctionable_GFs_Attacks_Data != null)
                        r |= Kernel_bin.Target.Enemy |
                            Kernel_bin.Target.Ally |
                            Kernel_bin.Target.Single_Target;
                    //non_Junctionable_GFs_Attacks_Data.Target;
                    return r;
                }
            }

            #endregion Properties
        }

        #endregion Structs
    }
}
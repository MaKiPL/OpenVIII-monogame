using System.Linq;
using Microsoft.Xna.Framework;
using OpenVIII.IGMDataItem;
using OpenVIII.Kernel;

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
            var r = Create<Combine>(count + 1, 1, new Box { Pos = pos, Title = Icons.ID.SPECIAL }, count, 1, damageable, battle: battle);
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
                Target_Group?.SelectTargetWindows(item);
                Target_Group?.ShowTargetWindows();
            }
            return true;
        }

        public override void Refresh()
        {
            var i = 0;
            var skip = Page * Rows;
            bool AddAngelo(KernelItem item)
            {
                if (i >= Rows) return false;
                if (skip-- <= 0)
                {
                    ((Text)ITEM[i, 0]).Data = item.Name;
                    Contents[i] = item;
                    BLANKS[i] = false;
                    ITEM[i, 0].Show();
                    i++;
                }
                return true;
            }
            if (Memory.KernelBin.RinoaLimitBreaksPart1 != null)
                foreach (var lb in Memory.KernelBin.RinoaLimitBreaksPart1)
                    if (!AddAngelo(new KernelItem
                    {
                        rinoa_Limit_Breaks_Part_1 = lb,
                        rinoa_Limit_Breaks_Part_2 = lb.Angelo == Angelo.Angel_Wing ?
                        Memory.KernelBin.RinoaLimitBreaksPart2.First(x => x.Angelo == lb.Angelo) : null
                    })) break;
            if (Memory.KernelBin.RinoaLimitBreaksPart2 != null)
                foreach (var lb in Memory.KernelBin.RinoaLimitBreaksPart2)
                    if (lb.Angelo != Angelo.Angel_Wing && !AddAngelo(
                        new KernelItem { rinoa_Limit_Breaks_Part_2 = lb })) break;
            var non_Junctionable_GFs_Attacks_Datas =
                Memory.KernelBin.NonJunctionableGFsAttacksData?.Where(x => !x.Angelo.Equals(Angelo.None));
            if (non_Junctionable_GFs_Attacks_Datas != null)
                foreach (var lb in non_Junctionable_GFs_Attacks_Datas)
                    if (!AddAngelo(
                        new KernelItem { non_Junctionable_GFs_Attacks_Data = lb })) break;
            DefaultPages = ((Memory.KernelBin.RinoaLimitBreaksPart1?.Count ?? 0) +
                (Memory.KernelBin.RinoaLimitBreaksPart2?.Count ?? 0) +
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
            for (var i = 0; i < Rows; i++)
            {
                ITEM[i, 0] = new Text
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

        #endregion Methods

        #region Structs

        public struct KernelItem
        {
            #region Fields

            public NonJunctionableGFsAttacksData non_Junctionable_GFs_Attacks_Data;
            public RinoaLimitBreaksPart1 rinoa_Limit_Breaks_Part_1;
            public RinoaLimitBreaksPart2 rinoa_Limit_Breaks_Part_2;

            #endregion Fields

            #region Properties

            public int ID
            {
                get
                {
                    if (rinoa_Limit_Breaks_Part_1 != null)
                        return rinoa_Limit_Breaks_Part_1.RinoaLimitID;
                    if (rinoa_Limit_Breaks_Part_2 != null)
                        return rinoa_Limit_Breaks_Part_2.RinoaLimit2ID;
                    if (non_Junctionable_GFs_Attacks_Data != null)
                        return non_Junctionable_GFs_Attacks_Data.NonGFID;
                    return 0;
                }
            }

            public FF8String Name
            {
                get
                {
                    FF8String replace(FF8String str)
                    {
                        return str.Clone().Replace(new FF8String(new byte[] { 3, 64 }), Memory.State.AngeloName);
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

            public Kernel.Target Target
            {
                get
                {
                    Kernel.Target r = 0;
                    if (rinoa_Limit_Breaks_Part_1 != null)
                        r |= rinoa_Limit_Breaks_Part_1.Target;
                    if (rinoa_Limit_Breaks_Part_2 != null)
                        r |= rinoa_Limit_Breaks_Part_2.Target;
                    // there is no target from njGFs so must be set in battle scripts.
                    // setting a base default for those.
                    if (non_Junctionable_GFs_Attacks_Data != null)
                        r |= Kernel.Target.Enemy |
                            Kernel.Target.Ally |
                            Kernel.Target.SingleTarget;
                    //non_Junctionable_GFs_Attacks_Data.Target;
                    return r;
                }
            }

            #endregion Properties
        }

        #endregion Structs
    }
}
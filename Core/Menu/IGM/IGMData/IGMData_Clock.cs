using Microsoft.Xna.Framework;

namespace OpenVIII
{
    public partial class IGM
    {
        #region Classes

        private class IGMData_Clock : IGMData
        {
            #region Constructors

            public IGMData_Clock() : base(1, 8, new IGMDataItem_Box(pos: new Rectangle { Width = 226, Height = 114, Y = 630 - 114, X = 843 - 226 }))
            {
            }

            #endregion Constructors

            #region Methods

            public override void Refresh()
            {
                base.Refresh();
                Rectangle r;

                r = CONTAINER;
                r.Offset(105, 14);
                ITEM[0, 1] = new IGMDataItem_Int(Memory.State.Timeplayed.TotalHours < 99 ? (int)(Memory.State.Timeplayed.TotalHours) : 99, r, 2, 0, 1, 2);

                r = CONTAINER;
                r.Offset(165, 14);
                ITEM[0, 3] = new IGMDataItem_Int(Memory.State.Timeplayed.TotalHours < 99 ? Memory.State.Timeplayed.Minutes : 99, r, 2, 0, 2, 2);
                if (!Memory.State.TeamLaguna)
                {
                    r = CONTAINER;
                    r.Offset(25, 48);
                    ITEM[0, 4] = new IGMDataItem_Icon(Icons.ID.SeeD, r, 13);

                    r = CONTAINER;
                    r.Offset(105, 48);
                    ITEM[0, 5] = Memory.State.Fieldvars != null
                        ? new IGMDataItem_Int(Memory.State.Fieldvars.SeedRankPts / 100 < 99999 ? Memory.State.Fieldvars.SeedRankPts / 100 : 99999, r, 2, 0, 1, 5)
                        : null;
                }
                else
                {
                    ITEM[0, 4] = null;
                    ITEM[0, 5] = null;
                }

                r = CONTAINER;
                r.Offset(25, 81);
                ITEM[0, 6] = new IGMDataItem_Int(Memory.State.AmountofGil < 99999999 ? (int)(Memory.State.AmountofGil) : 99999999, r, 2, 0, 1, 8);
            }

            public override bool Update()
            {
                bool ret = base.Update();

                return ret;
            }

            protected override void Init()
            {
                Rectangle r;
                r = CONTAINER;
                r.Offset(25, 14);
                ITEM[0, 0] = new IGMDataItem_Icon(Icons.ID.PLAY, r, 13);

                r = CONTAINER;
                r.Offset(145, 14);
                ITEM[0, 2] = new IGMDataItem_Icon(Icons.ID.Colon, r, 13, 2, .5f) { Blink = true };

                r = CONTAINER;
                r.Offset(185, 81);
                ITEM[0, 7] = new IGMDataItem_Icon(Icons.ID.G, r, 2);
                base.Init();
            }

            #endregion Methods
        }

        #endregion Classes
    }
}
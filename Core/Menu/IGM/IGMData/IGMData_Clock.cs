using Microsoft.Xna.Framework;

namespace OpenVIII
{
    public partial class IGM
    {
        #region Classes

        private class IGMData_Clock : IGMData.Base
        {
            private const int MaxSeedRank = 99999;
            private const int MaxGil = 99999999;
            private const int MaxHourOrMins = 99;

            #region Constructors

            public static IGMData_Clock Create() => Create<IGMData_Clock>(1, 8, new IGMDataItem.Box { Pos = new Rectangle { Width = 226, Height = 114, Y = 630 - 114, X = 843 - 226 } });

            #endregion Constructors

            #region Methods

            public override void Refresh()
            {
                base.Refresh();

                ((IGMDataItem.Integer)ITEM[0, 1]).Data = Memory.State.Timeplayed.TotalHours < MaxHourOrMins ? checked((int)(Memory.State.Timeplayed.TotalHours)) : MaxHourOrMins;
                ((IGMDataItem.Integer)ITEM[0, 3]).Data = Memory.State.Timeplayed.TotalHours < MaxHourOrMins ? Memory.State.Timeplayed.Minutes : MaxHourOrMins;
                if (!Memory.State.TeamLaguna)
                {
                    //TODO Hide seed rank if not in seed yet.
                    ((IGMDataItem.Integer)ITEM[0, 5]).Data = Memory.State.Fieldvars.SeedRankPts / 100 < MaxSeedRank ? Memory.State.Fieldvars.SeedRankPts / 100 : MaxSeedRank;
                    ITEM[0, 4].Show();
                    ITEM[0, 5].Show();
                }
                else
                {
                    ITEM[0, 4].Hide();
                    ITEM[0, 5].Hide();
                }
                ((IGMDataItem.Integer)ITEM[0, 6]).Data = Memory.State.AmountofGil < MaxGil ? (int)(Memory.State.AmountofGil) : MaxGil;
            }

            protected override void Init()
            {
                Rectangle r;
                r = CONTAINER;
                r.Offset(25, 14);
                ITEM[0, 0] = new IGMDataItem.Icon { Data = Icons.ID.PLAY, Pos = r, Palette = 13 };

                r = CONTAINER;
                r.Offset(145, 14);
                ITEM[0, 2] = new IGMDataItem.Icon { Data = Icons.ID.Colon, Pos = r, Palette = 13, Faded_Palette = 2, Blink_Adjustment = .5f, Blink = true };

                r = CONTAINER;
                r.Offset(185, 81);
                ITEM[0, 7] = new IGMDataItem.Icon { Data = Icons.ID.G, Pos = r, Palette = 2 };
                base.Init();
                r = CONTAINER;
                r.Offset(105, 14);
                ITEM[0, 1] = new IGMDataItem.Integer { Pos = r, Palette = 2, Faded_Palette = 0, Padding = 1, Spaces = 2 };

                r = CONTAINER;
                r.Offset(165, 14);
                ITEM[0, 3] = new IGMDataItem.Integer { Pos = r, Palette = 2, Faded_Palette = 0, Padding = 2, Spaces = 2 };

                r = CONTAINER;
                r.Offset(25, 48);
                ITEM[0, 4] = new IGMDataItem.Icon { Data = Icons.ID.SeeD, Pos = r, Palette = 13 };

                r = CONTAINER;
                r.Offset(105, 48);
                ITEM[0, 5] = new IGMDataItem.Integer { Pos = r, Palette = 2, Faded_Palette = 0, Padding = 1, Spaces = 5 };

                r = CONTAINER;
                r.Offset(25, 81);
                ITEM[0, 6] = new IGMDataItem.Integer { Pos = r, Palette = 2, Faded_Palette = 0, Padding = 1, Spaces = 8 };
            }

            #endregion Methods
        }

        #endregion Classes
    }
}
using Microsoft.Xna.Framework;
using System.Diagnostics.CodeAnalysis;

namespace OpenVIII
{
    public partial class IGM
    {
        #region Classes

        private class Clock : IGMData.Base
        {
            #region Fields

            private const int MaxGil = 99999999;
            private const int MaxHourOrMinutes = 99;
            private const int MaxSeedRank = 99999;

            #endregion Fields

            #region Methods

            [SuppressMessage("ReSharper", "MemberHidesStaticFromOuterClass")]
            public static Clock Create() => Create<Clock>(1, 8, new IGMDataItem.Box { Pos = new Rectangle { Width = 226, Height = 114, Y = 630 - 114, X = 843 - 226 } });

            public override void Refresh()
            {
                if (Memory.State?.FieldVars != null)
                {
                    base.Refresh();

                    ((IGMDataItem.Integer)ITEM[0, 1]).Data = Memory.State.TimePlayed.TotalHours < MaxHourOrMinutes ? checked((int)(Memory.State.TimePlayed.TotalHours)) : MaxHourOrMinutes;
                    ((IGMDataItem.Integer)ITEM[0, 3]).Data = Memory.State.TimePlayed.TotalHours < MaxHourOrMinutes ? Memory.State.TimePlayed.Minutes : MaxHourOrMinutes;
                    if (!Memory.State.TeamLaguna)
                    {
                        //TODO Hide seed rank if not in seed yet.
                        var seedRank = Memory.State.FieldVars.SeedRankPts / 100;
                        ((IGMDataItem.Integer)ITEM[0, 5]).Data = seedRank < MaxSeedRank ? seedRank : MaxSeedRank;
                        ITEM[0, 4].Show();
                        ITEM[0, 5].Show();
                    }
                    else
                    {
                        ITEM[0, 4].Hide();
                        ITEM[0, 5].Hide();
                    }
                    for (var i = 0; i < Depth; i++)
                    {
                        if (i != 4 && i != 5)
                            ITEM[0, i]?.Show();
                    }

                    var stateAmountOfGil = Memory.State.AmountOfGil;
                    ((IGMDataItem.Integer)ITEM[0, 6]).Data = stateAmountOfGil < MaxGil ? (int)stateAmountOfGil : MaxGil;
                }
            }

            protected override void Init()
            {
                base.Init();
                var r = CONTAINER.Pos;
                r.Offset(25, 14);
                ITEM[0, 0] = new IGMDataItem.Icon { Data = Icons.ID.PLAY, Pos = r, Palette = 13 };

                r = CONTAINER;
                r.Offset(145, 14);
                ITEM[0, 2] = new IGMDataItem.Icon { Data = Icons.ID.Colon, Pos = r, Palette = 13, Faded_Palette = 2, Blink_Adjustment = .5f, Blink = true };

                r = CONTAINER;
                r.Offset(185, 81);
                ITEM[0, 7] = new IGMDataItem.Icon { Data = Icons.ID.G, Pos = r, Palette = 2 };

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
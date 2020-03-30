using Microsoft.Xna.Framework;

namespace OpenVIII
{
    public partial class Junction
    {
        #region Classes

        private class IGMData_Mag_PageTitle : IGMData.Base
        {
            #region Fields

            private Mode last = 0;

            #endregion Fields

            #region Methods

            public static IGMData_Mag_PageTitle Create() => Create<IGMData_Mag_PageTitle>(1, 4, new IGMDataItem.Box { Pos = new Rectangle(0, 345, 435, 66) });

            public override void Refresh()
            {
                base.Refresh();

                if (UpdateChild(Mode.Mag_Stat, Icons.ID.Rewind_Fast, Strings.Name.ST_A_D, Icons.ID.Rewind, Strings.Name.EL_A_D))
                { }
                else if (UpdateChild(Mode.Mag_EL_A, Icons.ID.Rewind, Strings.Name.ST_A_D, Icons.ID.Forward, Strings.Name.Stats))
                { }
                else if (UpdateChild(Mode.Mag_ST_A, Icons.ID.Forward, Strings.Name.EL_A_D, Icons.ID.Forward_Fast, Strings.Name.Stats))
                { }
                bool UpdateChild(Mode mode, Icons.ID icon1, FF8StringReference str1, Icons.ID icon2, FF8StringReference str2)
                {
                    if (Junction != null && Junction.GetMode().Equals(mode) && Enabled)
                    {
                        ((IGMDataItem.Icon)ITEM[0, 0]).Data = icon1;
                        ((IGMDataItem.Text)ITEM[0, 1]).Data = str1;
                        ((IGMDataItem.Icon)ITEM[0, 2]).Data = icon2;
                        ((IGMDataItem.Text)ITEM[0, 3]).Data = str2;
                        return true;
                    }
                    return false;
                }
            }

            public override bool Update()
            {
                if (Junction != null && !Junction.GetMode().Equals(last) && Enabled)
                {
                    last = (Mode)Junction.GetMode();
                    Refresh();
                    return base.Update();
                }
                return false;
            }

            protected override void Init()
            {
                base.Init();
                ITEM[0, 0] = new IGMDataItem.Icon { Data = Icons.ID.Rewind_Fast, Pos = new Rectangle(SIZE[0].X, SIZE[0].Y, 0, 0), Palette = 2, Faded_Palette = 7 };
                ITEM[0, 1] = new IGMDataItem.Text { Pos = new Rectangle(SIZE[0].X + 20, SIZE[0].Y, 0, 0) };
                ITEM[0, 2] = new IGMDataItem.Icon { Data = Icons.ID.Rewind, Pos = new Rectangle(SIZE[0].X + 143, SIZE[0].Y, 0, 0), Palette = 2, Faded_Palette = 7 };
                ITEM[0, 3] = new IGMDataItem.Text { Pos = new Rectangle(SIZE[0].X + 169, SIZE[0].Y, 0, 0) };
            }

            protected override void InitShift(int i, int col, int row)
            {
                base.InitShift(i, col, row);
                SIZE[0].Inflate(-19, -18);
            }

            #endregion Methods
        }

        #endregion Classes
    }
}
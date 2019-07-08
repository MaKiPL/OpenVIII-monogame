using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace OpenVIII
{
        public partial class IGM_Junction
        {
            private class IGMData_Mag_PageTitle : IGMData
            {
                public new Dictionary<Items, FF8String> Descriptions { get; private set; }

                public IGMData_Mag_PageTitle() : base(1, 4, new IGMDataItem_Box(pos: new Rectangle(0, 345, 435, 66)))
                {
                }

                protected override void Init()
                {
                    Descriptions = new Dictionary<Items, FF8String> {
                    {Items.ST_A_D, Memory.Strings.Read(Strings.FileID.MNGRP,2,251) },
                    {Items.EL_A_D, Memory.Strings.Read(Strings.FileID.MNGRP,2,253) },
                    {Items.Stats, Memory.Strings.Read(Strings.FileID.MNGRP,2,255) },
                    };

                    base.Init();
                }

                protected override void InitShift(int i, int col, int row)
                {
                    base.InitShift(i, col, row);
                    SIZE[0].Inflate(-19, -18);
                }

                public override void ReInit()
                {
                    base.ReInit();

                    if (IGM_Junction != null && IGM_Junction.GetMode().Equals(Mode.Mag_Stat) && Enabled)
                    {
                        ITEM[0, 0] = new IGMDataItem_Icon(Icons.ID.Rewind_Fast, new Rectangle(SIZE[0].X, SIZE[0].Y, 0, 0), 2, 7);
                        ITEM[0, 1] = new IGMDataItem_String(Descriptions[Items.ST_A_D], new Rectangle(SIZE[0].X + 20, SIZE[0].Y, 0, 0));
                        ITEM[0, 2] = new IGMDataItem_Icon(Icons.ID.Rewind, new Rectangle(SIZE[0].X + 143, SIZE[0].Y, 0, 0), 2, 7);
                        ITEM[0, 3] = new IGMDataItem_String(Descriptions[Items.EL_A_D], new Rectangle(SIZE[0].X + 169, SIZE[0].Y, 0, 0));
                    }
                    else
                    if (IGM_Junction != null && IGM_Junction.GetMode().Equals(Mode.Mag_EL_A) && Enabled) //coords for these two need checked.
                    {
                        ITEM[0, 0] = new IGMDataItem_Icon(Icons.ID.Rewind, new Rectangle(SIZE[0].X, SIZE[0].Y, 0, 0), 2, 7);
                        ITEM[0, 1] = new IGMDataItem_String(Descriptions[Items.ST_A_D], new Rectangle(SIZE[0].X + 20, SIZE[0].Y, 0, 0));
                        ITEM[0, 2] = new IGMDataItem_Icon(Icons.ID.Forward, new Rectangle(SIZE[0].X + 143, SIZE[0].Y, 0, 0), 2, 7);
                        ITEM[0, 3] = new IGMDataItem_String(Descriptions[Items.Stats], new Rectangle(SIZE[0].X + 169, SIZE[0].Y, 0, 0));
                    }
                    else
                    if (IGM_Junction != null && IGM_Junction.GetMode().Equals(Mode.Mag_ST_A) && Enabled)
                    {
                        ITEM[0, 0] = new IGMDataItem_Icon(Icons.ID.Forward, new Rectangle(SIZE[0].X, SIZE[0].Y, 0, 0), 2, 7);
                        ITEM[0, 1] = new IGMDataItem_String(Descriptions[Items.EL_A_D], new Rectangle(SIZE[0].X + 20, SIZE[0].Y, 0, 0));
                        ITEM[0, 2] = new IGMDataItem_Icon(Icons.ID.Forward_Fast, new Rectangle(SIZE[0].X + 143, SIZE[0].Y, 0, 0), 2, 7);
                        ITEM[0, 3] = new IGMDataItem_String(Descriptions[Items.Stats], new Rectangle(SIZE[0].X + 169, SIZE[0].Y, 0, 0));
                    }
                }

                public override bool Update()
                {
                    ReInit();
                    return base.Update();
                }
            }
        }
    
}
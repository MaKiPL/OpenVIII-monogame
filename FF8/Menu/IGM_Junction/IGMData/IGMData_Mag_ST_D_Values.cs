using Microsoft.Xna.Framework;

namespace FF8
{
    public partial class Module_main_menu_debug
    {
        private partial class IGM_Junction
        {
            private class IGMData_Mag_ST_D_Values : IGMData
            {
                public IGMData_Mag_ST_D_Values() : base( 12, 3, new IGMDataItem_Box(title: Icons.ID.Status_Defense, pos: new Rectangle(280, 363, 545, 267)), 2, 6)
                {
                }

                public override bool Update()
                {
                    for (short pos = 0; pos < Count; pos++)
                    {
                        ITEM[pos, 0] = new IGMDataItem_Icon(Icons.ID.Status_Death + pos, new Rectangle(SIZE[pos].X, SIZE[pos].Y, 0, 0),10);
                    }
                    return base.Update();
                }
            }
        }
    }
}
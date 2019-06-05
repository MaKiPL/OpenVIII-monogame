using Microsoft.Xna.Framework;

namespace FF8
{
    public partial class Module_main_menu_debug
    {
        private partial class IGM_Junction
        {
            private class IGMData_Mag_ST_A_Values : IGMData
            {
                public IGMData_Mag_ST_A_Values() : base( 12, 3, new IGMDataItem_Box(title: Icons.ID.Status_Attack, pos: new Rectangle(280, 363, 545, 267)), 2, 6)
                {
                }
            }
        }
    }
}
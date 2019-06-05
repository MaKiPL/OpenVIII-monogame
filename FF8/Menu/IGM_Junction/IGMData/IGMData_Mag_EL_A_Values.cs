using Microsoft.Xna.Framework;

namespace FF8
{
    public partial class Module_main_menu_debug
    {
        private partial class IGM_Junction
        {
            private class IGMData_Mag_EL_A_Values : IGMData
            {
                public IGMData_Mag_EL_A_Values() : base( 8, 5, new IGMDataItem_Box(title: Icons.ID.Elemental_Attack, pos: new Rectangle(280, 423, 545, 201)), 2, 4)
                {
                }
            }
        }
    }
}
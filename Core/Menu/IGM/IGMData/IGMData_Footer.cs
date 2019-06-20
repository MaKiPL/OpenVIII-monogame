using Microsoft.Xna.Framework;

namespace FF8
{
    public partial class Module_main_menu_debug
    {
        private partial class IGM
        {
            private class IGMData_Footer : IGMData
            {
                public IGMData_Footer() : base(0, 0, new IGMDataItem_Box(pos: new Rectangle { Width = 610, Height = 75, Y = 630 - 75 }))
                {
                }

                public override bool Update()
                {
                    base.Update();
                    ((IGMDataItem_Box)CONTAINER).Data = Memory.Strings.Read(Strings.FileID.AREAMES, 0, Memory.State.LocationID).ReplaceRegion();
                    return true;
                }
            }
        }
    }
}
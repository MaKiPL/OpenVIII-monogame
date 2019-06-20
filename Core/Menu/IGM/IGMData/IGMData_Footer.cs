using Microsoft.Xna.Framework;

namespace OpenVIII
{
    public partial class Module_main_menu_debug
    {
        #region Classes

        private partial class IGM
        {
            #region Classes

            private class IGMData_Footer : IGMData
            {
                #region Constructors

                public IGMData_Footer() : base(0, 0, new IGMDataItem_Box(pos: new Rectangle { Width = 610, Height = 75, Y = 630 - 75 }))
                {
                }

                #endregion Constructors

                #region Methods

                public override void ReInit()
                {
                    base.ReInit();
                    ((IGMDataItem_Box)CONTAINER).Data = Memory.Strings.Read(Strings.FileID.AREAMES, 0, Memory.State.LocationID).ReplaceRegion();
                }

                #endregion Methods
            }

            #endregion Classes
        }

        #endregion Classes
    }
}
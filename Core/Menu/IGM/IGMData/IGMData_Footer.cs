using Microsoft.Xna.Framework;

namespace OpenVIII
{
    public partial class IGM
    {
        #region Classes

        private class IGMData_Footer : IGMData.Base
        {
            #region Methods

            public static IGMData_Footer Create() => Create<IGMData_Footer>(0, 0, new IGMDataItem.Box { Pos = new Rectangle { Width = 610, Height = 75, Y = 630 - 75 } });

            public override void Refresh()
            {
                base.Refresh();
                if (CONTAINER != null && Memory.State != null)
                    ((IGMDataItem.Box)CONTAINER).Data = Memory.Strings.Read(Strings.FileID.AREAMES, 0, Memory.State.LocationID);
            }

            #endregion Methods
        }

        #endregion Classes
    }
}
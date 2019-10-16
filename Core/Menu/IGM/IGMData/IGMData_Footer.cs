using Microsoft.Xna.Framework;

namespace OpenVIII
{
    public partial class IGM
    {
        #region Classes

        private class IGMData_Footer : IGMData.Base
        {
            #region Constructors

            public static IGMData_Footer Create() => Create<IGMData_Footer>(0, 0, new IGMDataItem.Box(pos: new Rectangle { Width = 610, Height = 75, Y = 630 - 75 }));

            #endregion Constructors

            #region Methods

            public override void Refresh()
            {
                base.Refresh();
                if(CONTAINER!=null)
                ((IGMDataItem.Box)CONTAINER).Data = Memory.Strings.Read(Strings.FileID.AREAMES, 0, Memory.State.LocationID);
            }

            #endregion Methods
        }

        #endregion Classes
    }
}
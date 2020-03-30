using Microsoft.Xna.Framework;
using System.Diagnostics.CodeAnalysis;

namespace OpenVIII
{
    public partial class IGM
    {
        #region Classes

        private class Footer : IGMData.Base
        {
            #region Methods

            [SuppressMessage("ReSharper", "MemberHidesStaticFromOuterClass")]
            public static Footer Create() => Create<Footer>(0, 0, new IGMDataItem.Box { Pos = new Rectangle { Width = 610, Height = 75, Y = 630 - 75 } });

            public override void Refresh()
            {
                base.Refresh();
                if (CONTAINER != null && Memory.State != null)
                    ((IGMDataItem.Box)CONTAINER).Data = Memory.Strings.Read(Strings.FileID.AreaNames, 0, Memory.State.LocationID);
            }

            #endregion Methods
        }

        #endregion Classes
    }
}
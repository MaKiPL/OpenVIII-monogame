using Microsoft.Xna.Framework;

namespace OpenVIII
{
    public partial class IGM_Junction
    {
        #region Classes

        private class IGMData_Help : IGMData.Base
        {
            #region Constructors

            public IGMData_Help() : base(0, 0, new IGMDataItem.Box(IGM_Junction.Descriptions[Items.Junction], pos: new Rectangle(15, 69, 810, 78), title: Icons.ID.HELP))
            {
            }

            #endregion Constructors

            #region Properties

            public FF8String Data { get => ((IGMDataItem.Box)CONTAINER).Data; set => ((IGMDataItem.Box)CONTAINER).Data = value; }

            #endregion Properties
        }

        #endregion Classes
    }
}
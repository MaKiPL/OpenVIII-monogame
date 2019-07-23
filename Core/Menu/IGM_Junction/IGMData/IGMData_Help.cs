using Microsoft.Xna.Framework;

namespace OpenVIII
{
    public partial class IGM_Junction
    {
        #region Classes

        private class IGMData_Help : IGMData
        {
            #region Constructors

            public IGMData_Help() : base(0, 0, new IGMDataItem_Box(IGM_Junction.Descriptions[Items.Junction], pos: new Rectangle(15, 69, 810, 78), title: Icons.ID.HELP))
            {
            }

            #endregion Constructors

            #region Properties

            public FF8String Data { get => ((IGMDataItem_Box)CONTAINER).Data; set => ((IGMDataItem_Box)CONTAINER).Data = value; }

            #endregion Properties
        }

        #endregion Classes
    }
}
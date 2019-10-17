using Microsoft.Xna.Framework;
using System;

namespace OpenVIII
{
    public partial class IGM_Junction
    {
        #region Classes

        private class IGMData_TopMenu_Auto_Group : IGMData.Group.Base
        {
            #region Constructors

            public static new IGMData_TopMenu_Auto_Group Create(params Menu_Base[] d) => Create<IGMData_TopMenu_Auto_Group>(d);

            #endregion Constructors

            #region Methods

            public override void Draw()
            {
                if (Enabled)
                {
                    Cursor_Status |= (Cursor_Status.Draw | Cursor_Status.Blinking);
                    base.Draw();
                    Menu.BoxReturn dims = ((IGMDataItem.Box)ITEM[0, 0]).Dims;
                    if (dims.Cursor != Point.Zero)
                        CURSOR[0] = dims.Cursor;
                }
            }

            protected override void Init()
            {
                base.Init();
                Hide();
            }

            #endregion Methods
        }

        #endregion Classes
    }
}
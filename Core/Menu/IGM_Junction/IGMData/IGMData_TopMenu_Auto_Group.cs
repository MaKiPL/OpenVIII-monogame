using Microsoft.Xna.Framework;
using System;

namespace OpenVIII
{
    public partial class IGM_Junction
    {
        #region Classes

        private class IGMData_TopMenu_Auto_Group : IGMData_Group
        {
            #region Constructors

            public IGMData_TopMenu_Auto_Group(params IGMData.Base[] d) : base(d)
            {
            }

            #endregion Constructors

            #region Methods

            public override void Draw()
            {
                if (Enabled)
                {
                    Cursor_Status |= (Cursor_Status.Draw | Cursor_Status.Blinking);
                    base.Draw();
                    Tuple<Rectangle, Point, Rectangle> i = ((IGMDataItem.Box)(((IGMData_Container)ITEM[0, 0]).CONTAINER)).Dims;
                    if (i != null)
                        CURSOR[0] = i.Item2;
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
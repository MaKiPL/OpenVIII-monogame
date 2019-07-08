using Microsoft.Xna.Framework;
using System;

namespace OpenVIII
{
        public partial class IGM_Junction
        {
            private class IGMData_TopMenu_Off_Group : IGMData_Group
            {
                public IGMData_TopMenu_Off_Group(params IGMData[] d) : base( d)
                {
                }

                public override void Draw()
                {
                    if (Enabled)
                    {
                        Cursor_Status |= (Cursor_Status.Draw | Cursor_Status.Blinking);
                        base.Draw();
                        Tuple<Rectangle, Point, Rectangle> i = ((IGMDataItem_Box)(((IGMData_Container)(((IGMDataItem_IGMData)ITEM[0, 0]).Data)).CONTAINER)).Dims;
                        if (i != null)
                            CURSOR[0] = i.Item2;
                    }
                }

                protected override void Init()
                {
                    base.Init();
                    Hide();
                }
            }
        }
    
}
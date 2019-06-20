using Microsoft.Xna.Framework;
using System;

namespace FF8
{
    public partial class Module_main_menu_debug
    {
        private partial class IGM_Junction
        {
            private class IGMData_TopMenu_Auto_Group : IGMData_Group
            {
                public IGMData_TopMenu_Auto_Group(params IGMData[] d) : base( d)
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
}
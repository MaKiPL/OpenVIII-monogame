using Microsoft.Xna.Framework;
using System;

namespace FF8
{
    public partial class Module_main_menu_debug
    {
        #region Classes

        private class IGMDataItem_Box : IGMDataItem
        {
            public FF8String Data { get; set; }
            public Icons.ID? Title { get; set; }
            public Box_Options Options { get; set; }
            public Tuple<Rectangle, Point, Rectangle> Dims { get; private set; }

            public IGMDataItem_Box(FF8String data = null, Rectangle? pos = null, Icons.ID? title = null, Box_Options options = Box_Options.Default) : base(pos)
            {
                Data = data;
                Title = title;
                Options = options;
            }

            public override void Draw()
            {
                if (Enabled)
                {
                    Dims = DrawBox(Pos, Data, Title, options: Options);
                }
            }
        }
        #endregion Classes
    }
}
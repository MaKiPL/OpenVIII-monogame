using Microsoft.Xna.Framework;

namespace OpenVIII
{
    public class IGMData_SmallMsgBox : IGMData
    {
        public IGMData_SmallMsgBox(FF8String data, int x, int y,Icons.ID? title = null, Box_Options options = Box_Options.Default) : base(container: new IGMDataItem_Box(data, new Rectangle(x, y, 0, 0),title, options: Box_Options.Center | Box_Options.Middle))
        {
            if (CONTAINER.GetType() == typeof(IGMDataItem_Box))
            {
                ((IGMDataItem_Box)CONTAINER).Draw(true);
                System.Tuple<Rectangle, Point, Rectangle> dims = ((IGMDataItem_Box)CONTAINER).Dims;
                if ((options & Box_Options.Center) != 0)
                {
                    CONTAINER.X = (int)(dims.Item1.Width / 2 + Menu.StaticSize.X / 2);
                }
                if ((options & Box_Options.Middle) != 0)
                {
                    CONTAINER.Y = (int)(dims.Item1.Height / 2 + Menu.StaticSize.Y / 2);
                }
            }
        }
    }
}
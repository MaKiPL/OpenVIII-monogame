using Microsoft.Xna.Framework;
using System;

namespace OpenVIII
{
    public static partial class Module_main_menu_debug
    {
        #region Enums

        [Flags]
        public enum Box_Options : byte
        {
            Default = 0x0,
            Indent = 0x1,
            Buttom = 0x2,
            SkipDraw = 0x4,
            Center = 0x8,
            Middle = 0x10,
        }

        #endregion Enums

        #region Methods

        private static Tuple<Rectangle, Point, Rectangle> DrawBox(Rectangle dst, FF8String buffer = null, Icons.ID? title = null, Vector2? textScale = null, Vector2? boxScale = null, Box_Options options = Box_Options.Default)
        {
            if (textScale == null) textScale = Vector2.One;
            if (boxScale == null) boxScale = Vector2.One;
            Point cursor = Point.Zero;
            dst.Size = (dst.Size.ToVector2()).ToPoint();
            dst.Location = (dst.Location.ToVector2()).ToPoint();
            Vector2 bgscale = new Vector2(2f) * textScale.Value;
            Rectangle box = dst.Scale(boxScale.Value);
            Rectangle backup = dst;
            Rectangle hotspot = new Rectangle(dst.Location, dst.Size);
            Rectangle font = new Rectangle();
            if ((options & Box_Options.SkipDraw) == 0)
            {
                if (dst.Width > 256 * bgscale.X)
                    Memory.Icons.Draw(Icons.ID.Menu_BG_368, 0, box, bgscale, Fade);
                else
                    Memory.Icons.Draw(Icons.ID.Menu_BG_256, 0, box, bgscale, Fade);
                if (title != null)
                {
                    //dst.Size = (Memory.Icons[title.Value].GetRectangle.Size.ToVector2()  * 2.823317308f).ToPoint();
                    dst.Offset(15, 0);
                    dst.Y = (int)(dst.Y * boxScale.Value.Y);
                    Memory.Icons.Draw(title.Value, 2, dst, (bgscale + new Vector2(.5f)), fade);
                }
                dst = backup;
            }
            if (buffer != null && buffer.Length > 0)
            {
                font = Memory.font.RenderBasicText(buffer, dst.Location.ToVector2(), TextScale * textScale.Value, Fade: fade, skipdraw: true);
                if ((options & Box_Options.Indent) != 0)
                    dst.Offset(70 * textScale.Value.X, 0);
                else if ((options & Box_Options.Center) != 0)
                    dst.Offset(dst.Width / 2 - font.Width / 2, 0);
                else
                    dst.Offset(25 * textScale.Value.X, 0);

                if ((options & Box_Options.Buttom) != 0)
                    dst.Offset(0, (dst.Height - 48));
                else if ((options & Box_Options.Middle) != 0)
                    dst.Offset(0, dst.Height / 2 - font.Height / 2);
                else
                    dst.Offset(0, 21);

                dst.Y = (int)(dst.Y * boxScale.Value.Y);
                font = Memory.font.RenderBasicText(buffer, dst.Location.ToVector2(), TextScale * textScale.Value, Fade: fade, skipdraw: (options & Box_Options.SkipDraw) != 0);
                cursor = dst.Location;
                cursor.Y += (int)(TextScale.Y * 6); // 12 * (3.0375/2)
            }
            return new Tuple<Rectangle, Point, Rectangle>(hotspot, cursor, font);
        }

        #endregion Methods
    }
}
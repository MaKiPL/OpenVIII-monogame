using Microsoft.Xna.Framework;

namespace FF8
{
    public static partial class Module_main_menu_debug
    {
        private static void DrawPointer(Point cursor, sbyte xoffset = -10, bool blink = false)
        {
            Rectangle dst = new Rectangle(cursor, new Point(24 * 2, 16 * 2));
            dst.Offset(-(dst.Width) + xoffset, -(dst.Height * .25f));
            if (blink)
            {
                Memory.Icons.Draw(Icons.ID.Finger_Right, 7, dst, new Vector2(2f), fade);
            }
            Memory.Icons.Draw(Icons.ID.Finger_Right, 2, dst, new Vector2(2f), blink ? fade * s_blink : fade);
        }
    }
}
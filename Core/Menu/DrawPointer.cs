using Microsoft.Xna.Framework;

namespace OpenVIII
{
    public static partial class Module_main_menu_debug
    {
        private static void DrawPointer(Point cursor, Vector2? offset = null, bool blink = false)
        {
            if(offset == null)
            offset = new Vector2(-1.15f, -.3f);
            Vector2 scale = new Vector2(2f);
            Vector2 size = Memory.Icons.GetEntry(Icons.ID.Finger_Right, 0).Size * scale;
            Rectangle dst = new Rectangle(cursor, Point.Zero);
            byte pallet = 2;
            byte fadedpallet = 7;
            dst.Offset(size*offset.Value);
            if (blink)
            {
                Memory.Icons.Draw(Icons.ID.Finger_Right, fadedpallet, dst, scale, fade);
            }
            Memory.Icons.Draw(Icons.ID.Finger_Right, pallet, dst, scale, blink ? fade * s_blink : fade);
        }
    }
}
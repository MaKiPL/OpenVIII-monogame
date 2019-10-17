using Microsoft.Xna.Framework;
using System;

namespace OpenVIII.IGMDataItem
{
    public class Box : Base, I_Data<FF8String>
    {
        #region Properties

        public FF8String Data { get; set; }
        public Tuple<Rectangle, Point, Rectangle> Dims { get; private set; }
        public Box_Options Options { get; set; } = Box_Options.Default;
        public Icons.ID? Title { get; set; } = Icons.ID.None;

        #endregion Properties

        #region Methods

        public override void Draw() => Draw(false);

        public void Draw(bool skipdraw)
        {
            if (Enabled)
            {
                Dims = Menu.DrawBox(Pos, Data, Title, options: skipdraw ? (Options | Box_Options.SkipDraw) : Options);
                if (Blink) //needs tested and tuned
                    Memory.spriteBatch.Draw(blank, Pos, Color.DarkGray * Fade * .5f * Blink_Amount * Blink_Adjustment);
            }
        }

        #endregion Methods
    }
}
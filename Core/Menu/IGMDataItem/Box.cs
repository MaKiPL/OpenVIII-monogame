using Microsoft.Xna.Framework;
using System;

namespace OpenVIII.IGMDataItem
{
        public class Box : Base, I_Data<FF8String>
        {
            #region Constructors

            public Box(FF8String data = null, Rectangle? pos = null, Icons.ID? title = null, Box_Options options = Box_Options.Default) : base(pos)
            {
                Data = data;
                Title = title;
                Options = options;
            }

            #endregion Constructors

            #region Properties

            public FF8String Data { get; set; }
            public Tuple<Rectangle, Point, Rectangle> Dims { get; private set; }
            public Box_Options Options { get; set; }
            public Icons.ID? Title { get; set; }

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

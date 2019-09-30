using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace OpenVIII
{
    public class IGMDataItem_Texture : IGMDataItem, I_Data<Texture2D>, I_Color
    {
        #region Constructors
        protected IGMDataItem_Texture(Rectangle? pos = null) : base(pos)
        { }
        public IGMDataItem_Texture(Texture2D data, Rectangle? pos = null, Color? color = null, Color? faded_color = null, float blink_adjustment = 1f) : base(pos)
        {
            Data = data;
            Color = color ?? Color.White;
            Faded_Color = faded_color ?? Color;
            Blink_Adjustment = blink_adjustment;
        }

        #endregion Constructors

        #region Properties
        public Rectangle Restriction { get; set; }
        public override bool Blink { get => base.Blink && (Color != Faded_Color); set => base.Blink = value; }
        public Texture2D Data { get; set; }

        #endregion Properties

        #region Methods

        public override void Draw()
        {
            if (Enabled)
            {
                var p = Pos;
                Rectangle src = new Rectangle(0, 0, Data.Width, Data.Height);
                if (!Restriction.IsEmpty)
                {
                    p = Rectangle.Intersect(p, Restriction);
                    if (p != Pos)
                    {
                        var missing = new Rectangle(
                            Math.Abs(p.X - Pos.X),
                            Math.Abs(p.Y - Pos.Y),
                            Pos.Width - p.Width,
                            Pos.Height - p.Height
                            );
                        
                        Vector2 scale = new Vector2(
                            (float)Width / Data.Width,
                            (float)Height / Data.Height);
                        Vector2 ploc = (src.Location.ToVector2() * scale + missing.Location.ToVector2()) / scale;
                        Vector2 pSize = (src.Size.ToVector2() * scale - missing.Size.ToVector2()) / scale;

                        src.Location = ploc.ToPoint();
                        src.Size = pSize.ToPoint();
                    }
                    
                }
                if (!Blink)
                    Memory.spriteBatch.Draw(Data, p, src, Color * Fade);
                else
                    Memory.spriteBatch.Draw(Data, p, src, Color.Lerp(Color, Faded_Color, Menu.Blink_Amount) * Blink_Adjustment * Fade);
                //    if (Blink)
                //        Memory.spriteBatch.Draw(Data, Pos, null, Faded_Color * Fade * Blink_Amount * Blink_Adjustment);
            }
        }

        #endregion Methods
    }
}
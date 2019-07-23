using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace OpenVIII
{
    public class IGMDataItem_Texture : IGMDataItem<Texture2D>
    {
        #region Constructors

        public IGMDataItem_Texture(Texture2D data, Rectangle? pos = null, Color? color = null, Color? faded_color = null, float blink_adjustment = 1f) : base(data, pos)
        {
            Color = color ?? Color.White;
            Faded_Color = faded_color ?? Color;
            Blink_Adjustment = blink_adjustment;
        }

        #endregion Constructors

        #region Properties

        public override bool Blink { get => base.Blink && (Color != Faded_Color); set => base.Blink = value; }
        public Color Faded_Color { get; set; }

        #endregion Properties

        #region Methods

        public override void Draw()
        {
            if (Enabled)
            {
                Memory.spriteBatch.Draw(Data, Pos, null, Color * Fade);
                if (Blink)
                    Memory.spriteBatch.Draw(Data, Pos, null, Faded_Color * Fade * Blink_Amount * Blink_Adjustment);
            }
        }

        #endregion Methods
    }
}
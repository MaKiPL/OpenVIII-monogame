using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace OpenVIII
{
    public class IGMDataItem_Texture : IGMDataItem, I_Data<Texture2D>, I_Color
    {
        #region Constructors

        public IGMDataItem_Texture(Texture2D data, Rectangle? pos = null, Color? color = null, Color? faded_color = null, float blink_adjustment = 1f) : base(pos)
        {
            Data = data;
            Color = color ?? Color.White;
            Faded_Color = faded_color ?? Color;
            Blink_Adjustment = blink_adjustment;
        }

        #endregion Constructors

        #region Properties

        public override bool Blink { get => base.Blink && (Color != Faded_Color); set => base.Blink = value; }
        public Texture2D Data { get; set; }
        public Color Faded_Color { get; set; }

        #endregion Properties

        #region Methods

        public override void Draw()
        {
            if (Enabled)
            {
                if (!Blink)
                    Memory.spriteBatch.Draw(Data, Pos, null, Color * Fade);
                else
                    Memory.spriteBatch.Draw(Data, Pos, null, Color.Lerp(Color, Faded_Color, Menu.Blink_Amount) * Blink_Adjustment * Fade);
                //    if (Blink)
                //        Memory.spriteBatch.Draw(Data, Pos, null, Faded_Color * Fade * Blink_Amount * Blink_Adjustment);
            }
        }

        #endregion Methods
    }
}
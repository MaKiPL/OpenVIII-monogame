using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace OpenVIII
{
    #region Classes

    public class IGMDataItem_Texture : IGMDataItem
    {
        public Texture2D Data { get; set; }
        public Color Faded_Color { get; set; }
        public float Blink_Adjustment { get; set; }

        public bool Blink => Color != Faded_Color;
        public IGMDataItem_Texture(Texture2D data, Rectangle? pos = null, Color? color = null, Color? faded_color = null, float blink_adjustment = 1f) : base(pos)
        {
            Data = data;
            Color = color?? Color.White;
            Faded_Color = faded_color ?? Color;
            Blink_Adjustment = blink_adjustment;
        }

        public override void Draw()
        {
            if (Enabled)
            {
                Memory.spriteBatch.Draw(Data, Pos, null, Color * Fade);
                if(Blink)
                    Memory.spriteBatch.Draw(Data, Pos, null, Faded_Color * Fade * Blink_Amount * Blink_Adjustment);
            }
        }
    }

    #endregion Classes
}
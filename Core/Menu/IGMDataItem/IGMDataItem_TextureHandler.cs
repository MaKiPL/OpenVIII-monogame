using Microsoft.Xna.Framework;

namespace OpenVIII
{
    public class IGMDataItem_TextureHandler : IGMDataItem<TextureHandler>
    {
        #region Constructors

        public IGMDataItem_TextureHandler(TextureHandler data, Rectangle? pos = null) : base(data, pos)
        {
        }

        #endregion Constructors

        #region Methods

        public override void Draw()
        {
            if (Enabled)
            {
                Data.Draw(Pos, null, Color * Fade);//4
                if (Blink)
                    Data.Draw(Pos, null, Color.DarkGray * Blink_Amount * Blink_Adjustment * Fade);//4
            }
        }

        #endregion Methods
    }
}
using Microsoft.Xna.Framework;

namespace OpenVIII
{
    public class IGMDataItem_TextureHandler : IGMDataItem, I_Data<TextureHandler>
    {
        #region Constructors

        public IGMDataItem_TextureHandler(TextureHandler data, Rectangle? pos = null) : base(pos) => Data = data;

        #endregion Constructors

        #region Properties

        public TextureHandler Data { get; set; }

        #endregion Properties

        #region Methods
        Color Faded_Color => Color.DarkGray;
        public override void Draw()
        {
            if (Enabled)
            {
                if (!Blink)
                    Data.Draw(Pos, null, Color * Fade);//4
                //if (Blink)
                //    Data.Draw(Pos, null, Color.DarkGray * Blink_Amount * Blink_Adjustment * Fade);//4
                else
                    Data.Draw(Pos, null, Color.Lerp(Color, Faded_Color, Menu.Blink_Amount) * Blink_Adjustment * Fade);
            }
        }

        #endregion Methods
    }
}
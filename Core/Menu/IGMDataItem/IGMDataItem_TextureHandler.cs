using Microsoft.Xna.Framework;

namespace OpenVIII
{
    public class IGMDataItem_TextureHandler : IGMDataItem
    {
        #region Constructors

        public IGMDataItem_TextureHandler(TextureHandler data, Rectangle? pos = null) : base(pos) => this.Data = data;

        #endregion Constructors

        #region Properties

        public TextureHandler Data { get; set; }

        #endregion Properties

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
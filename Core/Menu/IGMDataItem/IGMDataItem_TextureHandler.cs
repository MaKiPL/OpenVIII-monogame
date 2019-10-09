﻿using Microsoft.Xna.Framework;
using OpenVIII.IGMDataItem;

namespace OpenVIII
{
    public class IGMDataItem_TextureHandler : Base, I_Data<TextureHandler>
    {
        #region Constructors

        public IGMDataItem_TextureHandler(TextureHandler data, Rectangle? pos = null) : base(pos)
        {
            Faded_Color = Color.DarkGray;
            Data = data;
        }

        #endregion Constructors

        #region Properties

        public TextureHandler Data { get; set; }

        #endregion Properties

        #region Methods
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
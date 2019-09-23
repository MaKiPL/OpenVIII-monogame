using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace OpenVIII
{
    public class IGMDataItem_Renzokeken_Gradient : IGMDataItem_Texture
    {
        #region Fields

        private Color Color_default;
        private Slide<int> HitSlide;
        private Rectangle HotSpot;

        #endregion Fields

        #region Constructors
        public override void Reset()
        {
            Restart();
            base.Reset();
        }

        public IGMDataItem_Renzokeken_Gradient(Rectangle? pos = null, Color? color = null, Color? faded_color = null, float blink_adjustment = 1f, Rectangle? hotspot = null, Rectangle? restriction = null, double time = 0d, double delay = 0d) : base(pos)
        {
            HotSpot = hotspot ?? Rectangle.Empty;
            Restriction = restriction ?? Rectangle.Empty;
            int dark = 12;
            int fade = 180;
            Color lightline = new Color(118, 118, 118, 255);
            Color darkline = new Color(58, 58, 58, 255);
            Color[] cfade = new Color[12 + 180];
            int i;
            for (i = 0; i < dark; i++)
                cfade[i] = darkline;
            for (; i < cfade.Length; i++)
                cfade[i] = Color.Lerp(lightline, Color.TransparentBlack, (float)(i - dark) / (fade));
            Data = new Texture2D(Memory.graphics.GraphicsDevice, cfade.Length, 1);
            Width = Data.Width;
            Data.SetData(cfade);
            Color = color ?? Color.White;
            Color_default = Color;
            Faded_Color = faded_color ?? Color;
            Blink_Adjustment = blink_adjustment;

            HitSlide = new Slide<int>(Restriction.X + Restriction.Width, Restriction.X - Width, time, Lerp) { DelayMS = delay };
            int Lerp(int x, int y, float p) => (int)Math.Round(MathHelper.Lerp(x, y, p));
        }

        #endregion Constructors

        #region Properties

        public bool Trigger { get; private set; }
        public bool Done => HitSlide.Done;

        #endregion Properties

        #region Methods

        public void Restart()
        {
            Show();
            Color = Color_default;
            HitSlide.Restart();
        }

        public override bool Update()
        {
            X = HitSlide.Update();

            if (HotSpot.Contains(Pos.Location))
            {
                Color = Faded_Color;
                Trigger = true;
            }
            else
            {
                Trigger = false;
            }
            //if (HitSlide.Done) HitSlide.Restart();

            return base.Update();
        }

        #endregion Methods
    }
}
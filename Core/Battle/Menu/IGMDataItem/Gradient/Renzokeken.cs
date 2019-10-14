using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace OpenVIII.IGMDataItem.Gradient
{
    public class Renzokeken : Texture
    {
        #region Fields

        protected Color Color_default;
        protected Slide<int> HitSlide;
        protected Rectangle HotSpot;
        protected bool _trigger;

        #endregion Fields

        #region Constructors
        public override void Reset()
        {
            Restart();
            base.Reset();
        }

        public Renzokeken(Rectangle? pos = null, Color? color = null, Color? faded_color = null, float blink_adjustment = 1f, Rectangle? hotspot = null, Rectangle? restriction = null, double time = 0d, double delay = 0d, Color? darkcolor = null, bool rev = false, bool vanish = true)
        {
            Pos = pos ?? Rectangle.Empty;
            HotSpot = hotspot ?? Rectangle.Empty;
            Restriction = restriction ?? Rectangle.Empty;
            float dark = 0.067f;
            float fade = 0.933f;
            int total = 12 + 180;
            if (pos.HasValue && pos.Value.Width > 0)
            {
                total = pos.Value.Width;
            }
            Color lightline = new Color(118, 118, 118, 255);
            Color darkline = new Color(58, 58, 58, 255);
            Color[] cfade = new Color[total];
            int i;
            if (!rev)
            {
                for (i = 0; i < dark * total; i++)
                    cfade[i] = darkline;
                for (; i < cfade.Length; i++)
                    cfade[i] = Color.Lerp(lightline, darkcolor ?? Color.TransparentBlack, (i - (dark * total)) / (fade * total));
            }
            else
            {
                for (i = 0; i < cfade.Length - (dark * total); i++)
                    cfade[i] = Color.Lerp(Color.TransparentBlack, lightline, i / (fade * total));

                for (; i < cfade.Length; i++)
                    cfade[i] = darkline;
            }
            Data = new Texture2D(Memory.graphics.GraphicsDevice, cfade.Length, 1);
            Width = Data.Width;
            Data.SetData(cfade);
            Color = color ?? Color.White;
            Color_default = Color;
            Faded_Color = faded_color ?? Color;
            Blink_Adjustment = blink_adjustment;

            if (vanish) HitSlide = new Slide<int>(Restriction.X + Restriction.Width, Restriction.X - Width, time, Lerp) { DelayMS = delay };
            else HitSlide = new Slide<int>(Restriction.X, Restriction.X - Width, time, Lerp) { DelayMS = delay };
            if (rev) HitSlide.Reverse();

            int Lerp(int x, int y, float p) => (int)Math.Round(MathHelper.Lerp(x, y, p));
        }

        #endregion Constructors

        #region Properties

        public bool Trigger { get => (_trigger && !Done); private set => _trigger = value; }
        public bool Done => HitSlide.Done || !Enabled;

        
        #endregion Properties

        #region Methods

        public void Restart()
        {
            //Show();
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
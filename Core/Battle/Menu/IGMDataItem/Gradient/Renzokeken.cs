using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace OpenVIII.IGMDataItem.Gradient
{
    public class Renzokeken : Texture
    {
        #region Fields

        protected bool _trigger;
        protected Color Color_default;
        protected Slide<int> HitSlide;
        protected Rectangle HotSpot;

        #endregion Fields

        #region Constructors

        private Renzokeken()
        {
        }

        #endregion Constructors

        #region Properties

        public bool Done => HitSlide.Done || !Enabled;

        public bool Trigger { get => (_trigger && !Done); private set => _trigger = value; }

        #endregion Properties

        #region Methods

        public static Renzokeken Create(Rectangle? pos = null, Color? color = null, Color? faded_color = null, float blink_adjustment = 1f, Rectangle? hotspot = null, Rectangle? restriction = null, double time = 0d, double delay = 0d, Color? darkcolor = null, bool rev = false, bool vanish = true)
        {
            int total = 12 + 180;
            if (pos.HasValue && pos.Value.Width > 0)
            {
                total = pos.Value.Width;
            }
            Color[] cfade = new Color[total];
            Renzokeken r = new Renzokeken
            {
                Data = new Texture2D(Memory.graphics.GraphicsDevice, cfade.Length, 1),
                _pos = pos ?? Rectangle.Empty,
                HotSpot = hotspot ?? Rectangle.Empty,
                Restriction = restriction ?? Rectangle.Empty,
                Color = color ?? Color.White,
                Color_default = color ?? Color.White,
                Faded_Color = faded_color ?? color ?? Color.White,
                Blink_Adjustment = blink_adjustment
            };
            float dark = 0.067f;
            float fade = 0.933f;
            Color lightline = new Color(118, 118, 118, 255);
            Color darkline = new Color(58, 58, 58, 255);
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
            r.Width = r.Data.Width;
            r.Data.SetData(cfade);

            if (vanish) r.HitSlide = new Slide<int>(r.Restriction.X + r.Restriction.Width, r.Restriction.X - r.Width, time, Lerp) { DelayMS = delay };
            else r.HitSlide = new Slide<int>(r.Restriction.X, r.Restriction.X - r.Width, time, Lerp) { DelayMS = delay };
            if (rev) r.HitSlide.Reverse();

            int Lerp(int x, int y, float p) => (int)Math.Round(MathHelper.Lerp(x, y, p));
            return r;
        }

        public override void Reset()
        {
            Restart();
            base.Reset();
        }

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
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OpenVIII.Encoding.Tags;
using System;
using System.Diagnostics;

namespace OpenVIII
{
    public class IGMData_Renzokeken : IGMData
    {
        #region Fields

        private byte _count = 0;
        private int _hits = 7;
        private Slide<Color> HitSlider = new Slide<Color>(Color.White, Color.TransparentBlack, 300, Color.Lerp);
        private Color newattack;

        private Color rc;
        private Color rcdim;

        #endregion Fields

        #region Methods

        protected override void Init()
        {
            Texture2D pixel = new Texture2D(Memory.graphics.GraphicsDevice, 1, 1);
            pixel.SetData(new Color[] { Color.White });

            Memory.Icons[Icons.ID.Text_Cursor][0].Offset = Vector2.Zero;
            Memory.Icons.Trim(Icons.ID.Text_Cursor, 6);
            EntryGroup split = Memory.Icons[Icons.ID.Text_Cursor];
            EntryGroup e = Memory.Icons[Icons.ID.Text_Cursor];

            Rectangle r = CONTAINER.Pos; //new Rectangle(40, 524, 880, 84);
            r.Inflate(-16, -20);
            r.X += r.X % 4;
            r.Y += r.Y % 4;
            r.Width += r.Width % 4;
            r.Height += r.Height % 4;
            rc = Memory.Icons.MostSaturated(Icons.ID.Text_Cursor, 6);
            rcdim = Memory.Icons.MostSaturated(Icons.ID.Text_Cursor, 2);
            ITEM[0, 0] = new IGMDataItem_Texture(pixel, r, rcdim);
            r.Inflate(-4, -4);
            ITEM[1, 0] = new IGMDataItem_Texture(pixel, r, Color.Black);
            float scale = (float)r.Height / e.Height;
            int w = (int)(e.Width * scale);
            ITEM[Count - 3, 0] = new IGMDataItem_Icon(Icons.ID.Text_Cursor, new Rectangle(r.X + 80, r.Y, w, r.Height), 2, scale: new Vector2(scale));
            ITEM[Count - 2, 0] = new IGMDataItem_Icon(Icons.ID.Text_Cursor, new Rectangle(r.X + 208, r.Y, w, r.Height), 2, scale: new Vector2(scale));
            Rectangle hotspot = new Rectangle(r.X + 80 + (w / 2), r.Y + 4, 208 - 80, r.Height - 8);
            ITEM[Count - 4, 0] = new IGMDataItem_Texture(pixel, hotspot, Color.TransparentBlack);
            //Rectangle hotspotbox = hotspot;
            hotspot.Width += (int)(hotspot.Width * .50f);
            Rectangle tr = new Rectangle(r.X + 208 + (w / 2), r.Y + 4, 0, r.Height - 4);

            Memory.Icons[Icons.ID.Trigger_][0].Offset = Vector2.Zero;
            Memory.Icons.Trim(Icons.ID.Trigger_, 2);
            e = Memory.Icons[Icons.ID.Trigger_];
            scale = ((float)r.Height - 8) / e.Height;
            w = (int)(e.Width * scale);
            int trigwidtharea = (r.Right - tr.Left);
            int xbak = tr.X;
            tr.X += trigwidtharea / 2 - w / 2;

            ITEM[Count - 1, 0] = new IGMDataItem_Icon(Icons.ID.Trigger_, tr, 6, scale: new Vector2(scale));// { Color = rc};

            newattack = new Color(104, 80, 255);
            int delay = 500;
            const int Time = 2000;
            Rectangle pos = new Rectangle(r.X, r.Y + 4, 0, r.Height - 8);
            r.Inflate(-4, -4);
            for (int x = 0; x <= _hits && x <= 7; x++)
                ITEM[2 + x, 0] = new IGMDataItem_Renzokeken_Gradient(pos, newattack, rc, 1f, hotspot, r, time: Time, delay * (x));
            float totalx = 0;
            for (byte i = 0; i <= 7; i++)
            {
                Memory.Icons.Trim(Icons.ID._0_Hit_ + i, 2);
                e = Memory.Icons[Icons.ID._0_Hit_ + i];
                totalx += e[0].Offset.X;
            }
            float avgx = (float)Math.Round(totalx / 8);
            for (byte i = 0; i <= 7; i++)
            {
                Memory.Icons[Icons.ID._0_Hit_ + i][0].Offset.X = avgx;
            }
            e = Memory.Icons[Icons.ID._0_Hit_];
            scale = ((float)r.Height) / e.Height;
            w = (int)(e.Width * scale);
            tr.X = xbak + trigwidtharea / 2 - w / 2;
            ITEM[Count - 5, 0] = new IGMDataItem_Icon(Icons.ID._0_Hit_, tr, 2, scale: new Vector2(scale));
            Memory.Icons.Trim(Icons.ID.Perfect__, 2);
            e = Memory.Icons[Icons.ID.Perfect__];
            scale = ((float)r.Height) / e.Height;
            w = (int)(e.Width * scale);
            tr.X = xbak + trigwidtharea / 2 - w / 2;
            ITEM[Count - 6, 0] = new IGMDataItem_Icon(Icons.ID.Perfect__, tr, 8, scale: new Vector2(scale));
            Reset();
            base.Init();
            Show();
            Cursor_Status = Cursor_Status.Enabled | Cursor_Status.Static | Cursor_Status.Hidden;
        }

        #endregion Methods

        #region Constructors

        public IGMData_Renzokeken(Rectangle? pos = null) : base(15, 1, new IGMDataItem_Box(pos: pos ?? new Rectangle(24, 501, 912, 123), title: Icons.ID.SPECIAL), 0, 0, Characters.Squall_Leonhart)
        {
        }

        #endregion Constructors

        public override bool Inputs()
        {
            skipsnd = true;
            if (Input2.DelayedButton(FF8TextTagKey.RotateRight))
            {
                Inputs_OKAY();
                return true;
            }
            return base.Inputs();
        }

        public override bool Inputs_CANCEL() => base.Inputs_OKAY();

        public override bool Inputs_OKAY()
        {
            foreach (Menu_Base i in ITEM)
            {
                if (i?.GetType() == typeof(IGMDataItem_Renzokeken_Gradient))
                {
                    if (((IGMDataItem_Renzokeken_Gradient)i).Trigger)
                    {
                        skipsnd = true;
                        base.Inputs_OKAY();
                        HitSlider.Restart();
                        i.Hide();
                        _count++;
                        break;
                    }
                }
            }

            return true;
        }

        public override void Reset()
        {
            Hide();
            ITEM[Count - 5, 0].Hide();
            ITEM[Count - 6, 0].Hide();
            _count = 0;
            base.Reset();
        }

        public override bool Update()
        {
            bool done = false;
            bool hot = false;

            ((IGMDataItem_Icon)ITEM[Count - 5, 0]).Data = Icons.ID._0_Hit_ + _count;
            ((IGMDataItem_Texture)ITEM[Count - 4, 0]).Color = HitSlider.Update();

            int hotcnt = 0;
            foreach (Menu_Base i in ITEM)
            {
                if (i?.GetType() == typeof(IGMDataItem_Renzokeken_Gradient))
                {
                    done = !((IGMDataItem_Renzokeken_Gradient)i).Done || done;
                    hot = ((IGMDataItem_Renzokeken_Gradient)i).Trigger || hot;
                    if (((IGMDataItem_Renzokeken_Gradient)i).Done)
                        hotcnt++;
                }
            }
            if (!done)
            {
                foreach (Menu_Base i in ITEM)
                {
                    if (i?.GetType() == typeof(IGMDataItem_Renzokeken_Gradient))
                    {
                        ((IGMDataItem_Renzokeken_Gradient)i).Restart();
                    }
                }
            }
            if (hot)
            {
                ((IGMDataItem_Icon)ITEM[Count - 3, 0]).Palette = 6;
                ((IGMDataItem_Icon)ITEM[Count - 2, 0]).Palette = 6;
                ((IGMDataItem_Texture)ITEM[0, 0]).Color = rc;
                ITEM[Count - 1, 0].Show();
                ITEM[Count - 5, 0].Hide();
                ITEM[Count - 6, 0].Hide();
            }
            else
            {
                ((IGMDataItem_Icon)ITEM[Count - 3, 0]).Palette = 2;
                ((IGMDataItem_Icon)ITEM[Count - 2, 0]).Palette = 2;
                ((IGMDataItem_Texture)ITEM[0, 0]).Color = rcdim;
                if ((hotcnt >= _hits-2) && ITEM[Count - 1, 0].Enabled)
                {
                    if (_count >= _hits)
                        ITEM[Count - 6, 0].Show();
                    else if(_count>0)
                        ITEM[Count - 5, 0].Show();
                }
                ITEM[Count - 1, 0].Hide();
            }
            return base.Update();
        }
    }

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
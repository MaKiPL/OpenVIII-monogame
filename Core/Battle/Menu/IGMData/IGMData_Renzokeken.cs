using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace OpenVIII
{
    public class IGMDataItem_Renzokeken_Gradient : IGMDataItem_Texture
    {
        public IGMDataItem_Renzokeken_Gradient(Rectangle? pos = null, Color? color = null, Color? faded_color = null, float blink_adjustment = 1f) : base(pos)
        {
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
            Faded_Color = faded_color ?? Color;
            Blink_Adjustment = blink_adjustment;
        }
    }
    public class IGMData_Renzokeken : IGMData
    {
        #region Fields

        private Slide<int> HitSlide;
        private Rectangle hotspot;
        private Color newattack;

        private Color rc;

        #endregion Fields

        #region Methods

        private int Lerp(int x, int y, float p) => (int)Math.Round(MathHelper.Lerp(x, y, p));

        protected override void Init()
        {
            Texture2D pixel = new Texture2D(Memory.graphics.GraphicsDevice, 1, 1);
            pixel.SetData(new Color[] { Color.White });

            Memory.Icons.Trim(Icons.ID.Text_Cursor, 6);
            EntryGroup split = Memory.Icons[Icons.ID.Text_Cursor];
            var e = Memory.Icons[Icons.ID.Text_Cursor];

            Rectangle r = CONTAINER.Pos; //new Rectangle(40, 524, 880, 84);
            r.Inflate(-16, -20);
            r.X += r.X % 4;
            r.Y += r.Y % 4;
            r.Width += r.Width % 4;
            r.Height += r.Height % 4;
            rc = Memory.Icons.MostSaturated(Icons.ID.Text_Cursor, 6);
            ITEM[0, 0] = new IGMDataItem_Texture(pixel, r, rc);
            r.Inflate(-4, -4);
            ITEM[1, 0] = new IGMDataItem_Texture(pixel, r, Color.Black);
            int w = (int)(e.Width*((float)r.Height / e.Height));
            ITEM[Count - 2, 0] = new IGMDataItem_Icon(Icons.ID.Text_Cursor, new Rectangle(r.X + 80, r.Y, w, r.Height), 6, scale: new Vector2(((float)r.Height / e.Height)));
            ITEM[Count - 1, 0] = new IGMDataItem_Icon(Icons.ID.Text_Cursor, new Rectangle(r.X + 208, r.Y, w, r.Height), 6, scale: new Vector2(((float)r.Height / e.Height)));
            hotspot = new Rectangle(r.X + 80 + (w / 2), r.Y, 208-80, r.Height);
            newattack = new Color(104, 80, 255);
            ITEM[2, 0] = new IGMDataItem_Renzokeken_Gradient(new Rectangle(r.X, r.Y+4 , 0, r.Height - 8), newattack, rc);
            r.Inflate(-4, -4);
            ((IGMDataItem_Texture)ITEM[2, 0]).Restriction = r;
            HitSlide = new Slide<int>(((IGMDataItem_Texture)ITEM[1, 0]).X - ((IGMDataItem_Texture)ITEM[1, 0]).Width+4, ((IGMDataItem_Texture)ITEM[1, 0]).X + ((IGMDataItem_Texture)ITEM[1, 0]).Width-4, 5000, Lerp);
            HitSlide.Reverse();
            Reset();
            base.Init();
            Show();
        }

        #endregion Methods

        #region Constructors

        public IGMData_Renzokeken() : base(5, 1, new IGMDataItem_Box(pos: new Rectangle(24, 501, 912, 123), title: Icons.ID.SPECIAL), 0, 0, Characters.Squall_Leonhart)
        {
        }

        #endregion Constructors

        public override void Reset() =>
            //Hide();
            base.Reset();

        public override bool Update()
        {
            ((IGMDataItem_Texture)ITEM[2, 0]).X = HitSlide.Update();
            if (hotspot.Contains(((IGMDataItem_Texture)ITEM[2, 0]).Pos.Location))
                ((IGMDataItem_Texture)ITEM[2, 0]).Color = rc;
            else ((IGMDataItem_Texture)ITEM[2, 0]).Color = newattack;
            if (HitSlide.Done) HitSlide.Restart();

            return base.Update();
        }
    }
}
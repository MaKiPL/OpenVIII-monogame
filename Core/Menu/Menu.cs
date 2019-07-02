using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace OpenVIII
{
    public abstract class Menu
    {
        public abstract void SetMode(Enum mode);

        public abstract Enum GetMode();

        public bool Enabled { get; private set; } = true;

        public virtual void Hide() => Enabled = false;

        public virtual void Show() => Enabled = true;

        public Dictionary<Enum, IGMData> Data;


        private Vector2 _size;
        public static Matrix Focus { get; protected set; }

        private bool skipdata;

        public Vector2 Size { get => _size; protected set => _size = value; }

        public Menu()
        {
            Data = new Dictionary<Enum, IGMData>();
            Init();
            skipdata = true;
            ReInit();
            skipdata = false;
        }

        protected virtual void Init()
        {
        }

        public virtual void ReInit()
        {
            if (!skipdata)
                foreach (KeyValuePair<Enum, IGMData> i in Data)
                    i.Value.ReInit();
        }

        public virtual void StartDraw()
        {
            if (Enabled)
                Memory.SpriteBatchStartAlpha(ss: SamplerState.PointClamp, tm: Focus);
        }

        public virtual void Draw()
        {
            StartDraw();
            DrawData();
            EndDraw();
        }

        public virtual void DrawData()
        {
            if (!skipdata && Enabled)
                foreach (KeyValuePair<Enum, IGMData> i in Data)
                    i.Value.Draw();
        }

        public virtual void EndDraw()
        {
            if (Enabled)
                Memory.SpriteBatchEnd();
        }

        protected Vector2 vp { get; set; } = new Vector2(Memory.graphics.GraphicsDevice.Viewport.Width, Memory.graphics.GraphicsDevice.Viewport.Height);

        public virtual bool Update()
        {
            bool ret = false;
            Vector2 Zoom = Memory.Scale(Size.X, Size.Y, Memory.ScaleMode.FitBoth);
            Focus = Matrix.CreateTranslation((Size.X / -2), (Size.Y / -2), 0) *
                Matrix.CreateScale(new Vector3(Zoom.X, Zoom.Y, 1)) *
                Matrix.CreateTranslation(vp.X / 2, vp.Y / 2, 0);
            if (Enabled)
            {
                //todo detect when there is no saves detected.
                //check for null
                if (!skipdata)
                    foreach (KeyValuePair<Enum, IGMData> i in Data)
                    {
                        ret = i.Value.Update() || ret;
                    }
            }
            return Inputs() || ret;
        }

        public static Vector2 TextScale { get; } = new Vector2(2.545455f, 3.0375f);
        protected abstract bool Inputs();
        public static float Fade { get; set; } = 1f;
        public static float Blink_Amount { get; set; } = 1f;
        public static Tuple<Rectangle, Point, Rectangle> DrawBox(Rectangle dst, FF8String buffer = null, Icons.ID? title = null, Vector2? textScale = null, Vector2? boxScale = null, Box_Options options = Box_Options.Default)
        {
            if (textScale == null) textScale = Vector2.One;
            if (boxScale == null) boxScale = Vector2.One;
            Point cursor = Point.Zero;
            dst.Size = (dst.Size.ToVector2()).ToPoint();
            dst.Location = (dst.Location.ToVector2()).ToPoint();
            Vector2 bgscale = new Vector2(2f) * textScale.Value;
            Rectangle box = dst.Scale(boxScale.Value);
            Rectangle backup = dst;
            Rectangle hotspot = new Rectangle(dst.Location, dst.Size);
            Rectangle font = new Rectangle();
            if ((options & Box_Options.SkipDraw) == 0)
            {
                if (dst.Width > 256 * bgscale.X)
                    Memory.Icons.Draw(Icons.ID.Menu_BG_368, 0, box, bgscale, Fade);
                else
                    Memory.Icons.Draw(Icons.ID.Menu_BG_256, 0, box, bgscale, Fade);
                if (title != null)
                {
                    //dst.Size = (Memory.Icons[title.Value].GetRectangle.Size.ToVector2()  * 2.823317308f).ToPoint();
                    dst.Offset(15, 0);
                    dst.Y = (int)(dst.Y * boxScale.Value.Y);
                    Memory.Icons.Draw(title.Value, 2, dst, (bgscale + new Vector2(.5f)), Fade);
                }
                dst = backup;
            }
            if (buffer != null && buffer.Length > 0)
            {
                font = Memory.font.RenderBasicText(buffer, dst.Location.ToVector2(), TextScale * textScale.Value, Fade: Fade, skipdraw: true);
                if ((options & Box_Options.Indent) != 0)
                    dst.Offset(70 * textScale.Value.X, 0);
                else if ((options & Box_Options.Center) != 0)
                    dst.Offset(dst.Width / 2 - font.Width / 2, 0);
                else
                    dst.Offset(25 * textScale.Value.X, 0);

                if ((options & Box_Options.Buttom) != 0)
                    dst.Offset(0, (dst.Height - 48));
                else if ((options & Box_Options.Middle) != 0)
                    dst.Offset(0, dst.Height / 2 - font.Height / 2);
                else
                    dst.Offset(0, 21);

                dst.Y = (int)(dst.Y * boxScale.Value.Y);
                font = Memory.font.RenderBasicText(buffer, dst.Location.ToVector2(), TextScale * textScale.Value, Fade: Fade, skipdraw: (options & Box_Options.SkipDraw) != 0);
                cursor = dst.Location;
                cursor.Y += (int)(TextScale.Y * 6); // 12 * (3.0375/2)
            }
            return new Tuple<Rectangle, Point, Rectangle>(hotspot, cursor, font);
        }

        public static Point MouseLocation => Input.MouseLocation.Transform(Menu.Focus);
        public static void DrawPointer(Point cursor, Vector2? offset = null, bool blink = false)
        {
            if (offset == null)
                offset = new Vector2(-1.15f, -.3f);
            Vector2 scale = new Vector2(2f);
            Vector2 size = Memory.Icons.GetEntry(Icons.ID.Finger_Right, 0).Size * scale;
            Rectangle dst = new Rectangle(cursor, Point.Zero);
            byte pallet = 2;
            byte fadedpallet = 7;
            dst.Offset(size * offset.Value);
            if (blink)
            {
                Memory.Icons.Draw(Icons.ID.Finger_Right, fadedpallet, dst, scale, Fade);
            }
            Memory.Icons.Draw(Icons.ID.Finger_Right, pallet, dst, scale, blink ? Fade * Blink_Amount : Fade);
        }    
    }
}
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenVIII.IGMData.Limit
{
    public class Renzokuken : IGMData.Base
    {
        #region Fields

        private const int Renzokuken_Gradient_Width = 192;
        private static object locker = new object();
        private static Texture2D pixel;
        private static Color Renzokenken_Seperator_Color;
        private static Color Renzokenken_Seperator_Color_Faded;
        private byte _count = 0;
        private int _hits = 7;
        private TimeSpan delayMS;
        private Slide<Color> HitSlider = new Slide<Color>(Color.White, Color.TransparentBlack, HitTime, Color.Lerp);
        private Color newattack;

        #endregion Fields

        #region Properties

        private static TimeSpan HitTime => TimeSpan.FromMilliseconds(300d);
        private static TimeSpan RenzoDelay => TimeSpan.FromMilliseconds(500d);
        private static TimeSpan RenzoTime => TimeSpan.FromMilliseconds(2000d);

        #endregion Properties

        #region Methods

        public override bool Inputs() => base.Inputs() || true;

        public static Renzokuken Create(Rectangle? pos = null) => Create<Renzokuken>(15, 1, new IGMDataItem.Box { Pos = pos ?? new Rectangle(24, 501, 912, 123), Title = Icons.ID.SPECIAL }, 0, 0, Memory.State?[Characters.Squall_Leonhart]);

        public static void ThreadUnsafeOperations()
        {
            lock (locker)
            {
                if (pixel == null)
                {
                    pixel = new Texture2D(Memory.graphics.GraphicsDevice, 1, 1);
                    pixel.SetData(new Color[] { Color.White });
                }
                Renzokenken_Seperator_Color = Memory.Icons.MostSaturated(Icons.ID.Renzokuken_Seperator, 6);
                Renzokenken_Seperator_Color_Faded = Memory.Icons.MostSaturated(Icons.ID.Renzokuken_Seperator, 2);
            }
        }

        public override bool Inputs_CANCEL() => base.Inputs_OKAY();

        public override bool Inputs_OKAY()
        {
            skipsnd = true;
            foreach (var i in ITEM)
            {
                if (i?.GetType() == typeof(IGMDataItem.Gradient.Renzokuken))
                {
                    if (((IGMDataItem.Gradient.Renzokuken)i).Trigger)
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

        public override bool Inputs_RotateRight() => Inputs_OKAY();

        public override void Reset()
        {
            Hide();
            ITEM[Count - 5, 0].Hide();
            ITEM[Count - 6, 0].Hide();
            _count = 0;
            delayMS = TimeSpan.Zero;
            base.Reset();
        }

        public void Reset(int hits)
        {
            _hits = hits;
            Refresh();
            Reset();
        }

        public override bool Update()
        {
            if (!Enabled) return false;
            var done = false;
            var hot = false;

            ((IGMDataItem.Icon)ITEM[Count - 5, 0]).Data = Icons.ID._0_Hit_ + _count;
            ((IGMDataItem.Texture)ITEM[Count - 4, 0]).Color = HitSlider.Update();

            var hotcnt = 0;
            var pos = 0;
            foreach (var i in ITEM)
            {
                if (i?.GetType() == typeof(IGMDataItem.Gradient.Renzokuken) && pos++ < _hits)
                {
                    var gr = (IGMDataItem.Gradient.Renzokuken)i;
                    done = !gr.Done || done;
                    hot = gr.Trigger || hot;
                    if (gr.Done)
                        hotcnt++;
                }
            }
            if (!done)
            {
                if ((delayMS += Memory.ElapsedGameTime) > TimeSpan.FromMilliseconds(1000))
                {
                    //Damageable.EndTurn(); //gets stuck if the current player isn't squall
                    Menu.BattleMenus.GetCurrentBattleMenu().Damageable.EndTurn();
                }
            }
            if (hot)
            {
                ((IGMDataItem.Icon)ITEM[Count - 3, 0]).Palette = 6;
                ((IGMDataItem.Icon)ITEM[Count - 2, 0]).Palette = 6;
                ((IGMDataItem.Texture)ITEM[0, 0]).Color = Renzokenken_Seperator_Color;
                ITEM[Count - 1, 0].Show();
                ITEM[Count - 5, 0].Hide();
                ITEM[Count - 6, 0].Hide();
            }
            else
            {
                ((IGMDataItem.Icon)ITEM[Count - 3, 0]).Palette = 2;
                ((IGMDataItem.Icon)ITEM[Count - 2, 0]).Palette = 2;
                ((IGMDataItem.Texture)ITEM[0, 0]).Color = Renzokenken_Seperator_Color_Faded;
                if ((hotcnt >= _hits) || !done)// && ITEM[Count - 1, 0].Enabled)
                {
                    if (_count >= _hits)
                        ITEM[Count - 6, 0].Show();
                    else if (_count > 0)
                        ITEM[Count - 5, 0].Show();
                }
                //else if (hotcnt > 0) Debug.WriteLine(hotcnt);

                ITEM[Count - 1, 0].Hide();
            }
            base.Update();
            return true;
        }

        protected override void Init()
        {
            base.Init();
            ThreadUnsafeOperations();

            Rectangle r, hotspot, tr;
            float scale;
            int w;
            int trigwidtharea, xbak;
            Renzokuken_Seperator_Prep(out r, out hotspot, out tr);
            Trigger_Prep(ref r, ref tr, out scale, out w, out trigwidtharea, out xbak);

            newattack = new Color(104, 80, 255);
            Rectangle pos = new Rectangle(r.X, r.Y + 4, Renzokuken_Gradient_Width, r.Height - 8);
            r.Inflate(-4, -4);
            for (int x = 0; x <= _hits && x <= 7; x++)
                ITEM[2 + x, 0] = IGMDataItem.Gradient.Renzokuken.Create(pos, newattack, Renzokenken_Seperator_Color, 1f, hotspot, r, time: RenzoTime, TimeSpan.FromTicks(RenzoDelay.Ticks * x));
            tr = _0_Hit_Prep(r, tr, out scale, out w, trigwidtharea, xbak);
            Perfect_Prep(r, tr, out scale, out w, trigwidtharea, xbak);

            Reset();
            Cursor_Status = Cursor_Status.Enabled | Cursor_Status.Static | Cursor_Status.Hidden;
        }

        private void Perfect_Prep(Rectangle r, Rectangle tr, out float scale, out int w, int trigwidtharea, int xbak)
        {
            var e = Memory.Icons[Icons.ID.Perfect__];
            scale = 1f;
            w = 0;
            if (e != null)
            {
                scale = ((float)r.Height) / e.Height;
                w = (int)(e.Width * scale);
            }
            tr.X = xbak + trigwidtharea / 2 - w / 2;
            ITEM[Count - 6, 0] = new IGMDataItem.Icon { Data = Icons.ID.Perfect__, Pos = tr, Palette = 8, Scale = new Vector2(scale) };
        }

        private Rectangle _0_Hit_Prep(Rectangle r, Rectangle tr, out float scale, out int w, int trigwidtharea, int xbak)
        {
            EntryGroup e;

            scale = 1f;
            w = 0;
            var range = (from i in Enumerable.Range(0, 8)
                                             where Memory.Icons[Icons.ID._0_Hit_ + (byte)i] != null &&
                                                     Memory.Icons[Icons.ID._0_Hit_ + (byte)i].Count > 0
                                             select Memory.Icons[Icons.ID._0_Hit_ + (byte)i]);
            float avgx = range.Count()>0?(int)range.Average(x => x[0].Offset.X):0;
            range.ForEach(x => x[0].Offset.X = avgx);
            //float totalx = 0;
            //for (byte i = 0; i <= 7; i++)
            //{
            //    e = Memory.Icons[Icons.ID._0_Hit_ + i];
            //    if(e!=null)
            //        totalx += e[0].Offset.X;
            //}
            //float avgx = (float)Math.Round(totalx / 8);
            //for (byte i = 0; i <= 7; i++)
            //{
            //    e = Memory.Icons[Icons.ID._0_Hit_ + i];
            //    if (e != null)
            //        e[0].Offset.X = avgx;
            //}
            e = Memory.Icons[Icons.ID._0_Hit_];
            if (e != null)
            {
                scale = ((float)r.Height) / e.Height;
                w = (int)(e.Width * scale);
            }
            tr.X = xbak + trigwidtharea / 2 - w / 2;
            ITEM[Count - 5, 0] = new IGMDataItem.Icon { Data = Icons.ID._0_Hit_, Pos = tr, Scale = new Vector2(scale) };
            return tr;
        }

        private void Trigger_Prep(ref Rectangle r, ref Rectangle tr, out float scale, out int w, out int trigwidtharea, out int xbak)
        {
            var e = Memory.Icons[Icons.ID.Trigger_];
            scale = 1f;
            w = 0;
            if (e != null)
            {
                e[0].Offset = Vector2.Zero;
                scale = ((float)r.Height - 8) / e.Height;
                w = (int)(e.Width * scale);
            }
            trigwidtharea = (r.Right - tr.Left);
            xbak = tr.X;
            tr.X += trigwidtharea / 2 - w / 2;

            ITEM[Count - 1, 0] = new IGMDataItem.Icon { Data = Icons.ID.Trigger_, Pos = tr, Palette = 6, Scale = new Vector2(scale) };
        }

        private void Renzokuken_Seperator_Prep(out Rectangle r, out Rectangle hotspot, out Rectangle tr)
        {
            var e = Memory.Icons[Icons.ID.Renzokuken_Seperator];
            var w = 0;
            var scale = 1f;
            r = CONTAINER.Pos;
            r.Inflate(-16, -20);
            r.X += r.X % 4;
            r.Y += r.Y % 4;
            r.Width += r.Width % 4;
            r.Height += r.Height % 4;
            if (e != null)
            {
                e[0].Offset = Vector2.Zero;
                scale = (float)r.Height / e.Height;
                w = (int)(e.Width * scale);
            }
            ITEM[0, 0] = new IGMDataItem.Texture { Data = pixel, Pos = r, Color = Renzokenken_Seperator_Color_Faded };
            r.Inflate(-4, -4);
            ITEM[1, 0] = new IGMDataItem.Texture { Data = pixel, Pos = r, Color = Color.Black };
            ITEM[Count - 3, 0] = new IGMDataItem.Icon { Data = Icons.ID.Renzokuken_Seperator, Pos = new Rectangle(r.X + 80, r.Y, w, r.Height), Scale = new Vector2(scale) };
            ITEM[Count - 2, 0] = new IGMDataItem.Icon { Data = Icons.ID.Renzokuken_Seperator, Pos = new Rectangle(r.X + 208, r.Y, w, r.Height), Scale = new Vector2(scale) };
            hotspot = new Rectangle(r.X + 80 + (w / 2), r.Y + 4, 208 - 80, r.Height - 8);
            ITEM[Count - 4, 0] = new IGMDataItem.Texture { Data = pixel, Pos = hotspot, Color = Color.TransparentBlack };
            hotspot.Width += (int)(hotspot.Width * .50f);
            tr = new Rectangle(r.X + 208 + (w / 2), r.Y + 4, 0, r.Height - 4);
        }

        protected override void RefreshChild()
        {
            var pos = 0;
            foreach (var i in ITEM)
            {
                if (i.GetType() == typeof(IGMDataItem.Gradient.Renzokuken))
                {
                    var rg = (IGMDataItem.Gradient.Renzokuken)i;
                    if (pos++ < _hits)
                        rg.Show();
                    else
                        rg.Hide();
                }
            }
            base.RefreshChild();
        }

        #endregion Methods
    }
}

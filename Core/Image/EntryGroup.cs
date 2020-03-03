using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace OpenVIII
{
    public class EntryGroup : IEnumerator, IEnumerable
    {
        private const int minx = -1000;
        private const int miny = -360;

        #region Fields

        private List<Entry> list;
        private Rectangle rectangle;

        public bool Trimmed { get; set; } = false;

        public EntryGroup(int capacity = 1)
        {
            list = new List<Entry>(capacity);
            rectangle = new Rectangle();
        }

        public EntryGroup(params Entry[] entries)
        {
            list = new List<Entry>(entries.Length);
            rectangle = new Rectangle();
            Add(entries);
        }

        #endregion Fields

        #region Properties

        public int Count => list.Count;
        //public int Height { get => Rectangle.Height; private set => Rectangle = new Rectangle(Rectangle.X, Rectangle.Y, value, Rectangle.Height); }

        //public int Width { get => Rectangle.Width; private set => Rectangle = new Rectangle(Rectangle.X,Rectangle.Y, value,Rectangle.Width); }
        public int Height { get => rectangle.Height; set => rectangle.Height = value; }

        public int Width { get => rectangle.Width; set => rectangle.Width = value; }

        public Rectangle GetRectangle => rectangle;

        public object Current => list[position - 1];

        #endregion Properties

        #region Indexers

        /// <summary>
        /// donno if this works for assigning. perfer add.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Entry this[int id] => list[id]; /*set { if (list.Count - 1 < id) Add(value); else list[id] = value; }*/

        #endregion Indexers

        #region Methods

        private Vector2 nag_Offset = new Vector2();
        private Vector2 pos_Offset = new Vector2();

        public void Add(params Entry[] entries)
        {
            foreach (Entry entry in entries)
            {
                //TODO fix math
                if (list.Count >= 1)
                {
                    //DiscFix
                    if (entry.Height < float.Epsilon)
                    {
                        entry.Height = Height; // one item had a missing height.
                        if (Math.Abs(nag_Offset.X) + pos_Offset.X + entry.Offset.X < float.Epsilon) //assumes if the items are overlapping put them next to each other instead.
                            entry.Offset.X = (short)Width;
                    }
                    //BarFix
                    //has 2 sides and no filling.
                    //if(entry.CurrentPos == 1872)
                    if (list.Capacity == 2 && list.Count == 1 && (list[0].Width * 2) + list[0].X == entry.X && list[0].Y == entry.Y)
                        Add(new Entry
                        {
                            X = entry.X - list[0].Width,
                            Y = entry.Y,
                            Size = list[0].Size,
                            Tile = Vector2.UnitX,
                            Offset = new Vector2((int)list[0].Width, 0),
                            End = new Vector2((int)-list[0].Width, 0)
                        });
                }
                list.Add(entry);
                Vector2 size = Abs(entry.Offset) + entry.Size;
                if (Width < size.X) Width = (int)size.X;
                if (Height < size.Y) Height = (int)size.Y;
            }
        }

        private object locker = new object();
        private ConcurrentDictionary<int, Color> Average;

        public Color MostSaturated(TextureHandler tex, int palette = 0)
        {
            if (Count >= 1)
            {
                lock (locker)
                {
                    if (Average == null)
                        Average = new ConcurrentDictionary<int, Color>();
                }
                if (!(Average.TryGetValue(palette, out Color ret)))
                {
                    Rectangle r = this[0].GetRectangle;
                    for (int i = 1; i < Count; i++)
                        r = Rectangle.Union(r, this[i].GetRectangle);
                    r = r.Scale(tex.ScaleFactor);
                    Texture2D t = (Texture2D)tex;
                    if (t == null) return default;
                    Color[] tc = new Color[r.Width * r.Height];
                    t.GetData(0, r, tc, 0, tc.Length);
                    HSL test, last;
                    last.S = 0;
                    last.L = 0;
                    ret = Color.TransparentBlack;

                    foreach (Color p in tc)
                    {
                        if (p.A >= 250)
                        {
                            test = p;
                            if (ret == Color.TransparentBlack)
                                last = test;
                            if (test.S > .25f && last.S <= test.S)
                            {
                                if (ret == Color.TransparentBlack)
                                    ret = p;
                                else
                                {
                                    ret = Color.Lerp(ret, p, .5f);
                                    last = test;
                                }
                            }
                            else if (test.L > .25f && last.L <= test.L)
                            {
                                if (ret == Color.TransparentBlack)
                                    ret = p;
                                else
                                {
                                    ret = Color.Lerp(ret, p, .5f);
                                    last = test;
                                }
                            }
                        }
                    }
                    if (!Average.TryAdd(palette, ret))
                        throw new Exception("failed to cache color");
                }
                return ret;
            }
            throw new Exception("Count is <1");
        }

        public void Trim(TextureHandler tex)
        {
            if (!Trimmed)
            {
                if (Count >= 1)
                {
                    Width = 0;
                    Height = 0;
                    Trimmed = true;
                    Vector2 offset = new Vector2(float.MaxValue);
                    if (Count == 1)
                        this[0].Offset = Vector2.Zero;
                    for (int i = 0; i < Count; i++)
                    {
                        Rectangle ret = tex.Trim(this[i].GetRectangle);
                        this[i].SetTrim_1stPass(ret, Count == 1 ? true : false);
                        if (Count > 1)
                        {
                            if (this[i].Offset.X < offset.X) offset.X = this[i].Offset.X;
                            if (this[i].Offset.Y < offset.Y) offset.Y = this[i].Offset.Y;
                        }
                    }
                    for (int i = 0; i < Count; i++)
                    {
                        if (offset != Vector2.Zero && Count > 1)
                            this[i].SetTrim_2ndPass(offset);
                        Point size = new Point((int)(this[i].Width + Math.Abs(this[i].Offset.X)), (int)(this[i].Height + Math.Abs(this[i].Offset.Y)));

                        if (Width < size.X) Width = size.X;
                        if (Height < size.Y) Height = size.Y;
                    }
                }
            }
        }

        public static Vector2 Abs(Vector2 v2) => new Vector2(Math.Abs(v2.X), Math.Abs(v2.Y));

        public static Point RoundedPoint(Vector2 v) => v.RoundedPoint();

        public void Draw(List<TextureHandler> textures, int palette, Rectangle inputdst, Vector2 inscale, float fade = 1f, Color? color = null) =>
            Draw(textures, list, palette, inputdst, inscale, fade, new Point(Width, Height), color);

        public static int GetChange(int tot, int goal, float scale = 1f) => (int)Math.Round(Math.Abs(tot * scale - goal));

        public static void Draw(List<TextureHandler> textures, List<Entry> elist, int palette, Rectangle inputdst, Vector2 inscale, float fade, Point totalSize, Color? color_ = null)
        {
            Color color = color_ ?? Color.White;
            Rectangle dst;
            inscale = Abs(inscale);
            inputdst.Width = Math.Abs(inputdst.Width);
            inputdst.Height = Math.Abs(inputdst.Height);
            if (inputdst.Right < minx || inputdst.Bottom < miny)
                return;

            Vector2 autoscale = new Vector2((float)inputdst.Width / totalSize.X, (float)inputdst.Height / totalSize.Y);
            Vector2 scale;
            if (inscale == Vector2.Zero || inscale == Vector2.UnitX)
                scale = new Vector2(autoscale.X);
#pragma warning disable IDE0045 // Convert to conditional expression
            else if (inscale == Vector2.UnitY)
#pragma warning restore IDE0045 // Convert to conditional expression
                scale = new Vector2(autoscale.Y);
            else
                scale = inscale;
            foreach (Entry e in elist)
            {
                if (totalSize == new Point(0))
                {
                    totalSize.X = (int)e.Width;
                    totalSize.Y = (int)e.Height;
                }
                int cpalette = e.CustomPalette < 0 || e.CustomPalette >= textures.Count ? palette : e.CustomPalette;
                dst = inputdst;

                Vector2 Offset = e.Offset * scale;
                Point offset2 = RoundedPoint(e.End * scale);
                dst.Offset(e.Snap_Right ? inputdst.Width : 0, e.Snap_Bottom ? inputdst.Height : 0);
                dst.Offset(RoundedPoint(Offset));
                dst.Size = RoundedPoint(e.Size * scale);
                Rectangle src = e.GetRectangle;
                bool testY = false;
                bool testX = false;
                CorrectX(inputdst, totalSize, ref dst, autoscale, ref scale, e, ref src);
                CorrectY(inputdst, totalSize, ref dst, autoscale, ref scale, e, ref src);

                if (dst.Width <= 0 || dst.Height <= 0) continue; //infinate loop prevention

                if (e.Tile != Vector2.Zero)
                {
                    do
                    {
                        do
                        {
                            Point correction = Correction(inputdst, dst, offset2);
                            testY = testYfunct(dst, inputdst, offset2);
                            testX = testXfunct(dst, inputdst, offset2);
                            TileBounds(correction, ref dst, scale, e, ref src, ref testY, ref testX);
                            textures[cpalette].Draw(dst, src, color * fade);
                            dst = TileShift(dst, e);
                        }
                        while (e.Tile.Y > 0 && testY);
                    }
                    while (e.Tile.X > 0 && testX);
                }
                else
                    textures[cpalette].Draw(dst, src, color * fade);
            }
        }

        private static void CorrectY(Rectangle inputdst, Point totalSize, ref Rectangle dst, Vector2 autoscale, ref Vector2 scale, Entry e, ref Rectangle src)
        {
            if (inputdst.Height != 0 && dst.Bottom > inputdst.Bottom)
            {
                int change = GetChange(totalSize.Y, inputdst.Height, scale.Y); ;
                src.Height -= (int)Math.Round(change / scale.Y);
                dst.Height -= change;
            }
            else if (e.Fill.Y > 0 && autoscale.Y > scale.Y)
            {
                scale.Y = autoscale.Y;
                dst.Height = (int)Math.Ceiling((src.Height * autoscale.Y + 0.5f));
            }
        }

        private static void CorrectX(Rectangle inputdst, Point totalSize, ref Rectangle dst, Vector2 autoscale, ref Vector2 scale, Entry e, ref Rectangle src)
        {
            if ((inputdst.Width != 0) && dst.Right > inputdst.Right)
            {
                int change = GetChange(totalSize.X, inputdst.Width, scale.X);
                src.Width -= (int)Math.Round(change / scale.X);
                dst.Width -= change;
            }
            else if (e.Fill.X > 0 && autoscale.X > scale.X)
            {
                scale.X = autoscale.X;
                dst.Width = (int)Math.Ceiling((src.Width * autoscale.X + 0.5f));
            }
        }

        private static void TileBounds(Point correction, ref Rectangle dst, Vector2 scale, Entry e, ref Rectangle src, ref bool testY, ref bool testX)
        {
            if (e.Tile.Y > 0 && !testY)
            {
                dst.Height += correction.Y;
                src.Height += (int)Math.Round(correction.Y / scale.Y);
            }
            if (e.Tile.X > 0 && !testX)
            {
                dst.Width += correction.X;
                src.Width += (int)Math.Round(correction.X / scale.X);
            }
        }

        private static bool testXfunct(Rectangle dst, Rectangle inputdst, Point offset2) => (dst.Right) < (inputdst.Right + offset2.X);

        private static bool testYfunct(Rectangle dst, Rectangle inputdst, Point offset2) => (dst.Bottom) < (inputdst.Bottom + offset2.Y);

        private static Point Correction(Rectangle inputdst, Rectangle dst, Point offset2) => new Point((inputdst.Right + offset2.X) - (dst.Right), (inputdst.Bottom + offset2.Y) - (dst.Bottom));

        private static Rectangle TileShift(Rectangle dst, Entry e)
        {
            if (e.Tile.Y > 0)
            {
                dst.Y += dst.Height;
            }
            if (e.Tile.X > 0)
            {
                dst.X += dst.Width;
            }

            return dst;
        }

        private int position = 0;

        public bool MoveNext() => ++position <= list.Count;

        public void Reset() => position = 0;

        public IEnumerator GetEnumerator() => this;

        #endregion Methods
    }
}
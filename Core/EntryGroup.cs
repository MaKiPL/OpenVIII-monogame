using Microsoft.Xna.Framework;
using System;
using System.Collections;
using System.Collections.Generic;

namespace OpenVIII
{
    public class EntryGroup : IEnumerator, IEnumerable
    {
        #region Fields

        private List<Entry> list;
        private Rectangle rectangle;

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
        public int Height { get => rectangle.Height; private set => rectangle.Height = value; }

        public int Width { get => rectangle.Width; private set => rectangle.Width = value; }

        public Rectangle GetRectangle => rectangle;

        public object Current
        {
            get { return list[position-1]; }
        }

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

        public static Vector2 Abs(Vector2 v2) => new Vector2(Math.Abs(v2.X), Math.Abs(v2.Y));

        public static Point RoundedPoint(Vector2 v) => v.RoundedPoint();

        public void Draw(List<TextureHandler> textures, int palette, Rectangle inputdst, Vector2 inscale, float fade = 1f) =>
            Draw(textures, list, palette, inputdst, inscale, fade, new Point(Width, Height));
        public static int GetChange(int tot,int goal, float scale = 1f)
        {
           return (int) Math.Round(Math.Abs(tot* scale - goal));
        }
        public static void Draw(List<TextureHandler> textures,List<Entry> elist, int palette, Rectangle inputdst, Vector2 inscale, float fade, Point totalSize)
        {
            Rectangle dst;
            inscale = Abs(inscale);
            inputdst.Width = Math.Abs(inputdst.Width);
            inputdst.Height = Math.Abs(inputdst.Height);
            if (inputdst.X + inputdst.Width < 0 || inputdst.Y + inputdst.Height < 0) return;

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
                if ((inputdst.Width!=0) && dst.X + dst.Width > inputdst.X + inputdst.Width)
                {
                    int change = GetChange(totalSize.X,inputdst.Width,scale.X);
                    src.Width -= (int)Math.Round(change / scale.X);
                    dst.Width -= change;
                }
                else if (e.Fill.X > 0 && autoscale.X > scale.X)
                {
                    scale.X = autoscale.X;
                    dst.Width = (int)Math.Ceiling((src.Width * autoscale.X+0.5f));
                }
                
                if ((inputdst.Height != 0) && dst.Y + dst.Height > inputdst.Y + inputdst.Height)
                {
                    int change = GetChange(totalSize.Y, inputdst.Height, scale.Y); ;
                    src.Height -= (int)Math.Round(change / scale.Y);
                    dst.Height -= change;
                }
                else if (e.Fill.Y > 0 && autoscale.Y > scale.Y)
                {
                    scale.Y = autoscale.Y;
                    dst.Height = (int)Math.Ceiling((src.Height * autoscale.Y+0.5f));
                 
                }

                if (dst.Height <= 0 || dst.Height <= 0) continue; //infinate loop prevention

                do
                {
                    do
                    {
                        if (e.Tile.Y > 0)
                        {
                            testY = (dst.Y + dst.Height) < (inputdst.Y + inputdst.Height + offset2.Y);
                            if (!testY)
                            {
                                int correction = (inputdst.Y + inputdst.Height + offset2.Y) - (dst.Y + dst.Height);
                                dst.Height += correction;
                                src.Height += (int)Math.Round(correction / scale.Y);
                            }
                        }
                        if (e.Tile.X > 0)
                        {
                            testX = (dst.X + dst.Width) < (inputdst.X + inputdst.Width + offset2.X);
                            if (!testX)
                            {
                                int correction = (inputdst.X + inputdst.Width + offset2.X) - (dst.X + dst.Width);
                                dst.Width += correction;
                                src.Width += (int)Math.Round(correction / scale.X);
                            }
                        }
                        textures[cpalette].Draw(dst, src, Color.White * fade);
                        if (e.Tile.Y > 0)
                        {
                            dst.Y += dst.Height;
                        }
                        if (e.Tile.X > 0)
                        {
                            dst.X += dst.Width;
                        }
                    }
                    while (e.Tile.Y > 0 && testY);
                }
                while (e.Tile.X > 0 && testX);
            }
        }
        int position = 0;
        public bool MoveNext() => ++position<=list.Count;
        public void Reset() => position = 0;
        public IEnumerator GetEnumerator() => this;

        #endregion Methods
    }
}
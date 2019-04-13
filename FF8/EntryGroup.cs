using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace FF8
{
    public class EntryGroup
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

        public int Height { get => rectangle.Height; private set => rectangle.Height = value; }

        public int Width { get => rectangle.Width; private set => rectangle.Width = value; }

        public Rectangle GetRectangle => rectangle;

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
                            Offset = new Vector2((int)list[0].Width,0),
                            End = new Vector2((int)-list[0].Width,0)
                        });
                }
                list.Add(entry);
                Vector2 size = Abs(entry.Offset) + entry.Size;
                if (Width < size.X) Width = (int)size.X;
                if (Height < size.Y) Height = (int) size.Y;
            }
        }
        internal Vector2 Abs(Vector2 v2  )  {
        return new Vector2(Math.Abs(v2.X), Math.Abs(v2.Y));
        }

    internal void Draw(Texture2D[] textures, int pallet, Rectangle inputdst, float scale = 1f, float fade = 1f)
        {
            Rectangle dst;
            scale = Math.Abs(scale);
            inputdst.Width = Math.Abs(inputdst.Width);
            inputdst.Height = Math.Abs(inputdst.Height);
            if (inputdst.X + inputdst.Width < 0 || inputdst.Y + inputdst.Height < 0) return;
            if ((int)(8 * scale) <= float.Epsilon)
            {
                //vscale = (float)dst.Height / Height;
                scale = (float)inputdst.Width /Width;
            }

            foreach (Entry e in list)
            {
                int cpallet = e.CustomPallet < 0 || e.CustomPallet >= textures.Length ? pallet : e.CustomPallet;
                dst = inputdst;


                Vector2 Offset = e.Offset * scale;
                Point offset2 = (e.End * scale).ToPoint();
                dst.Offset(e.Snap_Right ? inputdst.Width : 0, e.Snap_Bottom ? inputdst.Height : 0);
                dst.Offset(Offset);
                dst.Size = (e.Size * scale).ToPoint();
               
                Rectangle src = e.GetRectangle;
                bool testY = false;
                bool testX = false;
                if (dst.Height<=0 || dst.Height <= 0) continue; //infinate loop prevention
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
                                src.Height += (int)Math.Floor(correction / scale);
                            }
                        }
                        if(e.Tile.X > 0)
                        { 
                            testX = (dst.X + dst.Width) < (inputdst.X + inputdst.Width + offset2.X);
                            if (!testX)
                            {
                                int correction = (inputdst.X + inputdst.Width + offset2.X) - (dst.X + dst.Width);
                                dst.Width += correction;
                                src.Width += (int)Math.Floor(correction / scale);
                            }
                        }
                        Memory.spriteBatch.Draw(textures[cpallet], dst, src, Color.White * fade);
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
                //Memory.spriteBatch.Draw(icons[pallet], Vector2.Zero, e.GetRectangle, Color.White*fade,0f,Vector2.Zero,Vector2.UnitX,SpriteEffects.None,0f);
            }
            //Memory.SpriteBatchStartStencil(SamplerState.PointClamp);
            //Memory.SpriteBatchEnd();
        }

        #endregion Methods
    }
}
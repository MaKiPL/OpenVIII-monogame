using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;

namespace FF8
{
    public class Entry //Rectangle + File
    {
        #region Fields

        public byte[] UNK;
        private Loc loc;
        private Point offset;
        private Rectangle src;

        #endregion Fields

        #region Constructors

        public Entry()
        {
            UNK = new byte[2];
            File = 0;
            Part = 1;
        }

        #endregion Constructors

        #region Properties

        public ushort CurrentPos { get; set; }
        public byte File { get; set; }
        public byte Height { get => (byte)src.Height; set => src.Height = value; }

        /// <summary>
        /// when you draw shift by X
        /// </summary>
        public short Offset_X { get => (short)offset.X; set => offset.X = value; }

        /// <summary>
        /// when you draw shift by Y
        /// </summary>
        public sbyte Offset_Y { get => (sbyte)offset.Y; set => offset.Y = value; }

        public byte Part { get; set; } = 1;
        public ushort Width { get => (ushort)src.Width; set => src.Width = value; }
        public byte X { get => (byte)src.X; set => src.X = value; }
        public byte Y { get => (byte)src.Y; set => src.Y = value; }

        #endregion Properties

        #region Methods

        public Loc GetLoc => loc; public Rectangle GetRectangle => src;

        public sbyte CustomPallet { get; internal set; } = -1;
        /// <summary>
        /// Vector2.Zero = no tile, Vector2.One = tile x & y, Vector.UnitX = tile x, Vector.UnitY = tile y
        /// </summary>
        public Vector2 Tile { get; internal set; } = Vector2.Zero;
        public bool Snap_Right { get; internal set; } = false;

        /// <summary>
        /// point where you want to stop drawing from right side so -8 would stop 8*scale pixels from edge
        /// </summary>
        public sbyte Offset_X2 { get; internal set; }

        /// <summary>
        /// point where you want to stop drawing from bottom side so -8 would stop 8*scale pixels
        /// from edge
        /// </summary>
        public sbyte Offset_Y2 { get; internal set; }

        public bool Snap_Bottom { get; internal set; } = false;

        public byte LoadfromStreamSP2(BinaryReader br, UInt16 loc = 0, byte prevY = 0, byte fid = 0)
        {
            if (loc > 0)
                br.BaseStream.Seek(loc + 4, SeekOrigin.Begin);
            CurrentPos = loc;
            X = br.ReadByte();
            Y = br.ReadByte();
            UNK[0] = br.ReadByte();
            UNK[1] = br.ReadByte();
            //br.BaseStream.Seek(2, SeekOrigin.Current);
            Width = br.ReadByte();
            Offset_X = br.ReadSByte();
            //br.BaseStream.Seek(1, SeekOrigin.Current);
            Height = br.ReadByte();
            Offset_Y = br.ReadSByte();
            //br.BaseStream.Seek(1, SeekOrigin.Current);
            if (prevY > 0 && Y < prevY)
                fid++;
            File = fid;
            return fid;
        }

        public void SetLoc(Loc value) => loc = value;

        internal void LoadfromStreamSP1(BinaryReader br)
        {
            CurrentPos = (ushort)br.BaseStream.Position;
            X = br.ReadByte();
            Y = br.ReadByte();

            UNK = br.ReadBytes(2);
            Width = br.ReadByte();
            Offset_X = br.ReadSByte();
            Height = br.ReadByte();
            Offset_Y = br.ReadSByte();
        }

        #endregion Methods
    }

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

        private Point nag_offset = new Point();
        private Point pos_offset = new Point();

        public void Add(params Entry[] entries)
        {
            foreach (Entry entry in entries)
            {
                //TODO fix math
                if (list.Count >= 1)
                {
                    //DiscFix
                    if (entry.Height == 0)
                    {
                        entry.Height = (byte)Height; // one item had a missing height.
                        if (Math.Abs(nag_offset.X) + pos_offset.X + entry.Offset_X == 0) //assumes if the items are overlapping put them next to each other instead.
                            entry.Offset_X = (short)Width;
                    }
                    //BarFix
                    //has 2 sides and no filling.
                    //if(entry.CurrentPos == 1872)
                    if (list.Capacity == 2 && list.Count == 1 && (list[0].Width * 2) + list[0].X == entry.X && list[0].Y == entry.Y)
                        Add(new Entry
                        {
                            X = (byte)(entry.X - list[0].Width),
                            Y = entry.Y,
                            Width = list[0].Width,
                            Height = list[0].Height,
                            Tile = Vector2.UnitX,
                            Offset_X = (short)list[0].Width,
                            Offset_X2 = (sbyte)-list[0].Width
                        });
                }
                list.Add(entry);
                ushort width = (ushort)(Math.Abs(entry.Offset_X) + entry.Width);
                ushort height = (ushort)(Math.Abs(entry.Offset_Y) + entry.Height);
                if (Width < width) Width = width;
                if (Height < height) Height = height;
            }
        }

        internal void Draw(Texture2D[] textures, int pallet, Rectangle inputdst, float scale = 1f, float fade = 1f)
        {
            Rectangle dst;
            if ((int)(8 * scale) <= 0)
            {
                //vscale = (float)dst.Height / Height;
                scale = (float)inputdst.Width / Width;
            }

            foreach (Entry e in list)
            {
                int cpallet = e.CustomPallet < 0 || e.CustomPallet >= textures.Length ? pallet : e.CustomPallet;
                dst = inputdst;

                Point offset = new Point((int)(e.Offset_X * scale), (int)(e.Offset_Y * scale));
                Point offset2 = new Point((int)(e.Offset_X2 * scale), (int)(e.Offset_Y2 * scale));
                dst.X += (e.Snap_Right ? inputdst.Width : 0) + offset.X;
                dst.Y += (e.Snap_Bottom ? inputdst.Height : 0) + offset.Y;
                dst.Width = (int)(e.Width * scale);
                dst.Height = (int)(e.Height * scale);
                Rectangle src = e.GetRectangle;
                bool testY = false;
                bool testX = false;
                //if (dst.Height<=0 || dst.Height <= 0) continue; //infinate loop prevention
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
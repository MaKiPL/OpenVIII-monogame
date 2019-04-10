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

        #endregion Fields

        #region Properties

        public int Height => rectangle.Height + Math.Abs(nag_offset.Y) + pos_offset.Y;

        public int Offset_X => 0;//nag_offset.X == 0 ? pos_offset.X : nag_offset.X;

        public int Offset_Y => 0;//nag_offset.Y == 0 ? pos_offset.Y : nag_offset.Y;

        public int Width => rectangle.Width + Math.Abs(nag_offset.X) + pos_offset.X;

        public Rectangle GetRectangle => rectangle;

        #endregion Properties

        #region Indexers

        /// <summary>
        /// donno if this works for assigning. perfer add.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Entry this[int id] => list[id]; /*set { if (list.Count - 1 < id) Add(value); else list[id] = value; }*/

        private Entry fillTexture = null;

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
                    if (list.Capacity == 2 && list.Count == 1 && 2 * Width + list[0].X == entry.X && list[0].Y == entry.Y)
                        fillTexture = new Entry { X = (byte)(entry.X - Width), Y = entry.Y, Width = (ushort)Width, Height = (byte)Height, Offset_X = (short)Width };
                }
                list.Add(entry);
                //if (rectangle.X > entry.Offset_X)
                //    rectangle.X = entry.Offset_X;
                Rectangle src = entry.GetRectangle;
                src.X += entry.Offset_X;
                src.Y += entry.Offset_Y;
                
                bool contain = rectangle.Contains(src);
                if (Width == 0 || /*Width > entry.Width &&*/ !contain)
                    rectangle.Width = entry.Width;
                if (entry.Offset_X < nag_offset.X)
                    nag_offset.X = entry.Offset_X;
                if (pos_offset.X == 0 && entry.Offset_X > pos_offset.X)
                    pos_offset.X = entry.Offset_X;
                //if (rectangle.Y > entry.Offset_Y)
                //    rectangle.Y = entry.Offset_Y;
                if (Height == 0 || /*Height < entry.Height && */ !contain)
                    rectangle.Height = entry.Height;
                if (entry.Offset_Y < nag_offset.Y)
                    nag_offset.Y = entry.Offset_Y;
                if (pos_offset.Y == 0 || entry.Offset_Y > pos_offset.Y)
                    pos_offset.Y = entry.Offset_Y;

                //rectangle.Height += entry.Height - (entry.Height + entry.Offset_Y);
            }
        }

        internal void Draw(Texture2D[] textures, int pallet, Rectangle inputdst, float scale = 1f, float fade = 1f)
        {
            //Viewport vp = Memory.graphics.GraphicsDevice.Viewport;

            Rectangle dst;
            Memory.SpriteBatchStartAlpha(SamplerState.PointClamp);

            if (fillTexture != null)
            {
                dst = inputdst;
                dst.X += (int)(fillTexture.Offset_X * scale);
                dst.Y += (int)(fillTexture.Offset_Y * scale);
                dst.Width = (int)((pos_offset.X - fillTexture.Width) * scale);
                Memory.spriteBatch.Draw(textures[pallet], dst, fillTexture.GetRectangle, Color.White * fade);
            }
            foreach (Entry e in list)
            {
                int cpallet = e.CustomPallet < 0 || e.CustomPallet >= textures.Length ? pallet : e.CustomPallet;
                dst = inputdst;
                float vscale = scale;
                float hscale = scale;
                if (scale == 0)
                {
                    //vscale = (float)dst.Height / Height;
                    vscale = hscale = (float)dst.Width / Width;
                }
                dst.X += (int)(e.Offset_X * hscale);
                dst.Y += (int)(e.Offset_Y * vscale);
                dst.Width = (int)(e.Width * hscale);
                dst.Height = (int)(e.Height * vscale);
                do
                {
                    //dst.X += (int)(e.Width * scale);
                    //dst.X += (int)(e.X * scale);
                    Memory.spriteBatch.Draw(textures[cpallet], dst, e.GetRectangle, Color.White * fade);
                    dst.Y += dst.Height;
                }
                while (dst.Y < inputdst.Height);
                //Memory.spriteBatch.Draw(icons[pallet], Vector2.Zero, e.GetRectangle, Color.White*fade,0f,Vector2.Zero,Vector2.UnitX,SpriteEffects.None,0f);
            }
            Memory.SpriteBatchEnd();
            //Memory.SpriteBatchStartStencil(SamplerState.PointClamp);
            //Memory.SpriteBatchEnd();
        }

        #endregion Methods
    }
}
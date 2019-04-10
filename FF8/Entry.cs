using Microsoft.Xna.Framework;
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
        public sbyte Offset_X { get => (sbyte)offset.X; set => offset.X = value; }

        /// <summary>
        /// when you draw shift by Y
        /// </summary>
        public sbyte Offset_Y { get => (sbyte)offset.Y; set => offset.Y = value; }

        public byte Part { get; set; } = 1;
        public byte Width { get => (byte)src.Width; set => src.Width = value; }
        public byte X { get => (byte)src.X; set => src.X = value; }
        public byte Y { get => (byte)src.Y; set => src.Y = value; }

        #endregion Properties

        #region Methods

        public Loc GetLoc => loc; public Rectangle GetRectangle => src;

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

        public int Height => rectangle.Height;

        public int Offset_X => rectangle.X;

        public int Offset_Y => rectangle.Y;

        public int Width => rectangle.Width;

        public Rectangle GetRectangle => rectangle;

        #endregion Properties

        #region Indexers
        /// <summary>
        /// donno if this works for assigning. perfer add.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Entry this[int id] { get => list[id]; /*set { if (list.Count - 1 < id) Add(value); else list[id] = value; }*/ }

        #endregion Indexers

        #region Methods

        public void Add(Entry entry)
        {
            //TODO fix math
            list.Add(entry);
            if (rectangle.X > entry.Offset_X)
                rectangle.X = entry.Offset_X;
            rectangle.Width += entry.Width - (entry.Width + entry.Offset_X);
            if (rectangle.Y > entry.Offset_Y)
                rectangle.Y = entry.Offset_Y;
            rectangle.Height += entry.Height - (entry.Height + entry.Offset_Y);
        }

        #endregion Methods
    }
}
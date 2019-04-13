using Microsoft.Xna.Framework;
using System;
using System.IO;

namespace FF8
{
    public class Entry //Rectangle + File
    {

        #region Fields

        public byte[] UNK;
        private Loc loc;
        private Rectangle src;
        public Point Offset;
        public Point End;

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
        public sbyte CustomPallet { get; internal set; } = -1;
        public byte File { get; set; }
        //public Loc GetLoc => loc;
        public Rectangle GetRectangle => src;
        public Point Size => src.Size;
        public Point Location => src.Location;

        /// <summary>
        /// point where you want to stop drawing from right side so -8 would stop 8*scale pixels from edge
        /// </summary>
        //public sbyte Offset_X2 { get; internal set; }

        /// <summary>
        /// point where you want to stop drawing from bottom side so -8 would stop 8*scale pixels
        /// from edge
        /// </summary>
        //public sbyte Offset_Y2 { get; internal set; }

        public byte Part { get; set; } = 1;
        public bool Snap_Bottom { get; internal set; } = false;
        public bool Snap_Right { get; internal set; } = false;
        /// <summary>
        /// Vector2.Zero = no tile, Vector2.One = tile x & y, Vector.UnitX = tile x, Vector.UnitY = tile y
        /// </summary>
        public Vector2 Tile { get; internal set; } = Vector2.Zero;
        public int Height { get =>src.Height; internal set=> src.Height=value; }
        public int Width { get=>src.Width; internal set=> src.Width=value; }
        public int Y { get => src.Y; internal set => src.Y = value; }
        public int X { get => src.X; internal set => src.X = value; }

        #endregion Properties

        #region Methods

        public byte LoadfromStreamSP2(BinaryReader br, UInt16 loc = 0, byte prevY = 0, byte fid = 0)
        {
            if (loc > 0)
                br.BaseStream.Seek(loc + 4, SeekOrigin.Begin);
            CurrentPos = loc;
            src.X = br.ReadByte();
            src.Y = br.ReadByte();
            UNK = br.ReadBytes(2);
            src.Width = br.ReadByte();
            Offset.X = br.ReadSByte();
            src.Height = br.ReadByte();
            Offset.Y = br.ReadSByte();
            if (prevY > 0 && src.Y < prevY)
                fid++;
            File = fid;
            return fid;
        }

        public void SetLoc(Loc value) => loc = value;

        internal void LoadfromStreamSP1(BinaryReader br)
        {
            CurrentPos = (ushort)br.BaseStream.Position;
            src.X = br.ReadByte();
            src.Y = br.ReadByte();

            UNK = br.ReadBytes(2);
            src.Width = br.ReadByte();
            Offset.X = br.ReadSByte();
            src.Height = br.ReadByte();
            Offset.Y = br.ReadSByte();
        }

        #endregion Methods

    }
}
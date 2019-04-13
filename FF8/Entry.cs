using Microsoft.Xna.Framework;
using System;
using System.IO;

namespace FF8
{
    public class Entry //Rectangle + File
    {

        #region Fields

        public byte[] UNK;
        public Vector2 Location;
        public Vector2 Size;
        private Loc loc;
        public Vector2 Offset;
        public Vector2 End;

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
        public Rectangle GetRectangle => new Rectangle(Location.ToPoint(),Size.ToPoint());

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
        public float Height { get =>Size.Y; internal set=> Size.Y=value; }
        public float Width { get=>Size.Y; internal set=> Size.X=value; }
        public float Y { get => Location.Y; internal set => Location.Y = value; }
        public float X { get => Location.X; internal set => Location.X = value; }

        #endregion Properties

        #region Methods

        public byte LoadfromStreamSP2(BinaryReader br, UInt16 loc = 0, byte prevY = 0, byte fid = 0)
        {
            if (loc > 0)
                br.BaseStream.Seek(loc + 4, SeekOrigin.Begin);
            CurrentPos = loc;
            Location.X = br.ReadByte();
            Location.Y = br.ReadByte();
            UNK = br.ReadBytes(2);
            Size.X = br.ReadByte();
            Offset.X = br.ReadSByte();
            Size.Y = br.ReadByte();
            Offset.Y = br.ReadSByte();
            if (prevY > 0 && Location.Y < prevY)
                fid++;
            File = fid;
            return fid;
        }

        public void SetLoc(Loc value) => loc = value;

        internal void LoadfromStreamSP1(BinaryReader br)
        {
            CurrentPos = (ushort)br.BaseStream.Position;
            Location.X = br.ReadByte();
            Location.Y = br.ReadByte();

            UNK = br.ReadBytes(2);
            Size.X = br.ReadByte();
            Offset.X = br.ReadSByte();
            Size.Y = br.ReadByte();
            Offset.Y = br.ReadSByte();
        }

        #endregion Methods

    }
}
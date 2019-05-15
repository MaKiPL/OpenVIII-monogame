using Microsoft.Xna.Framework;
using System;
using System.IO;

namespace FF8
{
    internal class Entry //Rectangle + File
    {

        #region Fields

        internal byte[] UNK;
        internal Vector2 Location;
        internal Vector2 Size;
        private Loc loc;
        internal Vector2 Offset;
        internal Vector2 End;
        #endregion Fields

        internal string ToStringHeader = "{File},{Part},{CustomPallet},{Location.X},{Location.Y},{Size.X},{Size.Y},{Offset.X},{Offset.Y},{End.X},{End.Y},{Tile.X},{Tile.Y},{Fill.X},{Fill.Y},{Snap_Right},{Snap_Bottom}\n";
        public override string ToString()
        {
            
            return $"{File},{Part},{CustomPallet},{Location.X},{Location.Y},{Size.X},{Size.Y},{Offset.X},{Offset.Y},{End.X},{End.Y},{Tile.X},{Tile.Y},{Fill.X},{Fill.Y},{Snap_Right},{Snap_Bottom}\n";
        }

        #region Constructors

        internal Entry()
        {
            UNK = new byte[2];
            File = 0;
            Part = 1;
        }

        #endregion Constructors

        #region Properties

        internal ushort CurrentPos { get; set; }
        internal sbyte CustomPallet { get; set; } = -1;
        internal byte File { get; set; }
        //internal Loc GetLoc => loc;
        internal Rectangle GetRectangle => new Rectangle(Location.ToPoint(),Size.ToPoint());

        /// <summary>
        /// point where you want to stop drawing from right side so -8 would stop 8*scale pixels from edge
        /// </summary>
        //internal sbyte Offset_X2 { get; set; }

        /// <summary>
        /// point where you want to stop drawing from bottom side so -8 would stop 8*scale pixels
        /// from edge
        /// </summary>
        //internal sbyte Offset_Y2 { get; set; }

        internal byte Part { get; set; } = 1;
        internal bool Snap_Bottom { get; set; } = false;
        internal bool Snap_Right { get; set; } = false;
        /// <summary>
        /// Vector2.Zero = no tile, Vector2.One = tile x & y, Vector.UnitX = tile x, Vector.UnitY = tile y
        /// </summary>
        internal Vector2 Tile { get; set; } = Vector2.Zero;
        internal float Height { get =>Size.Y; set=> Size.Y=value; }
        internal float Width { get=>Size.X; set=> Size.X=value; }
        internal float Y { get => Location.Y; set => Location.Y = value; }
        internal float X { get => Location.X; set => Location.X = value; }
        internal Vector2 Fill { get; set; }

        #endregion Properties

        #region Methods

        internal void LoadfromStreamSP2( BinaryReader br, UInt16 loc, Entry prev, ref byte fid)
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
            if (prev != null && Location.Y < prev.Y)
                fid++;
            File = fid;
        }

        internal void SetLoc(Loc value) => loc = value;

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
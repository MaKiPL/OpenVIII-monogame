using Microsoft.Xna.Framework;
using System;
using System.IO;

namespace OpenVIII
{
    public class Entry
    {

        #region Fields

        public Vector2 End;
        public Vector2 Location;
        public Vector2 Offset;
        public Vector2 Size;
        public string ToStringHeader { get; } = "{File},{Part},{CustomPalette},{Location.X},{Location.Y},{Size.X},{Size.Y},{Offset.X},{Offset.Y},{End.X},{End.Y},{Tile.X},{Tile.Y},{Fill.X},{Fill.Y},{Snap_Right},{Snap_Bottom}\n";
        public byte[] UNK;
        private Loc loc;

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

        public sbyte CustomPalette { get; set; } = -1;

        public byte File { get; set; }

        public Vector2 Fill { get; set; }

        //public Loc GetLoc => loc;
        public Rectangle GetRectangle => new Rectangle(Location.ToPoint(), Size.ToPoint());

        public float Height { get => Size.Y; set => Size.Y = value; }

        public byte Part { get; set; } = 1;

        public void SetTrim_1stPass(Rectangle value,bool skipoffset = false)
        {
            Vector2 newLoc = value.Location.ToVector2();
            //Offset may need to change.
            //I had to preserve a good offset for the D-pad to work. 
            //So i was getting the difference in the two locations and adding to the offset.
            //skipoffset can be used if the entry is solo so you don't need to alter the offset.
            if(!skipoffset)
            Offset += (newLoc - Location).Abs();
            Location = newLoc;
            Size = value.Size.ToVector2();
        }
        /// <summary>
        /// After determining the trimmed dim.
        /// There will be code in entrygroup that finds the actual top left and then passes to this function
        /// This will take that offset and subtract it from the current Offset value.
        /// </summary>
        /// <param name="offset"></param>
        public void SetTrim_2ndPass(Vector2 offset)
        {
            Offset -= offset;
        }
        /// <summary>
        /// point where you want to stop drawing from bottom side so -8 would stop 8*scale pixels
        /// from edge
        /// </summary>
        //public sbyte Offset_Y2 { get; set; }
        public bool Snap_Bottom { get; set; } = false;

        /// <summary>
        /// point where you want to stop drawing from right side so -8 would stop 8*scale pixels from edge
        /// </summary>
        //public sbyte Offset_X2 { get; set; }
        public bool Snap_Right { get; set; } = false;

        /// <summary>
        /// Vector2.Zero = no tile, Vector2.One = tile x & y, Vector.UnitX = tile x, Vector.UnitY = tile y
        /// </summary>
        public Vector2 Tile { get; set; } = Vector2.Zero;

        public float Width { get => Size.X; set => Size.X = value; }

        public float X { get => Location.X; set => Location.X = value; }

        public float Y { get => Location.Y; set => Location.Y = value; }
        /// <summary>
        /// Shadowcopy
        /// </summary>
        /// <returns>Copy of class</returns>
        public Entry Clone()
            => (Entry)MemberwiseClone();

        #endregion Properties

        #region Methods

        public void LoadfromStreamSP1(BinaryReader br)
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

        public void LoadfromStreamSP2(BinaryReader br, UInt16 loc, Entry prev, ref byte fid)
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

        public void SetLoc(Loc value) => loc = value;

        public override string ToString()
        {
            
            return $"{File},{Part},{CustomPalette},{Location.X},{Location.Y},{Size.X},{Size.Y},{Offset.X},{Offset.Y},{End.X},{End.Y},{Tile.X},{Tile.Y},{Fill.X},{Fill.Y},{Snap_Right},{Snap_Bottom}\n";
        }

        #endregion Methods
    }
}
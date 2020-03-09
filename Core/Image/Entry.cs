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
        public byte[] Unk;
        private Loc _loc;

        #endregion Fields

        #region Constructors

        public Entry()
        {
            Unk = new byte[2];
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

        /// <summary> Vector2.Zero = no tile, Vector2.One = tile x & y, Vector.UnitX = tile x,
        /// Vector.UnitY = tile y </summary>
        public Vector2 Tile { get; set; } = Vector2.Zero;

        public string ToStringHeader { get; } = "{File},{Part},{CustomPalette},{Location.X},{Location.Y},{Size.X},{Size.Y},{Offset.X},{Offset.Y},{End.X},{End.Y},{Tile.X},{Tile.Y},{Fill.X},{Fill.Y},{Snap_Right},{Snap_Bottom}\n";
        public bool Trimmed { get; private set; }
        public float Width { get => Size.X; set => Size.X = value; }

        public float X { get => Location.X; set => Location.X = value; }

        public float Y { get => Location.Y; set => Location.Y = value; }

        /// <summary>
        /// Animation Frame
        /// </summary>
        public int Frame { get; set; } = -1;

        /// <summary>
        /// How long to spend on each frame.
        /// </summary>
        public TimeSpan TimePerFrame { get; set; } = TimeSpan.Zero;

        /// <summary>
        /// Number Value, set if this is a number.
        /// </summary>
        public int? NumberValue { get; set; } = null;

        /// <summary>
        /// If set this has BattleID, default behavior is to leave unset.
        /// </summary>
        public Enum ID { get; set; } = null;

        #endregion Properties

        #region Methods

        /// <summary>
        /// Shadowcopy
        /// </summary>
        /// <returns>Copy of class</returns>
        public Entry Clone()
            => (Entry)MemberwiseClone();

        public void LoadfromStreamSP1(BinaryReader br)
        {
            CurrentPos = (ushort)br.BaseStream.Position;
            Location.X = br.ReadByte();
            Location.Y = br.ReadByte();

            Unk = br.ReadBytes(2);
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
            Unk = br.ReadBytes(2);
            Size.X = br.ReadByte();
            Offset.X = br.ReadSByte();
            Size.Y = br.ReadByte();
            Offset.Y = br.ReadSByte();
            if (prev != null && Location.Y < prev.Y)
                fid++;
            File = fid;
        }

        public void SetLoc(Loc value) => _loc = value;

        public void SetTrim_1stPass(Rectangle value, bool skipoffset = false)
        {
            Trimmed = true;
            Vector2 newLoc = value.Location.ToVector2();
            //Offset may need to change.
            //I had to preserve a good offset for the D-pad to work.
            //So i was getting the difference in the two locations and adding to the offset.
            //skipoffset can be used if the entry is solo so you don't need to alter the offset.
            if (!skipoffset)
                Offset += (newLoc - Location).Abs();
            Location = newLoc;
            Size = value.Size.ToVector2();
        }

        /// <summary>
        /// After determining the trimmed dim. There will be code in entrygroup that finds the actual
        /// top left and then passes to this function This will take that offset and subtract it from
        /// the current Offset value.
        /// </summary>
        /// <param name="offset"></param>
        public void SetTrim_2ndPass(Vector2 offset) => Offset -= offset;

        public void SetTrimNonGroup(TextureHandler tex)
        {
            if (!Trimmed)
                SetTrim_1stPass(tex.Trim(GetRectangle));
        }

        public override string ToString()
        {        //public override string ToString() => $"{File},{Part},{CustomPalette},{Location.X},{Location.Y},{Size.X},{Size.Y},{Offset.X},{Offset.Y},{End.X},{End.Y},{Tile.X},{Tile.Y},{Fill.X},{Fill.Y},{Snap_Right},{Snap_Bottom}\n";

            string prefix = "";
            string suffix = "";
            if (ID != null)
                prefix = ID.ToString();
            if (NumberValue.HasValue)
                suffix = NumberValue.Value.ToString();
            if (!string.IsNullOrWhiteSpace(prefix) && !string.IsNullOrWhiteSpace(suffix))
                return $"{prefix}.{suffix}";
            else if (!string.IsNullOrWhiteSpace(prefix))
                return prefix;
            else if (!string.IsNullOrWhiteSpace(suffix))
                return suffix;
            return base.ToString();
        }

        #endregion Methods
    }
}
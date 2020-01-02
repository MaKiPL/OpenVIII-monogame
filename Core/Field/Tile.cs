using Microsoft.Xna.Framework;

namespace OpenVIII
{





    public static partial class Module_field_debug
    {
        private class Tile
        {
            public const int size = 16;

            /// <summary>
            /// Distance from left side
            /// </summary>
            public short X;

            /// <summary>
            /// Distance from top side
            /// </summary>
            public short Y;

            /// <summary>
            /// Larger number is farther away. So OrderByDecending for draw order.
            /// </summary>
            public ushort Z;

            public byte TextureID; // 4 bits
            public byte PaletteID; //[6-10]

            /// <summary>
            /// Source X from Texture Data
            /// </summary>
            public byte SourceX;

            /// <summary>
            /// Source Y from Texture Data
            /// </summary>
            public byte SourceY;

            /// <summary>
            /// Layer ID, Used to control which parts draw.
            /// </summary>
            public byte LayerID;

            public BlendMode BlendMode;
            public byte AnimationState;
            public byte AnimationID;
            public byte blend1;
            public byte Depth;
            public float Zfloat => Z / 4096f;
            public int TileID;
            public bool Is8Bit => Depth >= 4;
            public bool Is4Bit => !Is8Bit;

            /// <summary>
            /// Gets +1 if Overlaps() == true;
            /// </summary>
            public byte OverLapID = 0;

            //public byte[] PaletteID4Bit => new byte[] {  };
            private uint pupuID;

            /// <summary>
            /// Pupu goes in a loop and +1 the id when it detects an overlap.
            /// There are some wasted bits
            /// </summary>
            public uint PupuID
            {
                get
                {
                    if (pupuID == 0)
                        pupuID = ((uint)(LayerID) << 24) + (((uint)BlendMode & 0x2) << 20) + ((uint)AnimationID << 12) + ((uint)(AnimationState & 0xF) << 4);
                    return pupuID;
                }
                set => pupuID = value;
            }

            public Rectangle Intersection { get => intersection; private set => intersection = value; }

            private Rectangle intersection;

            public bool Intersect(Tile tile, bool rev = false)
            {
                bool ret = Intersect(tile, out intersection, rev);
                tile.Intersection = Intersection;
                return ret;
            }

            public bool Intersect(Tile tile, out Rectangle intersection , bool rev = false)
            {
                intersection = Rectangle.Empty;
                bool flip = !rev && tile.Intersect(this, out intersection, !rev);
                bool ret = flip ||
                    X >= tile.X &&
                    X < tile.X + size &&
                    Y >= tile.Y &&
                    Y < tile.Y + size &&
                    Z == tile.Z &&
                    LayerID == tile.LayerID &&
                    BlendMode == tile.BlendMode &&
                    AnimationID == tile.AnimationID &&
                    AnimationState == tile.AnimationState;
                if (!flip && ret)
                    intersection = Rectangle.Intersect(GetRectangle(), tile.GetRectangle());
                return ret;
            }
            public Rectangle GetRectangle() => new Rectangle(X, Y, size, size);
            public static implicit operator Vector3(Tile @in) => new Vector3(@in.X, @in.Y, @in.Zfloat);
        }
    }
}
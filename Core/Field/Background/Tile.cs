using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace OpenVIII
{
    public partial class Background
    {
        #region Classes

        private class Tile
        {
            #region Fields

            public const int size = 16;

            /// <summary>
            /// Size of Texture Segment
            /// </summary>
            public static readonly Vector2 TextureSize = new Vector2(fourBitTexturePageWidth, fourBitTexturePageWidth);

            public byte AnimationID = 0xFF;

            public byte AnimationState;

            public byte blend1;

            public BlendMode BlendMode = BlendMode.none;
            public byte Depth;

            /// <summary>
            /// Layer ID, Used to control which parts draw.
            /// </summary>
            public byte LayerID;

            /// <summary>
            /// Gets +1 if Overlaps() == true;
            /// </summary>
            public byte OverLapID = 0;

            public byte PaletteID;

            /// <summary>
            /// for outputting the source tiles to texture pages. some tiles have the same source rectangle. So skip.
            /// </summary>
            public bool Skip = false;

            /// <summary>
            /// Source X from Texture Data
            /// </summary>
            public ushort SourceX;

            //[6-10]
            /// <summary>
            /// Source Y from Texture Data
            /// </summary>
            public ushort SourceY;

            public byte TextureID;

            public int TileID;

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

            //public byte[] PaletteID4Bit => new byte[] {  };
            private uint pupuID;

            #endregion Fields

            #region Properties

            public bool Is4Bit => !Is8Bit;

            public bool Is8Bit => Depth >= 4;

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

            public byte SourceOverLapID { get; internal set; }

            /// <summary>
            /// TopLeft UV
            /// </summary>
            public Vector2 UV => new Vector2(SourceX, SourceY) / TextureSize;

            // 4 bits
            public float Zfloat => Z / 4096f;

            #endregion Properties

            #region Methods

            /// <summary>
            /// TopLeft Cord
            /// </summary>
            /// <param name="in"></param>
            public static implicit operator Vector2(Tile @in) => new Vector2(@in.SourceX, @in.SourceY);

            public static implicit operator Vector3(Tile @in) => new Vector3(@in.X, @in.Y, @in.Zfloat);

            public static Tile Load(BinaryReader pbsmap, int id, byte type)
            {
                long p = pbsmap.BaseStream.Position;
                Tile t = new Tile { X = pbsmap.ReadInt16() };
                if (t.X == 0x7FFF)
                    return null;
                t.Y = pbsmap.ReadInt16();
                if (type == 1)
                {
                    t.Z = pbsmap.ReadUInt16();// (ushort)(4096 - pbsmap.ReadUShort());
                    byte texIdBuffer = pbsmap.ReadByte();
                    t.TextureID = (byte)(texIdBuffer & 0xF);
                    // pbsmap.BaseStream.Seek(-1, SeekOrigin.Current);
                    pbsmap.BaseStream.Seek(1, SeekOrigin.Current);
                    t.PaletteID = (byte)((pbsmap.ReadInt16() >> 6) & 0xF);
                    t.SourceX = pbsmap.ReadByte();
                    t.SourceY = pbsmap.ReadByte();
                    t.LayerID = (byte)(pbsmap.ReadByte() >> 1/*& 0x7F*/);
                    t.BlendMode = (BlendMode)pbsmap.ReadByte();
                    t.AnimationID = pbsmap.ReadByte();
                    t.AnimationState = pbsmap.ReadByte();
                    t.blend1 = (byte)((texIdBuffer >> 4) & 0x1);
                    t.Depth = (byte)(texIdBuffer >> 5);
                    t.TileID = id;
                }
                else if (type == 2)
                {
                    t.SourceX = pbsmap.ReadUInt16();
                    t.SourceY = pbsmap.ReadUInt16();
                    t.Z = pbsmap.ReadUInt16();
                    byte texIdBuffer = pbsmap.ReadByte();
                    t.TextureID = (byte)(texIdBuffer & 0xF);
                    pbsmap.BaseStream.Seek(1, SeekOrigin.Current);
                    t.PaletteID = (byte)((pbsmap.ReadInt16() >> 6) & 0xF);
                    t.AnimationID = pbsmap.ReadByte();
                    t.AnimationState = pbsmap.ReadByte();
                    t.blend1 = (byte)((texIdBuffer >> 4) & 0x1);
                    t.Depth = (byte)(texIdBuffer >> 5);
                    t.TileID = id;
                }
                Debug.Assert(p - pbsmap.BaseStream.Position == -16);
                return t;
            }

            public Rectangle GetRectangle() => new Rectangle(X, Y, size, size);

            public bool Intersect(Tile tile, bool rev = false)
            {
                bool flip = !rev && tile.Intersect(this, !rev);
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
                return ret;
            }

            public bool SourceIntersect(Tile tile, bool rev = false)
            {
                bool flip = !rev && tile.SourceIntersect(this, !rev);
                if (!flip && tile.TextureID == TextureID)
                {
                    return Rectangle.Intersect(SourceRectangle(), tile.SourceRectangle()) != Rectangle.Empty;
                }
                return flip;
            }

            public Rectangle SourceRectangle() => new Rectangle(SourceX, SourceY, size, size);

            private List<VertexPositionTexture> GetCorners(float scale)
            {
                Vector2 sizeVertex = new Vector2(size,size)/scale;
                Vector2 sizeUV = new Vector2(size) / TextureSize;
                List<VertexPositionTexture> r = new List<VertexPositionTexture>(4);
                for (int i = 0; i < r.Capacity; i++)
                {
                    VertexPositionTexture vpt = new VertexPositionTexture(((Vector3)this) / scale, UV);
                    switch (i)
                    {
                        case 0://top left
                            break;

                        case 1://top right
                            vpt.Position.X += sizeVertex.X;
                            break;

                        case 2://bottom right
                            vpt.Position.X += sizeVertex.X;
                            vpt.Position.Y += sizeVertex.Y;
                            break;

                        case 3://bottom left
                            vpt.Position.Y += sizeVertex.Y;
                            break;
                    }
                    switch (i)
                    {
                        case 0://top left
                            break;

                        case 1://top right
                            vpt.TextureCoordinate.X += sizeUV.X;
                            break;

                        case 2://bottom right
                            vpt.TextureCoordinate.X += sizeUV.X;
                            vpt.TextureCoordinate.Y += sizeUV.Y;
                            break;

                        case 3://bottom left
                            vpt.TextureCoordinate.Y += sizeUV.Y;
                            break;
                    }
                    Vector3 vectorflip = new Vector3(-1, -1, 1);
                    vpt.Position *= vectorflip;
                    r.Add(vpt);
                }

                return r;
            }

            public VertexPositionTexture[] GetQuad(float scale)
            {
                List<VertexPositionTexture> vpts = GetCorners(scale); // 4 unique corners.
                //create 2 triangles
                List<VertexPositionTexture> r = new List<VertexPositionTexture>
                {
                    vpts[3],
                    vpts[1],
                    vpts[0],

                    vpts[3],
                    vpts[2],
                    vpts[1],
                };
                return r.ToArray();
            }

            #endregion Methods
        }

        #endregion Classes
    }
}
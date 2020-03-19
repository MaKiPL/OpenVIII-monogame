using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace OpenVIII.Fields
{
    public partial class Background
    {
        #region Classes

        public sealed class Tile
        {
            #region Fields

            public const int Size = 16;

            /// <summary>
            /// Size of Texture Segment
            /// </summary>
            public static readonly Vector2 TextureSize = new Vector2(FourBitTexturePageWidth, FourBitTexturePageWidth);

            public byte AnimationID = 0xFF;

            public byte AnimationState;

            public byte Blend;

            public BlendMode BlendMode = BlendMode.None;
            public Bppflag Depth;

            /// <summary>
            /// bit is either 1 or 0. if 0 don't draw tile.
            /// </summary>
            /// <see cref="https://github.com/myst6re/deling/blob/develop/files/BackgroundFile.h#L49"/>
            public bool Draw = true;

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
            /// Larger number is farther away. So OrderByDescending for draw order.
            /// </summary>
            public ushort Z;

            #endregion Fields

            #region Properties

            public Rectangle ExpandedSource => new Rectangle(ExpandedSourceX, SourceY, Size, Size);

            public int ExpandedSourceX => Is4Bit ? SourceX * 2 : SourceX;
            public Rectangle GetRectangle => new Rectangle(X, Y, Size, Size);
            public int Height => Size;
            public bool Is4Bit => Test4Bit(Depth);

            public bool Is8Bit => Test8Bit(Depth);

            /// <summary>
            /// Pupu goes in a loop and +1 the ID when it detects an overlap.
            /// There are some wasted bits
            /// </summary>
            public uint PupuID { get; set; }

            public Rectangle Source => new Rectangle(SourceX, SourceY, Size, Size);

            /// <summary>
            /// TopLeft UV
            /// </summary>
            public Vector2 UV => new Vector2(SourceX, SourceY) / TextureSize;

            public int Width => Size;

            // 4 bits
            public float ZFloat => Z / 4096f;

            #endregion Properties

            #region Methods

            /// <summary>
            /// TopLeft Cord
            /// </summary>
            /// <param name="in"></param>
            public static implicit operator Vector2(Tile @in) => new Vector2(@in.SourceX, @in.SourceY);

            public static implicit operator Vector3(Tile @in) => new Vector3(@in.X, @in.Y, @in.ZFloat);

            public static Tile Load(BinaryReader br, int id, byte type)
            {
                long p = br.BaseStream.Position;
                Tile t = new Tile { X = br.ReadInt16() };
                if (t.X == 0x7FFF)
                    return null;
                t.Y = br.ReadInt16();
                if (type == 1)
                {
                    t.Z = br.ReadUInt16();
                    byte texIdBuffer = br.ReadByte();
                    t.TextureID = (byte)(texIdBuffer & 0xF);

                    br.BaseStream.Seek(1, SeekOrigin.Current);
                    t.PaletteID = GetPaletteID(br);
                    t.SourceX = br.ReadByte();
                    t.SourceY = br.ReadByte();
                    t.LayerID = GetLayerID(br);
                    t.BlendMode = (BlendMode)br.ReadByte();
                    t.AnimationID = br.ReadByte();
                    t.AnimationState = br.ReadByte();
                    t.Draw = GetDraw(texIdBuffer);
                    t.Blend = GetBlend(texIdBuffer);
                    t.Depth = GetDepth(texIdBuffer);
                    t.TileID = id;
                }
                else if (type == 2)
                {
                    t.SourceX = br.ReadUInt16();
                    t.SourceY = br.ReadUInt16();
                    t.Z = br.ReadUInt16();
                    byte texIdBuffer = br.ReadByte();
                    t.TextureID = (byte)(texIdBuffer & 0xF);
                    br.BaseStream.Seek(1, SeekOrigin.Current);
                    t.PaletteID = GetPaletteID(br);
                    t.AnimationID = br.ReadByte();
                    t.AnimationState = br.ReadByte();
                    t.Draw = GetDraw(texIdBuffer);
                    t.Blend = GetBlend(texIdBuffer);
                    t.Depth = GetDepth(texIdBuffer);
                    t.TileID = id;
                    t.BlendMode = (((t.Blend & 1) != 0) ? BlendMode.Add : BlendMode.None);
                }
                t.GeneratePupu();
                Debug.Assert(p - br.BaseStream.Position == -16);
                return t;
            }

            public static bool Test16Bit(Bppflag depth) => depth.HasFlag(Bppflag._16bpp) && !depth.HasFlag(Bppflag._8bpp);

            public static bool Test4Bit(Bppflag depth) => depth == 0;

            public static bool Test8Bit(Bppflag depth) => !depth.HasFlag(Bppflag._16bpp) && depth.HasFlag(Bppflag._8bpp);

            /*
                        public bool Is16Bit => Test16Bit(Depth);
            */
            /*
                        public byte SourceOverLapID { get; internal set; }
            */

            public VertexPositionTexture[] GetQuad(float scale)
            {
                List<VertexPositionTexture> vertexPositionTextures = GetCorners(scale); // 4 unique corners.
                //create 2 triangles
                List<VertexPositionTexture> r = new List<VertexPositionTexture>
                {
                    vertexPositionTextures[3],
                    vertexPositionTextures[1],
                    vertexPositionTextures[0],

                    vertexPositionTextures[3],
                    vertexPositionTextures[2],
                    vertexPositionTextures[1],
                };
                return r.ToArray();
            }

            public bool Intersect(Tile tile, bool rev = false)
            {
                bool flip = !rev && tile.Intersect(this, true);
                bool ret = flip ||
                    X >= tile.X &&
                    X < tile.X + Size &&
                    Y >= tile.Y &&
                    Y < tile.Y + Size;// &&
                                      //Z == tile.Z &&
                                      //LayerID == tile.LayerID &&
                                      //BlendMode == tile.BlendMode &&
                                      //AnimationID == tile.AnimationID &&
                                      //AnimationState == tile.AnimationState;
                return ret;
            }

            public override string ToString() =>
                $"Tile: {TileID}; " +
                $"Loc: {GetRectangle}; " +
                $"Z: {Z}; " +
                $"Source: {ExpandedSource}; " +
                $"TextureID: {TextureID}; " +
                $"PaletteID: {PaletteID}; " +
                $"LayerID: {LayerID}; " +
                $"BlendMode: {BlendMode}; " +
                $"AnimationID: {AnimationID}; " +
                $"AnimationState: {AnimationState}; " +
                $"4 bit? {Is4Bit}";

            private static byte GetBlend(byte texIdBuffer) => (byte)((texIdBuffer >> 5) & 0x3);

            private static Bppflag GetDepth(byte texIdBuffer) => (Bppflag)(texIdBuffer >> 7 & 0x3);

            private static bool GetDraw(byte texIdBuffer) => ((texIdBuffer >> 4) & 0x1) != 0;

            private static byte GetLayerID(BinaryReader br) => (byte)(br.ReadByte() >> 1);

            private static byte GetPaletteID(BinaryReader br) => (byte)((br.ReadInt16() >> 6) & 0xF);

            /*
                        public bool SourceIntersect(Tile tile)
            =>
                                Rectangle.Intersect(ExpandedSource, tile.ExpandedSource) != Rectangle.Empty;
            */

            private void GeneratePupu()
            {
                const int bitsPerLong = sizeof(ulong) * 8;
                const int bitsPerByte = sizeof(byte) * 8;
                const int bitsPerNibble = bitsPerByte / 2;
                int bits = bitsPerLong;
                bits -= bitsPerNibble;
                PupuID = (((uint)LayerID & 0xF) << bits);
                bits -= bitsPerNibble;
                PupuID += (((uint)BlendMode & 0xF) << bits);
                bits -= bitsPerByte;
                PupuID += ((uint)AnimationID << bits);
                bits -= bitsPerByte;
                PupuID += ((uint)(AnimationState) << bits);
                Debug.Assert(((PupuID & 0xF0000000) >> 28) == LayerID);
                Debug.Assert((BlendMode)((PupuID & 0x0F000000) >> 24) == BlendMode);
                Debug.Assert(((PupuID & 0x00FF0000) >> 16) == AnimationID);
                Debug.Assert(((PupuID & 0x0000FF00) >> 8) == AnimationState);
            }

            private List<VertexPositionTexture> GetCorners(float scale)
            {
                Vector2 sizeVertex = new Vector2(Size, Size) / scale;
                Vector2 sizeUV = new Vector2(Size) / TextureSize;
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
                    Vector3 vectorFlip = new Vector3(-1, -1, 1);
                    vpt.Position *= vectorFlip;
                    r.Add(vpt);
                }

                return r;
            }

            #endregion Methods
        }

        #endregion Classes
    }
}
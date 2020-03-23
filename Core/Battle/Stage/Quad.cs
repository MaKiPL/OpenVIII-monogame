using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.IO;

namespace OpenVIII.Battle
{
    public partial class Stage
    {
        #region Classes

        private class Quad : Polygon
        {
            #region Fields

            public readonly ushort D;
            private readonly byte _u4;
            private readonly byte _v4;

            #endregion Fields

            #region Methods

            private Quad(BinaryReader br)
            {
                byte tmp;

                A = br.ReadUInt16();
                B = br.ReadUInt16();
                C = br.ReadUInt16();
                D = br.ReadUInt16();
                U1 = br.ReadByte();
                V1 = br.ReadByte();
                Clut = GetClutId(br.ReadUInt16());
                U2 = br.ReadByte();
                V2 = br.ReadByte();
                TexturePage = GetTexturePage(tmp = br.ReadByte());
                BHide = br.ReadByte();
                U3 = br.ReadByte();
                V3 = br.ReadByte();
                _u4 = br.ReadByte();
                _v4 = br.ReadByte();
                Red = br.ReadByte();
                Green = br.ReadByte();
                Blue = br.ReadByte();
                GPU = (GPU) br.ReadByte();
            
                Unk = (byte)((tmp & 0xF0) >> 1);
                UVs = new List<Vector2> { new Vector2(U1, V1), new Vector2(U2, V2), new Vector2(U3, V3), new Vector2(_u4, _v4) };
            }
            public static Quad Read(BinaryReader br)
            {
                return new Quad(br);
            }

            #endregion Methods
        }

        #endregion Classes
    }
}
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.IO;

namespace OpenVIII.Battle
{
    public partial class Stage
    {
        #region Classes

        private class Triangle : Polygon
        {

            private Triangle(BinaryReader br)
            {
                byte tmp;
                A = br.ReadUInt16();
                B = br.ReadUInt16();
                C = br.ReadUInt16();
                U1 = br.ReadByte();
                V1 = br.ReadByte();
                U2 = br.ReadByte();
                V2 = br.ReadByte();
                Clut = GetClutId(br.ReadUInt16());
                U3 = br.ReadByte();
                V3 = br.ReadByte();
                TexturePage = GetTexturePage(tmp = br.ReadByte());
                BHide = br.ReadByte();
                Red = br.ReadByte();
                Green = br.ReadByte();
                Blue = br.ReadByte();
                GPU = (GPU)br.ReadByte();
                Unk = (byte)((tmp & 0xF0) >> 1);
                UVs = new List<Vector2> { new Vector2(U1, V1), new Vector2(U2, V2), new Vector2(U3, V3) };
            }
            #region Methods

            public static Triangle Read(BinaryReader br)
            {
                return new Triangle(br);
            }

            #endregion Methods

        }

        #endregion Classes

    }
}
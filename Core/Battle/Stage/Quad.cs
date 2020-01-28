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

            public ushort D;
            protected byte U4;
            protected byte V4;

            #endregion Fields

            #region Methods

            public static Quad Read(BinaryReader br)
            {
                byte tmp;
                Quad q = new Quad()
                {
                    A = br.ReadUInt16(),
                    B = br.ReadUInt16(),
                    C = br.ReadUInt16(),
                    D = br.ReadUInt16(),
                    U1 = br.ReadByte(),
                    V1 = br.ReadByte(),
                    clut = GetClutId(br.ReadUInt16()),
                    U2 = br.ReadByte(),
                    V2 = br.ReadByte(),
                    TexturePage = GetTexturePage(tmp = br.ReadByte()),
                    bHide = br.ReadByte(),
                    U3 = br.ReadByte(),
                    V3 = br.ReadByte(),
                    U4 = br.ReadByte(),
                    V4 = br.ReadByte(),
                    Red = br.ReadByte(),
                    Green = br.ReadByte(),
                    Blue = br.ReadByte(),
                    GPU = (GPU)br.ReadByte()
                };
                q.UNK = (byte)((tmp & 0xF0) >> 1);
                q.UVs = new List<Vector2> { new Vector2(q.U1, q.V1), new Vector2(q.U2, q.V2), new Vector2(q.U3, q.V3), new Vector2(q.U4, q.V4) };
                return q;
            }

            #endregion Methods
        }

        #endregion Classes
    }
}
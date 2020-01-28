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

            #region Methods

            public static Triangle Read(BinaryReader br)
            {
                byte tmp;
                Triangle t =
                    new Triangle()
                    {
                        A = br.ReadUInt16(),
                        B = br.ReadUInt16(),
                        C = br.ReadUInt16(),
                        U1 = br.ReadByte(),
                        V1 = br.ReadByte(),
                        U2 = br.ReadByte(),
                        V2 = br.ReadByte(),
                        clut = GetClutId(br.ReadUInt16()),
                        U3 = br.ReadByte(),
                        V3 = br.ReadByte(),
                        TexturePage = GetTexturePage(tmp = br.ReadByte()),
                        bHide = br.ReadByte(),
                        Red = br.ReadByte(),
                        Green = br.ReadByte(),
                        Blue = br.ReadByte(),
                        GPU = (GPU)br.ReadByte(),
                    };
                t.UNK = (byte)((tmp & 0xF0) >> 1);
                t.UVs = new List<Vector2> { new Vector2(t.U1, t.V1), new Vector2(t.U2, t.V2), new Vector2(t.U3, t.V3) };
                return t;
            }

            #endregion Methods

        }

        #endregion Classes

    }
}
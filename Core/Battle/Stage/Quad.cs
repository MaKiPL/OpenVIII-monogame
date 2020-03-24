using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.IO;

namespace OpenVIII.Battle
{
    public partial class Stage
    {
        #region Classes

        private class Quad : IPolygon, IQuad
        {
            #region Constructors

            private Quad(BinaryReader br)
            {
                A = br.ReadUInt16();
                B = br.ReadUInt16();
                C = br.ReadUInt16();
                D = br.ReadUInt16();
                var u1 = br.ReadByte();
                var v1 = br.ReadByte();
                Clut = GetClutId(br.ReadUInt16());
                var u2 = br.ReadByte();
                var v2 = br.ReadByte();
                TexturePage = GetTexturePage(br.ReadByte());
                Hide = br.ReadByte();
                var u3 = br.ReadByte();
                var v3 = br.ReadByte();
                var u4 = br.ReadByte();
                var v4 = br.ReadByte();
                Color = new Color(br.ReadByte(), br.ReadByte(), br.ReadByte());
                GPU = (GPU)br.ReadByte();

                //byte unk = (byte) ((tmp & 0xF0) >> 1);
                UVs = new [] { new Vector2(u1, v1), new Vector2(u2, v2), new Vector2(u3, v3), new Vector2(u4, v4) };
            }

            #endregion Constructors

            #region Properties

            public ushort A { get; }

            public ushort B { get; }

            public ushort C { get; }

            public byte Clut { get; }

            public ushort D { get; }

            public GPU GPU { get; }

            public byte TexturePage { get; }
            public byte Hide { get; }
            public Color Color { get; }
            public Vector2[] UVs { get; }

            #endregion Properties

            #region Methods

            public static Quad Read(BinaryReader br) => new Quad(br);

            #endregion Methods
        }

        #endregion Classes
    }
}
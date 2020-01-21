using System.IO;

namespace OpenVIII.Battle
{
    public partial class Stage
    {
        #region Structs

        private struct Quad
        {
            #region Fields

            public ushort A;
            public ushort B;
            public byte bHide;
            public byte Blue;
            public ushort C;
            public byte clut;
            public ushort D;
            public byte GPU;
            public byte Green;
            public byte Red;
            public byte TexturePage;
            public byte U1;
            public byte U2;
            public byte U3;
            public byte U4;
            public byte V1;
            public byte V2;
            public byte V3;
            public byte V4;

            #endregion Fields

            #region Methods

            public static Quad Read(BinaryReader br)
            => new Quad()
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
                TexturePage = GetTexturePage(br.ReadByte()),
                bHide = br.ReadByte(),
                U3 = br.ReadByte(),
                V3 = br.ReadByte(),
                U4 = br.ReadByte(),
                V4 = br.ReadByte(),
                Red = br.ReadByte(),
                Green = br.ReadByte(),
                Blue = br.ReadByte(),
                GPU = br.ReadByte()
            };

            #endregion Methods
        }

        #endregion Structs
    }
}
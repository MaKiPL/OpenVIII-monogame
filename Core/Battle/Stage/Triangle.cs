using System.IO;

namespace OpenVIII.Battle
{
    public partial class Stage
    {
        private struct Triangle
        {
            public ushort A;
            public ushort B;
            public ushort C;
            public byte U1;
            public byte V1;
            public byte U2;
            public byte V2;
            public byte clut;
            public byte U3;
            public byte V3;
            public byte TexturePage;
            public byte bHide;
            public byte Red;
            public byte Green;
            public byte Blue;
            public byte GPU;

            public static Triangle Read(BinaryReader br)
            =>
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
                    TexturePage = GetTexturePage(br.ReadByte()),
                    bHide = br.ReadByte(),
                    Red = br.ReadByte(),
                    Green = br.ReadByte(),
                    Blue = br.ReadByte(),
                    GPU = br.ReadByte(),
                };
        }
    }
}
using System;
using System.IO;

namespace OpenVIII.Battle
{
    public partial class Stage
    {
        private struct MainGeometrySection
        {
            public uint Group1Pointer;
            public uint Group2Pointer;
            public uint Group3Pointer;
            public uint Group4Pointer;
            public uint TextureUNUSEDPointer;
            public uint TexturePointer;
            public uint EOF;


            public static MainGeometrySection Read(BinaryReader br)
            {
                int basePointer = (int)br.BaseStream.Position - 4;
                uint objectGroup_1 = (uint)basePointer + br.ReadUInt32();
                uint objectGroup_2 = (uint)basePointer + br.ReadUInt32();
                uint objectGroup_3 = (uint)basePointer + br.ReadUInt32();
                uint objectGroup_4 = (uint)basePointer + br.ReadUInt32();
                uint TextureUnused = (uint)basePointer + br.ReadUInt32();
                uint Texture = (uint)basePointer + br.ReadUInt32();
                uint EOF = (uint)basePointer + br.ReadUInt32();
                if (EOF != br.BaseStream.Length)
                    throw new Exception("BS_PARSER_ERROR_LENGTH: Geometry EOF pointer is other than buffered filesize");

                return new MainGeometrySection()
                {
                    Group1Pointer = objectGroup_1,
                    Group2Pointer = objectGroup_2,
                    Group3Pointer = objectGroup_3,
                    Group4Pointer = objectGroup_4,
                    TextureUNUSEDPointer = TextureUnused,
                    TexturePointer = Texture,
                    EOF = EOF
                }; //EOF = EOF; beauty of language
            }
        }
    }
}
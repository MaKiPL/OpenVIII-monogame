using System;
using System.Runtime.InteropServices;

namespace OpenVIII
{
    public static partial class Module_field_debug
    {
        const int TileType1Size = 16;
        /// <summary>
        /// Tile Map
        /// </summary>
        /// <see cref="http://wiki.ffrtt.ru/index.php/FF8/FileFormat_MAP"/>
        [StructLayout(LayoutKind.Sequential, Pack = 1, Size = TileType1Size)]
        private struct Tile
        {
            private enum BlendMode :byte
            {
                @default = 4,
                additive = 1,
                subtractive,
                quarterpercent,
                unknown = 0
            }
            public short x;
            public short y;
            private ushort zint;
            private uint bitdata1; //32 bits

            public float z => zint / 4096f;
            public byte TexID => (byte)(bitdata1 & 0xF); // first 4 bits
            public bool unkbit1 => (bool)((bitdata1 & 0x10)!=0); //unk 1 bit
            public byte bbp => (byte) (((bitdata1 & 0x70) >> 5)<4?4:8); // 3 bits
            public bool unkbit2 => (bool)((bitdata1 & 0x80) != 0); //unk 1 bit
            public byte unkbit3 => (byte)((bitdata1 & 0x3F00) >> 12); //unk 6 bi
            public byte pallID => (byte)(((bitdata1 & 0x3C000) >>8)+8); //4 bit +8;
            public byte unkbit4 => (byte)((bitdata1 & 0xFC0000) >> 12); //unk 6 bit
            public byte srcx;
            public byte srcy;
            private byte layIdraw;
            public byte layId => (byte) ((layIdraw & 0xFE)>>1);
            public BlendMode blendType;
            public byte state;
            public byte parameter;
            public byte blend1;
            public byte blend2;
        }
    }
}
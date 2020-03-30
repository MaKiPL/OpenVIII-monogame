using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace OpenVIII
{
    //Class that provides language extensions made by JWP/Extapathy
    internal class ExtapathyExtended
    {
        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        public class BitReader : BinaryReader
        {
            private static readonly int[] PositionReadHelper = { 3, 6, 9, 16 };
            private static readonly int[] RotationReadHelper = { 3, 6, 8, 12 };
            private int _bitPosition;
            private long Position { get => BaseStream.Position; set => BaseStream.Position = value; }

            public BitReader(Stream input) : base(input) { }

            public short ReadBits(int count)
            {
                if (count > 16)
                    throw new ArgumentException();

                var position = Position;
                var byte1 = BaseStream.ReadByte();
                var byte2 = BaseStream.ReadByte();
                var byte3 = BaseStream.ReadByte();
                var temp = byte1 | byte2 << 8 | byte3 << 16;
                temp = (short)((temp >> _bitPosition) & ~(0xFFFFFFFF << count));
                var value = (short)((temp << (32 - count)) >> (32 - count));

                Position = position + (count + _bitPosition) / 8;
                _bitPosition = (count + _bitPosition) % 8;

                return value;
            }

            public short ReadPositionType()
            {
                var countIndex = ReadBits(2) & 3;
                return ReadBits(PositionReadHelper[countIndex]);
            }

            //+Maki
            public byte ReadPositionLength() => (byte)PositionReadHelper[ReadBits(2) & 0b11];
            public byte ReadRotationLength() => (byte)RotationReadHelper[ReadBits(2) & 0b11];
            //-Maki

            public short ReadRotationType()
            {
                var readRotation = (ReadBits(1) & 1) != 0;

                if (!readRotation)
                    return 0;

                var countIndex = ReadBits(2) & 3;
                return ReadBits(RotationReadHelper[countIndex]);
            }
        }
    }
}

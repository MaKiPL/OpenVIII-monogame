using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FF8
{
    //Class that provides language extensions made by JWP/Extapathy
    class ExtapathyExtended
    {
        public class BitReader : BinaryReader
        {
            private static readonly int[] positionReadHelper = { 3, 6, 9, 16 };
            private static readonly int[] rotationReadHelper = { 3, 6, 8, 12 };
            private int bitPosition = 0;
            private long Position { get => BaseStream.Position; set => BaseStream.Position = value; }

            public BitReader(Stream input) : base(input) { }

            public short ReadBits(int count)
            {
                if (count > 16)
                    throw new ArgumentException();

                var position = Position;
                int byte1 = BaseStream.ReadByte();
                int byte2 = BaseStream.ReadByte();
                int byte3 = BaseStream.ReadByte();
                var temp = byte1 | byte2 << 8 | byte3 << 16;
                temp = (short)((temp >> bitPosition) & ~(0xFFFFFFFF << count));
                short value = (short)((temp << (32 - count)) >> (32 - count));

                Position = position + (count + bitPosition) / 8;
                bitPosition = (count + bitPosition) % 8;

                return value;
            }

            public short ReadPositionType()
            {
                var countIndex = ReadBits(2) & 3;
                return ReadBits(positionReadHelper[countIndex]);
            }

            public short ReadRotationType()
            {
                var readRotation = (ReadBits(1) & 1) != 0;

                if (!readRotation)
                    return 0;

                var countIndex = ReadBits(2) & 3;
                return ReadBits(rotationReadHelper[countIndex]);
            }
        }
    }
}

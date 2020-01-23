using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Linq;

namespace OpenVIII.Fields
{
    public static partial class Msd
    {
        public static class Reader
        {
            public static IReadOnlyList<FF8String> FromBytes(Byte[] buff)
            {
                List<FF8String> monologues = new List<FF8String>();

                Int32 bufferSize = buff.Length;
                if (bufferSize < 4)
                    return monologues;
                        ReadMessages(buff, monologues);
                

                return monologues;
            }

            private static void ReadMessages(byte[] buff, List<FF8String> monologues)
            {

                using (BinaryReader br = new BinaryReader(new MemoryStream(buff)))
                {
                    uint dataOffset = br.ReadUInt32();
                    int count = checked((int)GetMessageNumber(dataOffset, br.BaseStream.Length));
                    if (count == 0)
                        return;

                    monologues.Capacity = count;
                    List<uint> Offsets = new List<uint>(count)
                    {
                        dataOffset
                    };
                    foreach (var i in Enumerable.Range(1,count-1))
                    {
                        Offsets.Add(br.ReadUInt32());
                    }
                    for (int i = 0; i < Offsets.Count; i++)
                    {
                        uint offset = Offsets[i];
                        uint nextoffset = i+1<Offsets.Capacity ? Offsets[i+1] : (uint)br.BaseStream.Length;
                        if (offset == nextoffset)
                        {
                            monologues.Add(String.Empty);
                            continue;
                        }
                        int length = checked((int)(nextoffset - offset - 1));
                        FF8String message = new FF8String(br.ReadBytes(length));
                        monologues.Add(message);
                    }                    
                }
            }

            private static UInt32 GetMessageNumber(UInt32 dataOffset, long bufferSize)
            {
                UInt32 count = dataOffset / 4;

                if (dataOffset % 4 != 0)
                    throw new InvalidDataException($"The offset to the beginning of the text data also determines the number of lines in the file and must be a multiple of 4. Occured: {dataOffset} mod 4 = {dataOffset % 4}");

                if (count < 0)
                    throw new InvalidDataException($"Unexpected negative value occured: {dataOffset}. Expected positive offset to the text data.");

                if (dataOffset > bufferSize)
                    throw new InvalidDataException($"Invalid data offset ({dataOffset}) is out of bounds ({bufferSize}).");

                return count;
            }
        }
    }
}
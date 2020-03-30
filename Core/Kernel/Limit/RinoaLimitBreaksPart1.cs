using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OpenVIII
{
    namespace Kernel
    {
        /// <summary>
        /// Rinoa limit breaks (part 1)
        /// </summary>
        /// <see cref="https://github.com/alexfilth/doomtrain/wiki/Rinoa-limit-breaks-%28part-1%29"/>
        public sealed class RinoaLimitBreaksPart1
        {
            private static RinoaLimitBreaksPart1 CreateInstance(BinaryReader br, int i)
            {
                return new RinoaLimitBreaksPart1(br, i);
            }

            public const int Count = 2;
            public const int ID = 24;
            public const int Size = 8;

            public override string ToString() => Name;

            public FF8String Name { get; }
            public FF8String Description { get; }
            public BitArray Unknown0 { get; }
            public Target Target { get; }
            public byte AbilityID { get; }
            public byte Unknown1 { get; }
            public int RinoaLimitID { get; }
            public Angelo Angelo { get; }

            private RinoaLimitBreaksPart1(BinaryReader br, int i)
            {
                RinoaLimitID = i;
                Angelo = i == 1 ? Angelo.Angel_Wing : Angelo.None;
                //0x0000	2 bytes Offset to name
                Name = Memory.Strings.Read(Strings.FileID.Kernel, ID, i * 2);
                //0x0002	2 bytes Offset to description
                Description = Memory.Strings.Read(Strings.FileID.Kernel, ID, i * 2 + 1);
                br.BaseStream.Seek(4, SeekOrigin.Current);
                //0x0004  1 byte Unknown Flags
                Unknown0 = new BitArray(br.ReadBytes(1));
                //0x0005  1 byte Target
                Target = (Target)br.ReadByte();
                //0x0006  1 byte Ability data ID
                AbilityID = br.ReadByte();
                //0x0007  1 byte Unknown / Unused
                Unknown1 = br.ReadByte();
            }

            public static IReadOnlyList<RinoaLimitBreaksPart1> Read(BinaryReader br)
                => Enumerable.Range(0, Count).Select(x => CreateInstance(br, x)).ToList().AsReadOnly();
        }
    }
}
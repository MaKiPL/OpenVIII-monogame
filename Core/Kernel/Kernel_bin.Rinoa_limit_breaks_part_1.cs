using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace OpenVIII
{
    namespace Kernel
    {
        /// <summary>
        /// Rinoa limit breaks (part 1)
        /// </summary>
        /// <see cref="https://github.com/alexfilth/doomtrain/wiki/Rinoa-limit-breaks-%28part-1%29"/>
        public class Rinoa_limit_breaks_part_1
        {
            public const int count = 2;
            public const int id = 24;
            public const int size = 8;

            public override string ToString() => Name;

            public FF8String Name { get; private set; }
            public FF8String Description { get; private set; }
            public BitArray Unknown0 { get; private set; }
            public Target Target { get; private set; }
            public byte AbilityID { get; private set; }
            public byte Unknown1 { get; private set; }
            public int ID { get; private set; }
            public Angelo Angelo { get; private set; }

            public void Read(BinaryReader br, int i)
            {
                ID = i;
                if (i == 1)
                    Angelo = Angelo.Angel_Wing;
                else
                    Angelo = Angelo.None;
                Name = Memory.Strings.Read(Strings.FileID.Kernel, id, i * 2);
                //0x0000	2 bytes Offset to name
                Description = Memory.Strings.Read(Strings.FileID.Kernel, id, i * 2 + 1);
                //0x0002	2 bytes Offset to description
                br.BaseStream.Seek(4, SeekOrigin.Current);
                Unknown0 = new BitArray(br.ReadBytes(1));
                //0x0004  1 byte Unknown Flags
                Target = (Target)br.ReadByte();
                //0x0005  1 byte Target
                AbilityID = br.ReadByte();
                //0x0006  1 byte Ability data BattleID
                Unknown1 = br.ReadByte();
                //0x0007  1 byte Unknown / Unused
            }
            public static List<Rinoa_limit_breaks_part_1> Read(BinaryReader br)
            {
                var ret = new List<Rinoa_limit_breaks_part_1>(count);

                for (int i = 0; i < count; i++)
                {
                    var tmp = new Rinoa_limit_breaks_part_1();
                    tmp.Read(br, i);
                    ret.Add(tmp);
                }
                return ret;
            }
        }
    }
}
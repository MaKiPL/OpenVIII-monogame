using System.Collections;
using System.IO;

namespace FF8
{
    public partial class Kernel_bin
    {
        /// <summary>
        /// Party Abilities Data
        /// </summary>
        /// <see cref="https://github.com/alexfilth/doomtrain/wiki/Party-abilities"/>
        public class Party_abilities : Equipable_Abilities
        {
            public new const int count = 5;
            public new const int id = 15;

            public BitArray Flags { get; private set; }
            public byte[] Unknown0 { get; private set; }

            public void Read(BinaryReader br, int i)
            {
                Name = Memory.Strings.Read(Strings.FileID.KERNEL, id, i * 2);
                //0x0000	2 bytes Offset to name
                Description = Memory.Strings.Read(Strings.FileID.KERNEL, id, i * 2 + 1);
                //0x0002	2 bytes Offset to description
                br.BaseStream.Seek(4, SeekOrigin.Current);
                AP = br.ReadByte();
                //0x0004  1 byte AP Required to learn ability
                Flags = new BitArray(br.ReadBytes(1));
                //0x0005  1 byte Flags
                Unknown0 = br.ReadBytes(2);
                //0x0006  2 byte Unknown/ Unused
            }
            public static Party_abilities[] Read(BinaryReader br)
            {
                var ret = new Party_abilities[count];

                for (int i = 0; i < count; i++)
                {
                    var tmp = new Party_abilities();
                    tmp.Read(br, i);
                    ret[i] = tmp;
                }
                return ret;
            }
        }
    }
}
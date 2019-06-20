using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace OpenVIII
{
    public partial class Kernel_bin
    {
        /// <summary>
        /// Party Abilities Data
        /// </summary>
        /// <see cref="https://github.com/alexfilth/doomtrain/wiki/Party-abilities"/>
        public class Party_abilities : Equipable_Ability
        {
            public new const int count = 5;
            public new const int id = 15;

            public BitArray Flags { get; private set; }
            public byte[] Unknown0 { get; private set; }

            public override void Read(BinaryReader br, int i)
            {
                Icon = Icons.ID.Ability_Party;
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
            public static Dictionary<Abilities,Party_abilities> Read(BinaryReader br)
            {
                Dictionary<Abilities, Party_abilities> ret = new Dictionary<Abilities, Party_abilities>(count);

                for (int i = 0; i < count; i++)
                {
                    var tmp = new Party_abilities();
                    tmp.Read(br, i);
                    ret[(Abilities)(i+(int)Abilities.Alert)] = tmp;
                }
                return ret;
            }
        }
    }
}
using System.IO;

namespace FF8
{
    internal partial class Kernel_bin
    {
        internal class Weapons_Data
        {
            internal const int count =33;
            internal const int id=4;
            internal FF8String Name { get; private set; }
            public override string ToString() => Name;

            //0x0000	2 bytes Offset to weapon name
            internal byte Renzokuken; //0x0002	1 byte Renzokuken finishers

            //  0x01 = Rough Divide
            //  0x02 = Fated Circle
            //  0x04 = Blasting Zone
            //  0x08 = Lion Heart
            internal byte Unknown0; //0x0003	1 byte Unknown
            internal Saves.Characters Character;//0x0004	1 byte Character ID

            internal byte Attack_type;//0x0005	1 bytes Attack Type
            internal byte Attack_power;//0x0006	1 byte Attack Power
            internal byte Attack_parameter;//0x0007	1 byte Attack Parameter
            internal byte STR;//0x0008	1 byte STR Bonus
            internal byte Tier;//0x0009	1 byte Weapon Tier
            internal byte CRIT;//0x000A	1 byte Crit Bonus
            internal byte Melee;//0x000B	1 byte Melee Weapon?

            internal void Read(BinaryReader br,int string_id = 0)
            {
                Name = Memory.Strings.Read(Strings.FileID.KERNEL, id, string_id);
                br.BaseStream.Seek(2, SeekOrigin.Current);
            }
        }
    }
}
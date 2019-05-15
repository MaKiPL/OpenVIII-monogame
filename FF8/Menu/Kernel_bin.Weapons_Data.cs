using System.IO;

namespace FF8
{
    public partial class Kernel_bin
    {
        public class Weapons_Data
        {
            public const int count =33;
            public const int id=4;
            public FF8String Name { get; private set; }
            public override string ToString() => Name;

            //0x0000	2 bytes Offset to weapon name
            public byte Renzokuken; //0x0002	1 byte Renzokuken finishers

            //  0x01 = Rough Divide
            //  0x02 = Fated Circle
            //  0x04 = Blasting Zone
            //  0x08 = Lion Heart
            public byte Unknown0; //0x0003	1 byte Unknown
            public Saves.Characters Character;//0x0004	1 byte Character ID

            public byte Attack_type;//0x0005	1 bytes Attack Type
            public byte Attack_power;//0x0006	1 byte Attack Power
            public byte Attack_parameter;//0x0007	1 byte Attack Parameter
            public byte STR;//0x0008	1 byte STR Bonus
            public byte Tier;//0x0009	1 byte Weapon Tier
            public byte CRIT;//0x000A	1 byte Crit Bonus
            public byte Melee;//0x000B	1 byte Melee Weapon?

            internal void Read(BinaryReader br,int string_id = 0)
            {
                Name = Memory.Strings.Read(Strings.FileID.KERNEL, id, string_id);
                br.BaseStream.Seek(2, SeekOrigin.Current);
            }
        }
    }
}
using System.Collections.Generic;
using System.IO;

namespace OpenVIII
{
    public partial class Kernel_bin
    {
        /// <summary>
        /// Weapon Data
        /// </summary>
        /// <see cref="https://github.com/alexfilth/doomtrain/wiki/Weapons"/>
        public class Weapons_Data
        {
            public const int count = 33;
            public const int id = 4;
            public FF8String Name { get; private set; }

            public override string ToString() => Name;

            //0x0000	2 bytes Offset to weapon name
            public Renzokeken_Finisher Renzokuken { get; private set; } //0x0002	1 byte Renzokuken finishers

            public byte Unknown0 { get; private set; } //0x0003	1 byte Unknown
            public Characters Character { get; private set; }//0x0004	1 byte Character ID
            public Attack_Type Attack_type { get; private set; }//0x0005	1 bytes Attack Type
            public byte Attack_power { get; private set; }//0x0006	1 byte Attack Power
            public byte HIT { get; private set; }//0x0007	1 byte Attack Parameter
            public byte STR { get; private set; }//0x0008	1 byte STR Bonus
            public byte Tier { get; private set; }//0x0009	1 byte Weapon Tier
            public byte CRIT { get; private set; }//0x000A	1 byte Crit Bonus
            public bool Melee { get; private set; }//0x000B	1 byte Melee Weapon?

            public void Read(BinaryReader br, int string_id = 0)
            {
                Name = Memory.Strings.Read(Strings.FileID.KERNEL, id, string_id);
                br.BaseStream.Seek(2, SeekOrigin.Current);
                Renzokuken = (Renzokeken_Finisher)br.ReadByte(); //0x0002	1 byte Renzokuken finishers
                Unknown0 = br.ReadByte(); //0x0003	1 byte Unknown
                Character = (Characters)br.ReadByte();//0x0004	1 byte Character ID
                Attack_type = (Attack_Type)br.ReadByte();//0x0005	1 bytes Attack Type
                Attack_power = br.ReadByte();//0x0006	1 byte Attack Power
                HIT = br.ReadByte();//0x0007	1 byte Attack Parameter
                STR = br.ReadByte();//0x0008	1 byte STR Bonus
                Tier = br.ReadByte();//0x0009	1 byte Weapon Tier
                CRIT = br.ReadByte();//0x000A	1 byte Crit Bonus
                Melee = br.ReadByte() == 0 ? false : true;//0x000B	1 byte Melee Weapon?
            }

            public static List<Weapons_Data> Read(BinaryReader br)
            {
                var ret = new List<Weapons_Data>(count);

                for (int i = 0; i < count; i++)
                {
                    Weapons_Data tmp = new Weapons_Data();
                    tmp.Read(br, i);
                    ret.Add(tmp);
                }
                return ret;
            }
        }
    }
}
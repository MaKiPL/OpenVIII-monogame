using System.IO;

namespace FF8
{
    public partial class Kernel_bin
    {
        /// <summary>
        /// Junctionable GFs Data
        /// </summary>
        /// <see cref="https://github.com/alexfilth/doomtrain/wiki/Junctionable-GFs"/>
        public class Junctionable_GFs_Data
        {
            public const int id = 2;
            public const int count = 16;
            //0x0000  2 bytes Offset to GF attack name
            //0x0002  2 bytes Offset to GF attack description
            public byte[] MagicID;             //0x0004  2 bytes[[Magic ID
            public byte Attack_type;           //0x0006  1 byte  Attack type
            public byte GF_power;              //0x0007  1 byte  GF power(used in damage formula)
            public byte[] Unknown0;            //0x0008  2 bytes Unknown
            public byte Attack_flags;          //0x000A  1 byte  Attack Flags
            public byte[] Unknown1;            //0x000B  2 bytes Unknown
            public byte Element;               //0x000D  1 byte[[Element
            public byte[] Statuses0;    //0x000E  2 bytes[[Statuses 0
            public byte[] Statuses1;    //0x0010  4 bytes[[Statuses 1
            public byte GFHP_modifier;         //0x0014  1 byte  GF HP Modifier(used in GF HP formula)
            public byte[] Unknown2;            //0x0015  6 bytes Unknown
            public byte Status_attack;         //0x001B  1 byte  Status attack enabler
            public byte[][] Ability;            
            //0x001C  1 byte[[Ability 1 Unlocker
            //0x001D  1 byte  Unknown
            //0x001E  1 byte[[Ability 1
            //0x001F  1 byte  Unknown
            //0x0020  1 byte[[Ability 2 Unlocker
            //0x0021  1 byte  Unknown
            //0x0022  1 byte[[Ability 2
            //0x0023  1 byte  Unknown
            //0x0024  1 byte[[Ability 3 Unlocker
            //0x0025  1 byte  Unknown
            //0x0026  1 byte[[Ability 3
            //0x0027  1 byte  Unknown
            //0x0028  1 byte[[Ability 4 Unlocker
            //0x0029  1 byte  Unknown
            //0x002A  1 byte[[Ability 4
            //0x002B  1 byte  Unknown
            //0x002C  1 byte[[Ability 5 Unlocker
            //0x002D  1 byte  Unknown
            //0x002E  1 byte[[Ability 5
            //0x002F  1 byte  Unknown
            //0x0030  1 byte[[Ability 6 Unlocker
            //0x0031  1 byte  Unknown
            //0x0032  1 byte[[Ability 6
            //0x0033  1 byte  Unknown
            //0x0034  1 byte[[Ability 7 Unlocker
            //0x0035  1 byte  Unknown
            //0x0036  1 byte[[Ability 7
            //0x0037  1 byte  Unknown
            //0x0038  1 byte[[Ability 8 Unlocker
            //0x0039  1 byte  Unknown
            //0x003A  1 byte[[Ability 8
            //0x003B  1 byte  Unknown
            //0x003C  1 byte[[Ability 9 Unlocker
            //0x003D  1 byte  Unknown
            //0x003E  1 byte[[Ability 9
            //0x003F  1 byte  Unknown
            //0x0040  1 byte[[Ability 10 Unlocker
            //0x0041  1 byte  Unknown
            //0x0042  1 byte[[Ability 10
            //0x0043  1 byte  Unknown
            //0x0044  1 byte[[Ability 11 Unlocker
            //0x0045  1 byte  Unknown
            //0x0046  1 byte[[Ability 11
            //0x0047  1 byte  Unknown
            //0x0048  1 byte[[Ability 12 Unlocker
            //0x0049  1 byte  Unknown
            //0x004A  1 byte[[Ability 12
            //0x004B  1 byte  Unknown
            //0x004C  1 byte[[Ability 13 Unlocker
            //0x004D  1 byte  Unknown
            //0x004E  1 byte[[Ability 13
            //0x004F  1 byte  Unknown
            //0x0050  1 byte[[Ability 14 Unlocker
            //0x0051  1 byte  Unknown
            //0x0052  1 byte[[Ability 14
            //0x0053  1 byte  Unknown
            //0x0054  1 byte[[Ability 15 Unlocker
            //0x0055  1 byte  Unknown
            //0x0056  1 byte[[Ability 15
            //0x0057  1 byte  Unknown
            //0x0058  1 byte[[Ability 16 Unlocker
            //0x0059  1 byte  Unknown
            //0x005A  1 byte[[Ability 16
            //0x005B  1 byte  Unknown
            //0x005C  1 byte[[Ability 17 Unlocker
            //0x005D  1 byte  Unknown
            //0x005E  1 byte[[Ability 17
            //0x005F  1 byte  Unknown
            //0x0060  1 byte[[Ability 18 Unlocker
            //0x0061  1 byte  Unknown
            //0x0062  1 byte[[Ability 18
            //0x0063  1 byte  Unknown
            //0x0064  1 byte[[Ability 19 Unlocker
            //0x0065  1 byte  Unknown
            //0x0066  1 byte[[Ability 19
            //0x0067  1 byte  Unknown
            //0x0068  1 byte[[Ability 20 Unlocker
            //0x0069  1 byte  Unknown
            //0x006A  1 byte[[Ability 20
            //0x006B  1 byte  Unknown
            //0x006C  1 byte[[Ability 21 Unlocker
            //0x006D  1 byte  Unknown
            //0x006E  1 byte[[Ability 21
            //0x006F  1 byte  Unknown
            public byte[] GF_Compatibility;
            //0x0070  1 byte  Quezacolt compatibility
            //0x0071  1 byte  Shiva compatibility
            //0x0072  1 byte  Ifrit compatibility
            //0x0073  1 byte  Siren compatibility
            //0x0074  1 byte  Brothers compatibility
            //0x0075  1 byte  Diablos compatibility
            //0x0076  1 byte  Carbuncle compatibility
            //0x0077  1 byte  Leviathan compatibility
            //0x0078  1 byte  Pandemona compatibility
            //0x0079  1 byte  Cerberus compatibility
            //0x007A  1 byte  Alexander compatibility
            //0x007B  1 byte  Doomtrain compatibility
            //0x007C  1 byte  Bahamut compatibility
            //0x007D  1 byte  Cactuar compatibility
            //0x007E  1 byte  Tonberry compatibility
            //0x007F  1 byte  Eden compatibility
            public byte[] Unknown3;            //0x0080  2 bytes Unknown
            public byte PowerMod;              //0x0082  1 byte  Power Mod(used in damage formula)
            public byte LevelMod;              //0x0083  1 byte  Level Mod(used in damage formula)
            internal void Read(BinaryReader br)
            {
                br.BaseStream.Seek(4, SeekOrigin.Current);

                MagicID = br.ReadBytes(2);             //0x0004  2 bytes[[Magic ID
                Attack_type = br.ReadByte();           //0x0006  1 byte  Attack type
                GF_power = br.ReadByte();              //0x0007  1 byte  GF power(used in damage formula)
                Unknown0 = br.ReadBytes(2);            //0x0008  2 bytes Unknown
                Attack_flags = br.ReadByte();          //0x000A  1 byte  Attack Flags
                Unknown1 = br.ReadBytes(2);            //0x000B  2 bytes Unknown
                Element = br.ReadByte();               //0x000D  1 byte[[Element
                Statuses0 = br.ReadBytes(2);           //0x000E  2 bytes[[Statuses 0
                Statuses1 = br.ReadBytes(4);           //0x0010  4 bytes[[Statuses 1
                GFHP_modifier = br.ReadByte();         //0x0014  1 byte  GF HP Modifier(used in GF HP formula)
                Unknown2 = br.ReadBytes(6);            //0x0015  6 bytes Unknown
                Status_attack = br.ReadByte();         //0x001B  1 byte  Status attack enabler
                Ability = new byte[21][];
                for (int i = 0; i < 21; i++)
                    Ability[i] = br.ReadBytes(2);
                GF_Compatibility = br.ReadBytes(16);
                Unknown3 = br.ReadBytes(2);            //0x0080  2 bytes Unknown
                PowerMod = br.ReadByte();              //0x0082  1 byte  Power Mod(used in damage formula)
                LevelMod = br.ReadByte();              //0x0083  1 byte  Level Mod(used in damage formula)
            }
        }
    }
}


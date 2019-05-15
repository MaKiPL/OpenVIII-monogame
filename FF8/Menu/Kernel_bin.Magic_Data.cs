using System.IO;

namespace FF8
{
    internal partial class Kernel_bin
    {
        /// <summary>
        /// Magic Data
        /// </summary>
        /// <see cref="https://github.com/alexfilth/doomtrain/wiki/Magic-data"/>
        internal struct Magic_Data
        {

            internal const int id = 1;
            internal const int count = 57;
            internal FF8String Name { get; private set; }
            internal FF8String Description { get; private set; }
            public override string ToString() => Name;
            //0x0000	2 bytes Offset to spell name
            //0x0002	2 bytes Offset to spell description
            internal ushort MagicID;     //0x0004	2 bytes Magic ID
            internal byte Unknown;       //0x0006  1 byte  Unknown
            internal byte Attack_type;   //0x0007  1 byte  Attack type
            internal byte Spellpower;    //0x0008  1 byte  Spell power(used in damage formula)
            internal byte Unknown2;      //0x0009  1 byte  Unknown
            internal byte Default_target;//0x000A  1 byte  Default_target
            internal byte Attack_flags;  //0x000B  1 byte  Attack Flags
            internal byte Draw_resist;   //0x000C  1 byte  Draw resist(how hard is the magic to draw)
            internal byte Hit_count;     //0x000D  1 byte  Hit count(works with meteor animation, not sure about others)
            internal byte Element;       //0x000E  1 byte Element
            internal byte Unknown3;      //0x000F  1 byte  Unknown
            internal byte[] Statuses1;   //0x0010  4 bytes Statuses 1
            internal byte[] Statuses0;   //0x0014  2 bytes Statuses 0
            internal byte Status_attack; //0x0016  1 byte  Status attack enabler
            internal byte HP_J;          //0x0017  1 byte  Characters HP junction value
            internal byte STR_J;         //0x0018  1 byte  Characters STR junction value
            internal byte VIT_J;         //0x0019  1 byte  Characters VIT junction value
            internal byte MAG_J;         //0x001A  1 byte  Characters MAG junction value
            internal byte SPR_J;         //0x001B  1 byte  Characters SPR junction value
            internal byte SPD_J;         //0x001C  1 byte  Characters SPD junction value
            internal byte EVA_J;         //0x001D  1 byte  Characters EVA junction value
            internal byte HIT_J;         //0x001E  1 byte  Characters HIT junction value
            internal byte LUCK_J;        //0x001F  1 byte  Characters LUCK junction value
            internal byte Elem_J_atk;    //0x0020  1 byte Characters J - Elem attack
            internal byte Elem_J_atk_val;//0x0021  1 byte  Characters J - Elem attack value
            internal byte Elem_J_def;    //0x0022  1 byte Characters J - Elem defense
            internal byte Elem_J_def_val;//0x0023  1 byte  Characters J - Elem defense value
            internal byte Stat_J_atk_val;//0x0024  1 byte  Characters J - Status attack value
            internal byte Stat_J_def_val;//0x0025  1 byte  Characters J - Status defense value
            internal byte[] Stat_J_atk;  //0x0026  2 bytes Characters J - Statuses Attack
            internal byte[] Stat_J_def;  //0x0028  2 bytes Characters J - Statuses Defend
            internal byte[] GF_Compatibility;
            //0x002A  1 byte  Quezacolt compatibility
            //0x002B  1 byte  Shiva compatibility
            //0x002C  1 byte  Ifrit compatibility
            //0x002D  1 byte  Siren compatibility
            //0x002E  1 byte  Brothers compatibility
            //0x002F  1 byte  Diablos compatibility
            //0x0030  1 byte  Carbuncle compatibility
            //0x0031  1 byte  Leviathan compatibility
            //0x0032  1 byte  Pandemona compatibility
            //0x0033  1 byte  Cerberus compatibility
            //0x0034  1 byte  Alexander compatibility
            //0x0035  1 byte  Doomtrain compatibility
            //0x0036  1 byte  Bahamut compatibility
            //0x0037  1 byte  Cactuar compatibility
            //0x0038  1 byte  Tonberry compatibility
            //0x0039  1 byte  Eden compatibility
            internal byte[] Unknown4;      //0x003A  2 bytes Unknown
            internal void Read(BinaryReader br, int i)
            {
                Name = Memory.Strings.Read(Strings.FileID.KERNEL, id, i * 2);
                Description = Memory.Strings.Read(Strings.FileID.KERNEL, id, i * 2 + 1);
                MagicID = br.ReadUInt16();
                Unknown = br.ReadByte();
                Attack_type = br.ReadByte();
                Spellpower = br.ReadByte();
                Unknown2 = br.ReadByte();
                Default_target = br.ReadByte();
                Attack_flags = br.ReadByte();
                Draw_resist = br.ReadByte();
                Hit_count = br.ReadByte();
                Element = br.ReadByte();
                Unknown3 = br.ReadByte();
                Statuses1 = br.ReadBytes(4);
                Statuses0 = br.ReadBytes(2);
                Status_attack = br.ReadByte();
                HP_J = br.ReadByte();
                STR_J = br.ReadByte();
                VIT_J = br.ReadByte();
                MAG_J = br.ReadByte();
                SPR_J = br.ReadByte();
                SPD_J = br.ReadByte();
                EVA_J = br.ReadByte();
                HIT_J = br.ReadByte();
                LUCK_J = br.ReadByte();
                Elem_J_atk = br.ReadByte();
                Elem_J_atk_val = br.ReadByte();
                Elem_J_def = br.ReadByte();
                Elem_J_def_val = br.ReadByte();
                Stat_J_atk_val = br.ReadByte();
                Stat_J_def_val = br.ReadByte();
                Stat_J_atk = br.ReadBytes(2);
                Stat_J_def = br.ReadBytes(2);
                GF_Compatibility = br.ReadBytes(16);
                Unknown4 = br.ReadBytes(2);
            }
        }
    }
}


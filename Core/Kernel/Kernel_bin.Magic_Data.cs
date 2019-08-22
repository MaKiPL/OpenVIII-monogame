using System.Collections.Generic;
using System.IO;

namespace OpenVIII
{
    public partial class Kernel_bin
    {
        /// <summary>
        /// Magic Data
        /// </summary>
        /// <see cref="https://github.com/alexfilth/doomtrain/wiki/Magic-data"/>
        public class Magic_Data
        {
            public const int id = 1;
            public const int count = 57;
            public FF8String Name { get; private set; }
            public FF8String Description { get; private set; }
            public byte ID { get; private set; }

            public override string ToString() => Name;

            //0x0000	2 bytes Offset to spell name
            //0x0002	2 bytes Offset to spell description
            public Magic_ID MagicID { get; private set; }     //0x0004	2 bytes Magic ID

            public byte Unknown { get; private set; }       //0x0006  1 byte  Unknown
            public Attack_Type Attack_type { get; private set; }   //0x0007  1 byte  Attack type
            public byte Spellpower { get; private set; }    //0x0008  1 byte  Spell power(used in damage formula)
            public byte Unknown2 { get; private set; }      //0x0009  1 byte  Unknown
            public Target Target { get; private set; }//0x000A  1 byte  Default_target
            public Attack_Flags Attack_flags { get; private set; }  //0x000B  1 byte  Attack Flags
            public byte Draw_resist { get; private set; }   //0x000C  1 byte  Draw resist(how hard is the magic to draw)
            public byte Hit_count { get; private set; }     //0x000D  1 byte  Hit count(works with meteor animation, not sure about others)
            public Element Element { get; private set; }       //0x000E  1 byte Element
            public byte Unknown3 { get; private set; }      //0x000F  1 byte  Unknown
            public Persistant_Statuses Statuses0 { get; private set; }   //0x0014  2 bytes Statuses 0
            public Battle_Only_Statuses Statuses1 { get; private set; }   //0x0010  4 bytes Statuses 1
            public byte Status_attack { get; private set; } //0x0016  1 byte  Status attack enabler\
            public IReadOnlyDictionary<Stat, byte> J_Val { get => _j_Val;  }
            //public byte HP_J;          //0x0017  1 byte  Characters HP junction value
            //public byte STR_J;         //0x0018  1 byte  Characters STR junction value
            //public byte VIT_J;         //0x0019  1 byte  Characters VIT junction value
            //public byte MAG_J;         //0x001A  1 byte  Characters MAG junction value
            //public byte SPR_J;         //0x001B  1 byte  Characters SPR junction value
            //public byte SPD_J;         //0x001C  1 byte  Characters SPD junction value
            //public byte EVA_J;         //0x001D  1 byte  Characters EVA junction value
            //public byte HIT_J;         //0x001E  1 byte  Characters HIT junction value
            //public byte LUCK_J;        //0x001F  1 byte  Characters LUCK junction value
            public Element EL_Atk { get; private set; }    //0x0020  1 byte Characters J - Elem attack
            //public byte J_Val[Kernel_bin.Stat.EL_Atk];//0x0021  1 byte  Characters J - Elem attack value
            public Element EL_Def { get; private set; }    //0x0022  1 byte Characters J - Elem defense
            //public byte J_Val[Kernel_bin.Stat.EL_Def_1];//0x0023  1 byte  Characters J - Elem defense value
            //public byte J_Val[Kernel_bin.Stat.ST_Atk];//0x0024  1 byte  Characters J - Status attack value
            //public byte J_Val[Kernel_bin.Stat.ST_Def_1];//0x0025  1 byte  Characters J - Status defense value
            public J_Statuses ST_Atk { get; private set; }  //0x0026  2 bytes Characters J - Statuses Attack
            public J_Statuses ST_Def { get; private set; }  //0x0028  2 bytes Characters J - Statuses Defend
            public byte[] GF_Compatibility { get; private set; }

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
            public byte[] Unknown4 { get; private set; }      //0x003A  2 bytes Unknown
            private Dictionary<Stat, byte> _j_Val;

            public void Read(BinaryReader br, int i)
            {
                Name = Memory.Strings.Read(Strings.FileID.KERNEL, id, i * 2);
                Description = Memory.Strings.Read(Strings.FileID.KERNEL, id, i * 2 + 1);
                ID = (byte)i;
                br.BaseStream.Seek(4, SeekOrigin.Current);
                MagicID = (Magic_ID)br.ReadUInt16();
                Unknown = br.ReadByte();
                Attack_type = (Attack_Type)br.ReadByte();
                Spellpower = br.ReadByte();
                Unknown2 = br.ReadByte();
                Target = (Target)br.ReadByte();
                Attack_flags = (Attack_Flags)br.ReadByte();
                Draw_resist = br.ReadByte();
                Hit_count = br.ReadByte();
                Element = (Element)br.ReadByte();
                Unknown3 = br.ReadByte();
                Statuses1 = (Battle_Only_Statuses)br.ReadUInt32();
                Statuses0 = (Persistant_Statuses)br.ReadUInt16();
                Status_attack = br.ReadByte();
                _j_Val = new Dictionary<Stat, byte>(6)
                {
                    { Stat.HP, br.ReadByte() },
                    { Stat.STR, br.ReadByte() },
                    { Stat.VIT, br.ReadByte() },
                    { Stat.MAG, br.ReadByte() },
                    { Stat.SPR, br.ReadByte() },
                    { Stat.SPD, br.ReadByte() },
                    { Stat.EVA, br.ReadByte() },
                    { Stat.HIT, br.ReadByte() },
                    { Stat.LUCK, br.ReadByte() }
                };
                EL_Atk = (Element)br.ReadByte();
                _j_Val.Add(Stat.EL_Atk, br.ReadByte());
                EL_Def = (Element)br.ReadByte();
                _j_Val.Add(Stat.EL_Def_1, br.ReadByte());
                _j_Val.Add(Stat.EL_Def_2, _j_Val[Stat.EL_Def_1]);
                _j_Val.Add(Stat.EL_Def_3, _j_Val[Stat.EL_Def_1]);
                _j_Val.Add(Stat.EL_Def_4, _j_Val[Stat.EL_Def_1]);
                _j_Val.Add(Stat.ST_Atk, br.ReadByte());
                _j_Val.Add(Stat.ST_Def_1, br.ReadByte());
                _j_Val.Add(Stat.ST_Def_2, _j_Val[Stat.ST_Def_1]);
                _j_Val.Add(Stat.ST_Def_3, _j_Val[Stat.ST_Def_1]);
                _j_Val.Add(Stat.ST_Def_4, _j_Val[Stat.ST_Def_1]);
                ST_Atk = (J_Statuses)br.ReadUInt16();
                ST_Def = (J_Statuses)br.ReadUInt16();
                GF_Compatibility = br.ReadBytes(16);
                Unknown4 = br.ReadBytes(2);
            }

            public uint totalStatVal(Stat stat)
            {
                if (stat < Stat.EL_Atk)
                    return J_Val[stat];
                else if (stat == Stat.EL_Atk)
                    return J_Val[stat] * EL_Atk.Count();
                else if (stat == Stat.ST_Atk)
                    return J_Val[stat] * ST_Atk.Count();
                else if (stat >= Stat.EL_Def_1 && stat <= Stat.EL_Def_4)
                    return J_Val[stat] * EL_Def.Count();
                else if (stat >= Stat.ST_Def_1 && stat <= Stat.ST_Def_4)
                    return J_Val[stat] * ST_Def.Count();
                return 0;
            }

            public static List<Magic_Data> Read(BinaryReader br)
            {
                var ret = new List<Magic_Data>(count);

                for (int i = 0; i < count; i++)
                {
                    var tmp = new Magic_Data();
                    tmp.Read(br, i);
                    ret.Add(tmp);
                }
                return ret;
            }
        }
    }
}
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;

namespace OpenVIII
{
    public partial class Kernel_bin
    {
        /// <summary>
        /// requirement to unlock abilities
        /// </summary>
        /// <see cref="https://github.com/alexfilth/doomtrain/blob/master/Doomtrain/MainForm.Designer.cs"/>
        public enum Unlocker:byte
        {
            None,
            GFLevel1,
            GFLevel2,
            GFLevel3,
            GFLevel4,
            GFLevel5,
            GFLevel6,
            GFLevel7,
            GFLevel8,
            GFLevel9,
            GFLevel10,
            GFLevel11,
            GFLevel12,
            GFLevel13,
            GFLevel14,
            GFLevel15,
            GFLevel16,
            GFLevel17,
            GFLevel18,
            GFLevel19,
            GFLevel20,
            GFLevel21,
            GFLevel22,
            GFLevel23,
            GFLevel24,
            GFLevel25,
            GFLevel26,
            GFLevel27,
            GFLevel28,
            GFLevel29,
            GFLevel30,
            GFLevel31,
            GFLevel32,
            GFLevel33,
            GFLevel34,
            GFLevel35,
            GFLevel36,
            GFLevel37,
            GFLevel38,
            GFLevel39,
            GFLevel40,
            GFLevel41,
            GFLevel42,
            GFLevel43,
            GFLevel44,
            GFLevel45,
            GFLevel46,
            GFLevel47,
            GFLevel48,
            GFLevel49,
            GFLevel50,
            GFLevel51,
            GFLevel52,
            GFLevel53,
            GFLevel54,
            GFLevel55,
            GFLevel56,
            GFLevel57,
            GFLevel58,
            GFLevel59,
            GFLevel60,
            GFLevel61,
            GFLevel62,
            GFLevel63,
            GFLevel64,
            GFLevel65,
            GFLevel66,
            GFLevel67,
            GFLevel68,
            GFLevel69,
            GFLevel70,
            GFLevel71,
            GFLevel72,
            GFLevel73,
            GFLevel74,
            GFLevel75,
            GFLevel76,
            GFLevel77,
            GFLevel78,
            GFLevel79,
            GFLevel80,
            GFLevel81,
            GFLevel82,
            GFLevel83,
            GFLevel84,
            GFLevel85,
            GFLevel86,
            GFLevel87,
            GFLevel88,
            GFLevel89,
            GFLevel90,
            GFLevel91,
            GFLevel92,
            GFLevel93,
            GFLevel94,
            GFLevel95,
            GFLevel96,
            GFLevel97,
            GFLevel98,
            GFLevel99,
            GFLevel100,
            Ability1,
            Ability2,
            Ability3,
            Ability4,
            Ability5,
            Ability6,
            Ability7,
            Ability8,
            Ability9,
            Ability10,
            Ability11,
            Ability12,
            Ability13,
            Ability14,
            Ability15,
            Ability16,
            Ability17,
            Ability18,
            Ability19,
            Ability20,
            Ability21,

        }
        /// <summary>
        /// Junctionable GFs Data
        /// </summary>
        /// <see cref="https://github.com/alexfilth/doomtrain/wiki/Junctionable-GFs"/>
        public class Junctionable_GFs_Data
        {
            public const int id = 2;
            public const int count = 16;
            private OrderedDictionary<Abilities, Unlocker> _ability;
            private Dictionary<GFs, decimal> _gF_Compatibility;

            public FF8String Name { get; private set; }
            public FF8String Description { get; private set; }
            public override string ToString() => Name;
            //0x0000  2 bytes Offset to GF attack name
            //0x0002  2 bytes Offset to GF attack description
            public Magic_ID MagicID { get; private set; }             //0x0004  2 bytes[[Magic ID
            public Attack_Type Attack_type { get; private set; }           //0x0006  1 byte  Attack type
            public byte GF_power { get; private set; }              //0x0007  1 byte  GF power(used in damage formula)
            public byte[] Unknown0 { get; private set; }            //0x0008  2 bytes Unknown
            public Attack_Flags Attack_flags { get; private set; }          //0x000A  1 byte  Attack Flags
            public byte[] Unknown1 { get; private set; }            //0x000B  2 bytes Unknown
            public Element Element { get; private set; }               //0x000D  1 byte[[Element
            public Persistent_Statuses Statuses0 { get; private set; }           //0x000E  2 bytes[[Statuses 0
            public Battle_Only_Statuses Statuses1 { get; private set; }           //0x0010  4 bytes[[Statuses 1
            public byte HP_MOD { get; private set; }         //0x0014  1 byte  GF HP Modifier(used in GF HP formula)
            public byte[] Unknown2_1 { get; private set; }            //0x0015  3 bytes Unknown
            public ushort EXPperLevel { get; private set; }             //0x18  1 byte *10;
            public byte[] Unknown2_2 { get; private set; }            //0x0019  2 bytes Unknown
            public byte Status_attack { get; private set; }         //0x001B  1 byte  Status attack enabler
            public OrderedDictionary<Abilities, Unlocker> Ability { get => _ability; }
            public IReadOnlyDictionary<GFs, decimal> GF_Compatibility { get => _gF_Compatibility; }
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
            public byte[] Unknown3 { get; private set; }            //0x0080  2 bytes Unknown
            public byte PowerMod { get; private set; }              //0x0082  1 byte  Power Mod(used in damage formula)
            public byte LevelMod { get; private set; }              //0x0083  1 byte  Level Mod(used in damage formula)
            public void Read(BinaryReader br, int i)
            {
                Name = Memory.Strings.Read(Strings.FileID.KERNEL, id, i * 2);
                Description = Memory.Strings.Read(Strings.FileID.KERNEL, id, i * 2 + 1);

                br.BaseStream.Seek(4, SeekOrigin.Current);

                MagicID = (Magic_ID)br.ReadUInt16();             //0x0004  2 bytes[[Magic ID
                Attack_type = (Attack_Type)br.ReadByte();           //0x0006  1 byte  Attack type
                GF_power = br.ReadByte();              //0x0007  1 byte  GF power(used in damage formula)
                Unknown0 = br.ReadBytes(2);            //0x0008  2 bytes Unknown
                Attack_flags = (Attack_Flags)(br.ReadByte());          //0x000A  1 byte  Attack Flags
                Unknown1 = br.ReadBytes(2);            //0x000B  2 bytes Unknown
                Element = (Element)br.ReadByte();               //0x000D  1 byte[[Element
                Statuses0 = (Persistent_Statuses)br.ReadUInt16();           //0x000E  2 bytes[[Statuses 0
                Statuses1 = (Battle_Only_Statuses)br.ReadUInt32();           //0x0010  4 bytes[[Statuses 1
                HP_MOD = br.ReadByte();         //0x0014  1 byte  GF HP Modifier(used in GF HP formula)
                Unknown2_1 = br.ReadBytes(3);            //0x0015  3 bytes Unknown
                EXPperLevel = (ushort)((br.ReadByte()) * 10); //0x0018
                Unknown2_2 = br.ReadBytes(2);            //0x0019  2 bytes Unknown
                Status_attack = br.ReadByte();         //0x001B  1 byte  Status attack enabler
                _ability = new OrderedDictionary<Abilities,Unlocker>(21);
                for (i = 0; i < 21; i++)
                {
                    Unlocker val = (Unlocker)br.ReadByte();
                    br.BaseStream.Seek(1, SeekOrigin.Current);
                    Abilities key = (Abilities)br.ReadUInt16();
                    _ability.Add(key, val);
                }
                _gF_Compatibility = new Dictionary<GFs, decimal>(16);
                for (i = 0; i < 16; i++)
                    _gF_Compatibility.Add((GFs)i, (100 - Convert.ToDecimal(br.ReadByte())) / 5); //doomtrain shows this is a decimal number. i got formula from code.
                Unknown3 = br.ReadBytes(2);            //0x0080  2 bytes Unknown
                PowerMod = br.ReadByte();              //0x0082  1 byte  Power Mod(used in damage formula)
                LevelMod = br.ReadByte();              //0x0083  1 byte  Level Mod(used in damage formula)
            }
            public static Dictionary<GFs, Junctionable_GFs_Data> Read(BinaryReader br)
            {
                var ret = new Dictionary<GFs, Junctionable_GFs_Data>(count);

                for (int i = 0; i < count; i++)
                {
                    var tmp = new Junctionable_GFs_Data();
                    tmp.Read(br, i);
                    ret[(GFs)i] = tmp;
                }
                return ret;
            }
        }
    }
}


using System.Collections.Generic;
using System.IO;

namespace FF8
{
    public partial class Kernel_bin
    {
        /// <summary>
        /// Command Abilities Data
        /// </summary>
        /// <see cref="https://github.com/alexfilth/doomtrain/wiki/Command-ability-data"/>
        public class Command_ability_data
        {
            public const int count = 12;
            public const int id = 10;

            public Abilities Ability { get; private set; }
            public Magic_ID MagicID { get; private set; }
            public byte[] Unknown0 { get; private set; }
            public Attack_Type Attack_Type { get; private set; }
            public byte Attack_Power { get; private set; }
            public Attack_Flags Attack_Flags { get; private set; }
            public byte Hit_Count { get; private set; }
            public Element Element { get; private set; }
            public byte Status_Attack { get; private set; }
            public Statuses0 Statuses0 { get; private set; }
            public Statuses1 Statuses1 { get; private set; }
            /// <summary>
            /// order is different so convertion is requried.
            /// </summary>
            private static Dictionary<int, Abilities> ConvertCommandAbilitytoAbility = new Dictionary<int, Abilities>
            {
                {0,Abilities.Recover},
                {1,Abilities.Revive},
                {2,Abilities.Treatment},
                {3,Abilities.MadRush},
                {4,Abilities.Doom},
                {5,Abilities.Absorb},
                {6,Abilities.LVDown},
                {7,Abilities.LVUp},
                {8,Abilities.Kamikaze},
                {9,Abilities.Devour},
                {10,Abilities.Card},
                {11,Abilities.Defend},
            };
            public static Dictionary<Abilities, Command_ability_data> Read(BinaryReader br)
            {
                var ret = new Dictionary<Abilities, Command_ability_data>(count);

                for (int i = 0; i < count; i++)
                {
                    var tmp = new Command_ability_data();
                    tmp.Read(br, ConvertCommandAbilitytoAbility[i]);
                    ret.Add(ConvertCommandAbilitytoAbility[i], tmp);
                }
                return ret;
            }

            public void Read(BinaryReader br, Abilities i)
            {
                Ability = i;
                MagicID = (Magic_ID)br.ReadUInt16();
                //0x0000  2 bytes Magic ID
                Unknown0 = br.ReadBytes(2);
                //0x0002  2 bytes Unknown
                Attack_Type = (Attack_Type)br.ReadByte();
                //0x0004  1 byte Attack type
                Attack_Power = br.ReadByte();
                //0x0005  1 byte Attack power
                Attack_Flags = (Attack_Flags)br.ReadByte();
                //0x0006  1 byte Attack flags
                Hit_Count = br.ReadByte();
                //0x0007  1 byte Hit Count
                Element = (Element)br.ReadByte();
                //0x0008  1 byte Element
                Status_Attack = br.ReadByte();
                //0x0009  1 byte Status attack enabler
                Statuses0 = (Statuses0)br.ReadUInt16();
                Statuses1 = (Statuses1)br.ReadUInt32();
                //0x000A  6 bytes Statuses
            }
        }
    }
}
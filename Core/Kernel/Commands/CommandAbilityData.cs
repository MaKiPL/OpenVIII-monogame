using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OpenVIII
{
    namespace Kernel
    {
        /// <summary>
        /// Command Abilities Data
        /// </summary>
        /// <see cref="https://github.com/alexfilth/doomtrain/wiki/Command-ability-data"/>
        public class CommandAbilityData
        {
            public const int Count = 12;
            public const int ID = 10;

            public Abilities Ability { get;  }
            public MagicID MagicID { get;  }
            public byte[] Unknown0 { get;  }
            public AttackType AttackType { get;  }
            public byte AttackPower { get;  }
            public AttackFlags AttackFlags { get;  }
            public byte HitCount { get;  }
            public Element Element { get;  }
            public byte StatusAttack { get;  }
            public Persistent_Statuses Statuses0 { get;  }
            public BattleOnlyStatuses Statuses1 { get;  }
            /// <summary>
            /// order is different so conversation is required.
            /// </summary>
            private static IEnumerable<Abilities> ConvertCommandAbilityToAbility { get; } = new []
            {
                Abilities.Recover,
                Abilities.Revive,
                Abilities.Treatment,
                Abilities.MadRush,
                Abilities.Doom,
                Abilities.Absorb,
                Abilities.LvDown,
                Abilities.LvUp,
                Abilities.Kamikaze,
                Abilities.Devour,
                Abilities.Card,
                Abilities.Defend
            };

            public static IReadOnlyDictionary<Abilities, CommandAbilityData> Read(BinaryReader br) =>
                ConvertCommandAbilityToAbility.ToDictionary(x => x, x => CreateInstance(br, x));
            
            private static CommandAbilityData CreateInstance(BinaryReader br, Abilities i)
            => new CommandAbilityData(br,i);
            private CommandAbilityData(BinaryReader br, Abilities i)
            {
                Ability = i;
                MagicID = (MagicID)br.ReadUInt16();
                //0x0000  2 bytes Magic ID
                Unknown0 = br.ReadBytes(2);
                //0x0002  2 bytes Unknown
                AttackType = (AttackType)br.ReadByte();
                //0x0004  1 byte Attack type
                AttackPower = br.ReadByte();
                //0x0005  1 byte Attack power
                AttackFlags = (AttackFlags)br.ReadByte();
                //0x0006  1 byte Attack flags
                HitCount = br.ReadByte();
                //0x0007  1 byte Hit Count
                Element = (Element)br.ReadByte();
                //0x0008  1 byte Element
                StatusAttack = br.ReadByte();
                //0x0009  1 byte Status attack enabler
                Statuses0 = (Persistent_Statuses)br.ReadUInt16();
                Statuses1 = (BattleOnlyStatuses)br.ReadUInt32();
                //0x000A  6 bytes Statuses
            }
        }
    }
}
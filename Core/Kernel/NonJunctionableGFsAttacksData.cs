using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OpenVIII
{
    namespace Kernel
    {
        /// <summary>
        /// Non-Junctionable GFs Attacks data
        /// </summary>
        /// <see cref="https://github.com/alexfilth/doomtrain/wiki/Non-junctionable-GF-attacks"/>
        public sealed class NonJunctionableGFsAttacksData
        {
            private static NonJunctionableGFsAttacksData CreateInstance(BinaryReader br, int i)
            {
                return new NonJunctionableGFsAttacksData(br, i);
            }

            #region Fields

            public const int Count = 16;
            public const int ID = 9;

            #endregion Fields

            #region Constructors

            private NonJunctionableGFsAttacksData(BinaryReader br, int i)
            {
                NonGFID = i;
                switch (i)
                {
                    case 11:
                        Angelo = Angelo.Rush;
                        break;

                    case 12:
                        Angelo = Angelo.Recover;
                        break;

                    case 13:
                        Angelo = Angelo.Reverse;
                        break;

                    case 14:
                        Angelo = Angelo.Search;
                        break;

                    default:
                        Angelo = Angelo.None;
                        break;
                }
                //0x0000	2 bytes Offset to GF attack name
                Name = Memory.Strings.Read(Strings.FileID.Kernel, ID, i);
                br.BaseStream.Seek(2, SeekOrigin.Current);
                //0x0002	2 bytes Magic ID(decides what animation to play)
                MagicID = (MagicID)br.ReadUInt16();
                //0x0004	1 byte Attack type
                AttackType = (AttackType)br.ReadByte();
                //0x0005	1 byte GF power(used in damage formula)
                GFPower = br.ReadByte();
                //0x0006	1 byte Status attack enabler
                StatusAttack = br.ReadByte();
                //0x0007	1 byte Unknown
                Unknown0 = br.ReadByte();
                //0x0008	1 byte Status flags ?
                StatusFlags = br.ReadByte();
                //0x0009	2 bytes Unknown
                Unknown1 = br.ReadBytes(2);
                //0x000B	1 byte Element
                Element = (Element)br.ReadByte();
                //0x000C	1 byte Status 1
                //0x000D	1 byte Status 2
                //0x000E	1 byte Status 3
                //0x000F	1 byte Status 4
                Statuses1 = (BattleOnlyStatuses)br.ReadUInt32();
                //0x0010	1 byte Status 5
                Statuses0 = (PersistentStatuses)br.ReadUInt16();
                //0x0011	1 byte Unknown
                Unknown2 = br.ReadByte();
                //0x0012	1 byte Power Mod(used in damage formula)
                PowerMod = br.ReadByte();
                //0x0013	1 byte Level Mod(used in damage formula)
                LevelMod = br.ReadByte();
            }

            #endregion Constructors

            #region Properties

            public Angelo Angelo { get; }

            /// <summary>
            ///0x0004	1 byte Attack type
            /// </summary>
            public AttackType AttackType { get; }

            /// <summary>
            ///0x000B	1 byte Element
            /// </summary>
            /// <remarks>
            /// //0x00 - Non-Elemental
            /// //0x01 - Fire
            /// //0x02 - Ice
            /// //0x04 - Thunder
            /// //0x08 - Earth
            /// //0x10 - Poison
            /// //0x20 - Wind
            /// //0x40 - Water
            /// //0x80 - Holy
            /// </remarks>
            public Element Element { get; }

            /// <summary>
            ///0x0005	1 byte GF power(used in damage formula)
            /// </summary>
            public byte GFPower { get; }

            /// <summary>
            /// //0x0013	1 byte Level Mod(used in damage formula)
            /// </summary>
            public byte LevelMod { get; }

            /// <summary>
            ///0x0002	2 bytes Magic ID(decides what animation to play)
            /// </summary>
            public MagicID MagicID { get; }

            /// <summary>
            ///0x0000	2 bytes Offset to GF attack name
            /// </summary>
            public FF8String Name { get; }

            public int NonGFID { get; }

            /// <summary>
            /// //0x0012	1 byte Power Mod(used in damage formula)
            /// </summary>
            public byte PowerMod { get; }

            /// <summary>
            ///0x0006	1 byte Status attack enabler
            /// </summary>
            public byte StatusAttack { get; }

            /// <summary>
            /// //0x0010	1 byte Status 5
            /// </summary>
            /// <remarks>
            /// //0x00 - None
            /// //0x01 - Death
            /// //0x02 - Poison
            /// //0x04 - Petrify
            /// //0x08 - Darkness
            /// //0x10 - Silence
            /// //0x20 - Berserk
            /// //0x40 - Zombie
            /// //0x80 - ???
            /// </remarks>
            public PersistentStatuses Statuses0 { get; }

            /// <summary>
            ///0x000C	1 byte Status 1
            /// </summary>
            /// <remarks>
            /// //0x00 - None
            /// //0x01 - Sleep
            /// //0x02 - Haste
            /// //0x04 - Slow
            /// //0x08 - Stop
            /// //0x10 - Regen
            /// //0x20 - Protect
            /// //0x40 - Shell
            /// //0x80 - Reflect
            ///
            /// //0x000D	1 byte Status 2
            /// //0x00 - None
            /// //0x01 - Aura
            /// //0x02 - Curse
            /// //0x04 - Doom
            /// //0x08 - Invincible
            /// //0x10 - Petrifying
            /// //0x20 - Float
            /// //0x40 - Confusion
            /// //0x80 - Drain
            /// //0x000E	1 byte Status 3
            ///
            /// //0x00 - None
            /// //0x01 - Eject
            /// //0x02 - Double
            /// //0x04 - Triple
            /// //0x08 - Defend
            /// //0x10 - ???
            /// //0x20 - ???
            /// //0x40 - ???
            /// //0x80 - ???
            /// //0x000F	1 byte Status 4
            ///
            /// //0x00 - None
            /// //0x01 - Vit0
            /// //0x02 - ???
            /// //0x04 - ???
            /// //0x08 - ???
            /// //0x10 - ???
            /// //0x20 - ???
            /// //0x40 - ???
            /// //0x80 - ???
            /// </remarks>
            public BattleOnlyStatuses Statuses1 { get; }

            /// <summary>
            ///0x0008	1 byte Status flags ?
            /// </summary>
            public byte StatusFlags { get; }

            /// <summary>
            ///0x0007	1 byte Unknown
            /// </summary>
            public byte Unknown0 { get; }

            /// <summary>
            ///0x0009	2 bytes Unknown
            /// </summary>
            public byte[] Unknown1 { get; }

            /// <summary>
            /// //0x0011	1 byte Unknown
            /// </summary>
            public byte Unknown2 { get; }

            #endregion Properties

            #region Methods

            public static IReadOnlyList<NonJunctionableGFsAttacksData> Read(BinaryReader br)
                => Enumerable.Range(0, Count).Select(x => CreateInstance(br, x)).ToList()
                    .AsReadOnly();

            public override string ToString() => Name?.Value_str ?? base.ToString();

            #endregion Methods
        }
    }
}
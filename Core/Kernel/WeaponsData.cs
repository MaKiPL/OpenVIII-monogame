using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OpenVIII
{
    namespace Kernel
    {
        /// <summary>
        /// Weapon Data
        /// </summary>
        /// <see cref="https://github.com/alexfilth/doomtrain/wiki/Weapons"/>
        public sealed class WeaponsData
        {
            #region Fields

            public const int Count = 33;
            public const int ID = 4;
            private static byte _counter;

            private static Characters _lastCharacter = Characters.Blank;

            #endregion Fields

            #region Constructors

            private WeaponsData(BinaryReader br, int stringID = 0)
            {
                if (br == null) throw new ArgumentNullException(nameof(br));
                if (stringID < 0) throw new ArgumentOutOfRangeException(nameof(stringID));
                WeaponID = checked((byte)stringID);
                Name = Memory.Strings.Read(Strings.FileID.Kernel, ID, stringID);
                br.BaseStream.Seek(2, SeekOrigin.Current);
                Renzokuken = (Renzokeken_Finisher)br.ReadByte(); //0x0002	1 byte Renzokuken finishers
                Unknown0 = br.ReadByte(); //0x0003	1 byte Unknown
                Character = (Characters)br.ReadByte();//0x0004	1 byte Character BattleID
                if (_lastCharacter != Character)
                {
                    AltID = _counter = 0;
                    _lastCharacter = Character;
                }
                else AltID = ++_counter;
                AttackType = (AttackType)br.ReadByte();//0x0005	1 bytes Attack Type
                AttackPower = br.ReadByte();//0x0006	1 byte Attack Power
                Hit = br.ReadByte();//0x0007	1 byte Attack Parameter
                STR = br.ReadByte();//0x0008	1 byte STR Bonus
                Tier = br.ReadByte();//0x0009	1 byte Weapon Tier
                Critical = br.ReadByte();//0x000A	1 byte Critical Bonus
                Melee = br.ReadByte() != 0;//0x000B	1 byte Melee Weapon?
            }

            #endregion Constructors

            #region Properties

            public byte AltID { get; }

            /// <summary>
            ///0x0006	1 byte Attack Power
            /// </summary>
            public byte AttackPower { get; }

            /// <summary>
            ///0x0005	1 bytes Attack Type
            /// </summary>
            public AttackType AttackType { get; }

            /// <summary>
            ///0x0004	1 byte Character BattleID
            /// </summary>
            public Characters Character { get; }

            /// <summary>
            ///0x000A	1 byte Critical Bonus
            /// </summary>
            public byte Critical { get; }

            /// <summary>
            ///0x0007	1 byte Attack Parameter
            /// </summary>
            public byte Hit { get; }

            /// <summary>
            ///0x000B	1 byte Melee Weapon?
            /// </summary>
            public bool Melee { get; }

            /// <summary>
            ///0x0000	2 bytes Offset to weapon name
            /// </summary>
            public FF8String Name { get; }

            /// <summary>
            ///0x0002	1 byte Renzokuken finishers
            /// </summary>
            public Renzokeken_Finisher Renzokuken { get; }

            /// <summary>
            ///0x0008	1 byte STR Bonus
            /// </summary>
            public byte STR { get; }

            /// <summary>
            ///0x0009	1 byte Weapon Tier
            /// </summary>
            public byte Tier { get; }

            /// <summary>
            ///0x0003	1 byte Unknown
            /// </summary>
            public byte Unknown0 { get; }

            public byte WeaponID { get; }

            #endregion Properties

            #region Methods

            public static IReadOnlyList<WeaponsData> Read(BinaryReader br)
                => Enumerable.Range(0, Count).Select(x => new WeaponsData(br, x)).ToList();

            public override string ToString() => Name;

            #endregion Methods
        }
    }
}
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OpenVIII
{
    namespace Kernel
    {
        #region Classes

        /// <summary>
        /// Stat Percentage Increasing Abilities Data
        /// </summary>
        /// <see cref="https://github.com/alexfilth/doomtrain/wiki/Stat-percentage-increasing-abilities"/>
        public class StatPercentageAbilities : EquippableAbility
        {
            #region Fields

            /// <summary>
            /// Section Count
            /// </summary>
            public const int Count = 19;

            /// <summary>
            /// Icon for this type.
            /// </summary>
            public new const Icons.ID Icon = Icons.ID.Ability_Character;

            /// <summary>
            /// Icon for this type.
            /// </summary>
            public const int ID = 13;

            #endregion Fields

            #region Constructors

            private StatPercentageAbilities(FF8String name, FF8String description, byte ap, Stat stat,
                                                    byte value, byte unknown0)
                : base(name, description, ap, Icon)
                => (Stat, Value, Unknown0) = (stat, value, unknown0);

            #endregion Constructors

            #region Properties

            /// <summary>
            /// Stat increased
            /// </summary>
            public Stat Stat { get; }

            /// <summary>
            /// Unknown byte
            /// </summary>
            public byte Unknown0 { get; }

            /// <summary>
            /// AmountIncreased
            /// </summary>
            public byte Value { get; }

            #endregion Properties

            #region Methods

            public static Dictionary<Abilities, StatPercentageAbilities> Read(BinaryReader br)

                => Enumerable.Range(0, Count)
                    .ToDictionary(i => (Abilities)(i + (int)Abilities.HP20), i => CreateInstance(br, i));

            private static StatPercentageAbilities CreateInstance(BinaryReader br, int i)
            {
                //0x0000	2 bytes Offset
                FF8StringReference name = Memory.Strings.Read(Strings.FileID.KERNEL, ID, i * 2);
                //0x0002	2 bytes Offset
                FF8StringReference description = Memory.Strings.Read(Strings.FileID.KERNEL, ID, i * 2 + 1);
                br.BaseStream.Seek(4, SeekOrigin.Current);
                //0x0004  1 byte
                byte ap = br.ReadByte();
                //0x0005  1 byte
                Stat stat = (Stat)br.ReadByte();
                //0x0006  1 byte
                byte value = br.ReadByte();
                //0x0007  1 byte
                byte unknown0 = br.ReadByte();
                return new StatPercentageAbilities(name, description, ap, stat, value, unknown0);
            }

            #endregion Methods
        }

        #endregion Classes
    }
}
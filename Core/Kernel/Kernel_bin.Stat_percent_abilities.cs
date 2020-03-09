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
        public class StatPercentageAbilities : IEquippableAbility
        {
            #region Fields

            /// <summary>
            /// Section Count
            /// </summary>
            public const int Count = 19;

            /// <summary>
            /// Icon for this type.
            /// </summary>
            public const int ID = 13;

            #endregion Fields

            #region Constructors

            private StatPercentageAbilities(BinaryReader br, int i)
            {
                //0x0000	2 bytes Offset
                Name = Memory.Strings.Read(Strings.FileID.Kernel, ID, i * 2);
                //0x0002	2 bytes Offset
                Description = Memory.Strings.Read(Strings.FileID.Kernel, ID, i * 2 + 1);
                br.BaseStream.Seek(4, SeekOrigin.Current);
                //0x0004  1 byte
                AP = br.ReadByte();
                //0x0005  1 byte
                Stat = (Stat)br.ReadByte();
                //0x0006  1 byte
                Value = br.ReadByte();
                //0x0007  1 byte
                Unknown0 = br.ReadByte();
            }

            #endregion Constructors

            #region Properties

            public byte AP { get; }

            public FF8String Description { get; }

            public Icons.ID Icon { get; } = Icons.ID.Ability_Character;

            public FF8String Name { get; }

            public byte Palette { get; } = Ability.DefaultPalette;

            /// <summary>
            /// Stat increased
            /// </summary>
            public Stat Stat { get; }

            public Stat Stat { get; private set; }
            public byte Value { get; private set; }
            public byte Unknown0 { get; private set; }

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
                => new StatPercentageAbilities(br, i);

            public static Dictionary<Abilities, Stat_percent_abilities> Read(BinaryReader br)
            {
                Dictionary<Abilities, Stat_percent_abilities> ret = new Dictionary<Abilities, Stat_percent_abilities>(count);

                for (int i = 0; i < count; i++)
                {
                    Stat_percent_abilities tmp = new Stat_percent_abilities();
                    tmp.Read(br, i);
                    ret[(Abilities)(i+ (int)Abilities.HP_20)] = tmp;
                }
                return ret;
            }
        }

        #endregion Classes
    }
}

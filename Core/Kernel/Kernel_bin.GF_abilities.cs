using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OpenVIII
{
    namespace Kernel
    {
        #region Classes

        /// <summary>
        /// GF Abilities Data
        /// </summary>
        /// <see cref="https://github.com/alexfilth/doomtrain/wiki/GF-abilities"/>
        public sealed class GFAbilities : EquippableAbility
        {
            #region Fields

            /// <summary>
            /// Section Count
            /// </summary>
            public const int Count = 9;

            /// <summary>
            /// Icon for this type.
            /// </summary>
            public new const Icons.ID Icon = Icons.ID.Ability_GF;

            /// <summary>
            /// Section ID
            /// </summary>
            public const int ID = 16;

            #endregion Fields

            #region Constructors

            private GFAbilities(FF8String name, FF8String description, byte ap, bool boost, Stat stat, byte value) :
                base(name, description, ap, Icon) => (Boost, Stat, Value) = (boost, stat, value);

            #endregion Constructors

            #region Properties

            /// <summary>
            /// Boost Enable
            /// </summary>
            public bool Boost { get; }

            /// <summary>
            /// Stat to increase
            /// </summary>
            public Stat Stat { get; }

            /// <summary>
            /// Increase value
            /// </summary>
            public byte Value { get; }

            #endregion Properties

            #region Methods

            public static Dictionary<Abilities, GFAbilities> Read(BinaryReader br)

                => Enumerable.Range(0, Count)
                    .ToDictionary(i => (Abilities)(i + (int)Abilities.SumMag10), i => CreateInstance(br, i));

            private static GFAbilities CreateInstance(BinaryReader br, int i)
            {
                //0x0000	2 bytes Offset to name
                FF8StringReference name = Memory.Strings.Read(Strings.FileID.KERNEL, ID, i * 2);
                //0x0002	2 bytes Offset to description
                FF8StringReference description = Memory.Strings.Read(Strings.FileID.KERNEL, ID, i * 2 + 1);
                br.BaseStream.Seek(4, SeekOrigin.Current);
                //0x0004 1 byte AP needed to learn the ability
                byte ap = br.ReadByte();
                //0x0005  1 byte
                bool boost = br.ReadByte() != 0;
                //0x0006  1 byte
                Stat stat = (Stat)br.ReadByte();
                //0x0007  1 byte
                byte value = br.ReadByte();
                return new GFAbilities(name, description, ap, boost, stat, value);
            }

            #endregion Methods
        }

        #endregion Classes
    }
}
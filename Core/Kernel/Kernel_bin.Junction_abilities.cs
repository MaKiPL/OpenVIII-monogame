using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OpenVIII
{
    namespace Kernel
    {
        #region Classes

        /// <summary>
        /// Junction Abilities Data
        /// </summary>
        /// <see cref="https://github.com/alexfilth/doomtrain/wiki/Junction-abilities"/>
        public sealed class JunctionAbilities : Ability
        {
            #region Fields

            /// <summary>
            /// Section Count
            /// </summary>
            public const int Count = 20;

            /// <summary>
            /// Section ID
            /// </summary>
            public const int ID = 11;

            /// <summary>
            /// Icon for this type.
            /// </summary>
            private new const Icons.ID Icon = Icons.ID.Ability_Junction;

            #endregion Fields

            #region Constructors

            private JunctionAbilities(FF8String name, FF8String description, byte ap, JunctionAbilityFlags jFlags)
                        : base(name, description, ap, Icon)
                            => JFlags = jFlags;

            #endregion Constructors

            #region Properties

            /// <summary>
            /// Flags related to what abilities are enabled
            /// </summary>
            public JunctionAbilityFlags JFlags { get; }

            #endregion Properties

            #region Methods

            public static Dictionary<Abilities, JunctionAbilities> Read(BinaryReader br)

                => Enumerable.Range(0, Count)
                    .ToDictionary(i => (Abilities)i, i => CreateInstance(br, i));

            private static JunctionAbilities CreateInstance(BinaryReader br, int i)
            {
                //0x0000	2 bytes
                FF8StringReference name = Memory.Strings.Read(Strings.FileID.KERNEL, ID, i * 2);
                //0x0002	2 bytes
                FF8StringReference description = Memory.Strings.Read(Strings.FileID.KERNEL, ID, i * 2 + 1);
                br.BaseStream.Seek(4, SeekOrigin.Current);
                //0x0004  1 byte
                byte ap = br.ReadByte();
                //0x0005  3 byte J_Flag
                byte[] tmp = br.ReadBytes(3);
                JunctionAbilityFlags jFlags = (JunctionAbilityFlags)(tmp[2] << 16 | tmp[1] << 8 | tmp[0]);
                return new JunctionAbilities(name, description, ap, jFlags);
            }

            #endregion Methods
        }

        #endregion Classes
    }
}
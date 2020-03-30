using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OpenVIII
{
    namespace Kernel
    {
        #region Classes

        /// <summary>
        /// Characters Abilities Data
        /// </summary>
        /// <see cref="https://github.com/alexfilth/doomtrain/wiki/Character-abilities"/>
        public sealed class CharacterAbilities : EquippableAbility
        {
            #region Fields

            /// <summary>
            /// Section Count
            /// </summary>
            public const int Count = 20;

            /// <summary>
            /// Section ID
            /// </summary>
            public const int ID = 14;

            /// <summary>
            /// Icon for this type.
            /// </summary>
            public new const Icons.ID Icon = Icons.ID.Ability_Character2;

            #endregion Fields

            #region Constructors

            private CharacterAbilities
                        (FF8String name, FF8String description, byte ap, CharacterAbilityFlags flags) :
                base(name, description, ap, Icon) => Flags = flags;

            #endregion Constructors

            #region Properties

            /// <summary>
            /// Ability Flags that are enabled by this
            /// </summary>
            public CharacterAbilityFlags Flags { get; }

            #endregion Properties

            #region Methods

            public static Dictionary<Abilities, CharacterAbilities> Read(BinaryReader br)

                => Enumerable.Range(0, Count)
                    .ToDictionary(i => (Abilities)(i + (int)Abilities.Mug), i => CreateInstance(br, i));

            private static CharacterAbilities CreateInstance(BinaryReader br, int i)
            {
                //0x0000	2 bytes
                FF8StringReference name = Memory.Strings.Read(Strings.FileID.KERNEL, ID, i * 2);
                //0x0002	2 bytes
                FF8StringReference description = Memory.Strings.Read(Strings.FileID.KERNEL, ID, i * 2 + 1);
                br.BaseStream.Seek(4, SeekOrigin.Current);
                //0x0004  1 byte
                byte ap = br.ReadByte();
                //0x0005  3 byte
                byte[] tmp = br.ReadBytes(3);
                CharacterAbilityFlags flags =
                    (CharacterAbilityFlags)(tmp[2] << (16) | tmp[1] << (8) | tmp[0]);

                return new CharacterAbilities(name, description, ap, flags);
            }

            #endregion Methods
        }

        #endregion Classes
    }
}
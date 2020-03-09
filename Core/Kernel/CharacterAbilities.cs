using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OpenVIII.Kernel
{
    /// <summary>
    /// Characters Abilities Data
    /// </summary>
    /// <see cref="https://github.com/alexfilth/doomtrain/wiki/Character-abilities"/>
    public sealed class CharacterAbilities : IEquippableAbility
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

        #endregion Fields

        /// <summary>
        /// Icon for this type.
        /// </summary>

        #region Constructors

        private CharacterAbilities(BinaryReader br, int i)
        {
            //0x0000	2 bytes
            Name = Memory.Strings.Read(Strings.FileID.Kernel, ID, i * 2);
            //0x0002	2 bytes
            Description = Memory.Strings.Read(Strings.FileID.Kernel, ID, i * 2 + 1);
            br.BaseStream.Seek(4, SeekOrigin.Current);
            //0x0004  1 byte
            AP = br.ReadByte();
            //0x0005  3 byte
            byte[] tmp = br.ReadBytes(3);
            Flags = (CharacterAbilityFlags)(tmp[2] << (16) | tmp[1] << (8) | tmp[0]);
        }

        #endregion Constructors

        #region Properties

        public byte AP { get; }

        public FF8String Description { get; }

        /// <summary>
        /// Ability Flags that are enabled by this
        /// </summary>
        public CharacterAbilityFlags Flags { get; }

        public Icons.ID Icon { get; } = Icons.ID.Ability_Character2;

        public FF8String Name { get; }

        public byte Palette { get; } = Ability.DefaultPalette;

        #endregion Properties

        #region Methods

        public static IReadOnlyDictionary<Abilities, CharacterAbilities> Read(BinaryReader br) =>
                Enumerable.Range(0, Count)
                    .ToDictionary(i => (Abilities)(i + (int)Abilities.Mug), i => CreateInstance(br, i));

        public override string ToString() => Name;

        private static CharacterAbilities CreateInstance(BinaryReader br, int i)
                    => new CharacterAbilities(br, i);

        #endregion Methods
    }
}
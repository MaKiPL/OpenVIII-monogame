using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OpenVIII.Kernel
{
    /// <summary>
    /// Menu Abilities Data
    /// </summary>
    /// <see cref="https://github.com/alexfilth/doomtrain/wiki/Menu-abilities"/>
    public sealed class MenuAbilities : IAbility
    {
        #region Fields

        /// <summary>
        /// Section Count
        /// </summary>
        public const int Count = 24;

        /// <summary>
        /// Section ID
        /// </summary>
        public const int ID = 17;

        #endregion Fields

        #region Constructors

        private MenuAbilities(BinaryReader br, int i)
        {
            //0x0000	2 bytes Offset
            Name = Memory.Strings.Read(Strings.FileID.Kernel, ID, i * 2);
            //0x0002	2 bytes Offset
            Description = Memory.Strings.Read(Strings.FileID.Kernel, ID, i * 2 + 1);
            br.BaseStream.Seek(4, SeekOrigin.Current);
            //0x0004  1 byte
            AP = br.ReadByte();
            //0x0005  1 byte
            Index = br.ReadByte();
            //0x0006  1 byte
            Start = br.ReadByte();
            //0x0007  1 byte
            End = br.ReadByte();
        }

        #endregion Constructors

        #region Properties

        public byte AP { get; }

        public FF8String Description { get; }

        /// <summary>
        /// End offset
        /// </summary>
        public byte End { get; }

        public Icons.ID Icon { get; } = Icons.ID.Ability_Menu;

        /// <summary>
        /// <para>Index to m00X files in menu.fs</para>
        /// <para>first 3 sections are treated as special cases</para>
        /// </summary>
        public byte Index { get; }

        public FF8String Name { get; }

        public byte Palette { get; } = Ability.DefaultPalette;

        /// <summary>
        /// Start offset
        /// </summary>
        public byte Start { get; }

        #endregion Properties

        #region Methods

        public static IReadOnlyDictionary<Abilities, MenuAbilities> Read(BinaryReader br)

                => Enumerable.Range(0, Count)
                    .ToDictionary(i => (Abilities)(i + (int)Abilities.Haggle), i => CreateInstance(br, i));

        private static MenuAbilities CreateInstance(BinaryReader br, int i)
        => new MenuAbilities(br, i);

        #endregion Methods
    }
}
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OpenVIII
{
    namespace Kernel
    {
        #region Classes

        /// <summary>
        /// Menu Abilities Data
        /// </summary>
        /// <see cref="https://github.com/alexfilth/doomtrain/wiki/Menu-abilities"/>
        public class MenuAbilities : Ability
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

            /// <summary>
            /// Icon for this type.
            /// </summary>
            private new const Icons.ID Icon = Icons.ID.Ability_Menu;

            #endregion Fields

            #region Constructors

            private MenuAbilities(FF8String name, FF8String description, byte ap, byte index, byte start, byte end)
                            : base(name, description, ap, Icon)
                            => (Index, Start, End) = (index, start, end);

            #endregion Constructors

            #region Properties

            /// <summary>
            /// End offset
            /// </summary>
            public byte End { get; }

            /// <summary>
            /// <para>Index to m00X files in menu.fs</para>
            /// <para>first 3 sections are treated as special cases</para>
            /// </summary>
            public byte Index { get; }

            /// <summary>
            /// Start offset
            /// </summary>
            public byte Start { get; }

            #endregion Properties

            #region Methods

            public static Dictionary<Abilities, MenuAbilities> Read(BinaryReader br)

                => Enumerable.Range(0, Count)
                    .ToDictionary(i => (Abilities)(i + (int)Abilities.Haggle), i => CreateInstance(br, i));

            private static MenuAbilities CreateInstance(BinaryReader br, int i)
            {
                //0x0000	2 bytes Offset
                FF8StringReference name = Memory.Strings.Read(Strings.FileID.KERNEL, ID, i * 2);
                //0x0002	2 bytes Offset
                FF8StringReference description = Memory.Strings.Read(Strings.FileID.KERNEL, ID, i * 2 + 1);
                br.BaseStream.Seek(4, SeekOrigin.Current);
                //0x0004  1 byte
                byte ap = br.ReadByte();
                //0x0005  1 byte
                byte index = br.ReadByte();
                //0x0006  1 byte
                byte start = br.ReadByte();
                //0x0007  1 byte
                byte end = br.ReadByte();
                return new MenuAbilities(name, description, ap, index, start, end);
            }

            #endregion Methods
        }

        #endregion Classes
    }
}
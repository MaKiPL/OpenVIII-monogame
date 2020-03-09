using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OpenVIII
{
    namespace Kernel
    {
        #region Classes

        /// <summary>
        /// Party Abilities Data
        /// </summary>
        /// <see cref="https://github.com/alexfilth/doomtrain/wiki/Party-abilities"/>
        public sealed class PartyAbilities : IEquippableAbility
        {
            #region Fields

            /// <summary>
            /// Section Count
            /// </summary>
            public const int Count = 5;

            /// <summary>
            /// Section ID
            /// </summary>
            public const int ID = 15;

            #endregion Fields

            #region Constructors

            private PartyAbilities(BinaryReader br, int i)
            {
                //0x0000	2 bytes Offset
                Name = Memory.Strings.Read(Strings.FileID.Kernel, ID, i * 2);
                //0x0002	2 bytes Offset
                Description = Memory.Strings.Read(Strings.FileID.Kernel, ID, i * 2 + 1);
                br.BaseStream.Seek(4, SeekOrigin.Current);
                //0x0004  1 byte
                AP = br.ReadByte();
                //0x0005  1 byte //TODO reverse flags. probably a unique value for each ability
                Flags = new BitArray(br.ReadBytes(1)).Cast<bool>().ToList();
                //0x0006  2 byte
                Unknown0 = br.ReadBytes(2);
            }

            #endregion Constructors

            #region Properties

            public byte AP { get; }

            public FF8String Description { get; }

            /// <summary>
            /// Unknown flags
            /// </summary>
            public IReadOnlyList<bool> Flags { get; }

            public Icons.ID Icon { get; } = Icons.ID.Ability_Party;

            public FF8String Name { get; }

            public byte Palette { get; } = Ability.DefaultPalette;

            /// <summary>
            /// Unknown bytes
            /// </summary>
            public byte[] Unknown0 { get; }

            #endregion Properties

            #region Methods

            public static Dictionary<Abilities, PartyAbilities> Read(BinaryReader br)

                => Enumerable.Range(0, Count)
                    .ToDictionary(i => (Abilities)(i + (int)Abilities.Alert), i => CreateInstance(br, i));

            private static PartyAbilities CreateInstance(BinaryReader br, int i)
                => new PartyAbilities(br, i);

            #endregion Methods
        }

        #endregion Classes
    }
}

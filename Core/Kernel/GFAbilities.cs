using System.Collections.Generic;
using System.IO;

namespace OpenVIII
{
    public partial class Kernel_bin
    {
        /// <summary>
        /// GF Abilities Data
        /// </summary>
        /// <see cref="https://github.com/alexfilth/doomtrain/wiki/GF-abilities"/>
        public sealed class GFAbilities : IEquippableAbility
        {
            #region Fields

            /// <summary>
            /// Section Count
            /// </summary>
            public const int Count = 9;

            /// <summary>
            /// Section ID
            /// </summary>
            public const int ID = 16;

            #endregion Fields

            #region Constructors

            private GFAbilities(BinaryReader br, int i)
            {
                //0x0000	2 bytes Offset to name
                Name = Memory.Strings.Read(Strings.FileID.Kernel, ID, i * 2);
                //0x0002	2 bytes Offset to description
                Description = Memory.Strings.Read(Strings.FileID.Kernel, ID, i * 2 + 1);
                br.BaseStream.Seek(4, SeekOrigin.Current);
                //0x0004 1 byte AP needed to learn the ability
                AP = br.ReadByte();
                //0x0005  1 byte
                Boost = br.ReadByte() != 0;
                //0x0006  1 byte
                Stat = (Stat)br.ReadByte();
                //0x0007  1 byte
                Value = br.ReadByte();
            }

            #endregion Constructors

            #region Properties

            public byte AP { get; }

            /// <summary>
            /// Boost Enable
            /// </summary>
            public bool Boost { get; }

            public FF8String Description { get; }

            public Icons.ID Icon { get; } = Icons.ID.Ability_GF;

            public FF8String Name { get; }

            public byte Palette { get; } = Ability.DefaultPalette;

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
                => new GFAbilities(br, i);

            #endregion Methods
        }
    }
}

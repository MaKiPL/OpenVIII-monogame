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
        public class PartyAbilities : EquippableAbility
        {
            #region Fields

            /// <summary>
            /// Section Count
            /// </summary>
            public const int Count = 5;

            /// <summary>
            /// Icon for this type.
            /// </summary>
            public new const Icons.ID Icon = Icons.ID.Ability_Party;

            /// <summary>
            /// Section ID
            /// </summary>
            public const int ID = 15;

            #endregion Fields

            #region Constructors

            private PartyAbilities(FF8String name, FF8String description, byte ap, IReadOnlyList<bool> flags, byte[] unknown0) :
                        base(name, description, ap, Icon)
                        => (Flags, Unknown0) = (flags, unknown0);

            #endregion Constructors

            #region Properties

            /// <summary>
            /// Unknown flags
            /// </summary>
            public IReadOnlyList<bool> Flags { get; }

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
            {
                //0x0000	2 bytes Offset
                FF8StringReference name = Memory.Strings.Read(Strings.FileID.KERNEL, ID, i * 2);
                //0x0002	2 bytes Offset
                FF8StringReference description = Memory.Strings.Read(Strings.FileID.KERNEL, ID, i * 2 + 1);
                br.BaseStream.Seek(4, SeekOrigin.Current);
                //0x0004  1 byte
                byte ap = br.ReadByte();
                //0x0005  1 byte //TODO reverse flags. probably a unique value for each ability
                IReadOnlyList<bool> flags = new BitArray(br.ReadBytes(1)).Cast<bool>().ToList();
                //0x0006  2 byte
                byte[] unknown0 = br.ReadBytes(2);
                return new PartyAbilities(name, description, ap, flags, unknown0);
            }

            #endregion Methods
        }

        #endregion Classes
    }
}
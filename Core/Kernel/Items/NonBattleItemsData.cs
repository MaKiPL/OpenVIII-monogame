using System.Collections.Generic;
using System.Linq;

namespace OpenVIII
{
    namespace Kernel
    {
        /// <summary>
        /// Non battle Items Name and Description Offsets Data
        /// </summary>
        /// <see cref="https://github.com/alexfilth/doomtrain/wiki/Non-battle-item-name-and-description-offsets"/>
        public sealed class NonBattleItemsData
        {
            #region Fields

            public const int Count = 166;

            public const int ID = 8;

            #endregion Fields

            #region Constructors

            private NonBattleItemsData(int i)
            {
                //0x0000	2 bytes Offset to item name
                Name = Memory.Strings.Read(Strings.FileID.Kernel, ID, i * 2);
                //0x0002	2 bytes Offset to item description
                Description = Memory.Strings.Read(Strings.FileID.Kernel, ID, i * 2 + 1);
                NonBattleItemsDataID = i;
            }

            #endregion Constructors

            #region Properties

            /// <summary>
            ///0x0002	2 bytes Offset to item description
            /// </summary>
            public FF8String Description { get; }

            /// <summary>
            ///0x0000	2 bytes Offset to item name
            /// </summary>
            public FF8String Name { get; }

            public int NonBattleItemsDataID { get; }

            #endregion Properties

            #region Methods

            public static IReadOnlyList<NonBattleItemsData> Read()
                => Enumerable.Range(0, Count).Select(CreateInstance).ToList();

            public override string ToString() => Name;

            private static NonBattleItemsData CreateInstance(int i) => new NonBattleItemsData(i);

            #endregion Methods
        }
    }
}

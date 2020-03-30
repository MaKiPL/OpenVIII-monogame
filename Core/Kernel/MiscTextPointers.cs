using System.Collections.Generic;
using System.Linq;

namespace OpenVIII
{
    namespace Kernel
    {
        /// <summary>
        /// Misc Text Pointers Data
        /// </summary>
        /// <see cref="https://github.com/alexfilth/doomtrain/wiki/Misc-text-pointers"/>
        public sealed class MiscTextPointers
        {
            #region Fields

            public const int Count = 128;

            public const int ID = 30;

            public const int Size = 2;

            #endregion Fields

            #region Constructors

            /// <summary>
            /// Create Misc Text Pointer
            /// </summary>
            /// <param name="i">//0x0000	2 bytes Offset to item name</param>
            private MiscTextPointers(int i) => Value = Memory.Strings.Read(Strings.FileID.Kernel, ID, i);

            #endregion Constructors

            #region Properties

            ///0x0000	2 bytes Offset to item name
            public FF8String Value { get; }

            #endregion Properties

            #region Methods

            public static explicit operator FF8String(MiscTextPointers v) => v.Value;

            public static IReadOnlyList<MiscTextPointers> Read()
                => Enumerable.Range(0, Count).Select(CreateInstance).ToList();

            public override string ToString() => Value;

            private static MiscTextPointers CreateInstance(int i) => new MiscTextPointers(i);

            #endregion Methods
        }
    }
}
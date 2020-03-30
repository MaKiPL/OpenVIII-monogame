using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OpenVIII
{
    namespace Kernel
    {
        /// <summary>
        /// Slot Sets Data
        /// </summary>
        /// <see cref="https://github.com/alexfilth/doomtrain/wiki/Selphie-limit-break-sets"/>
        /// <seealso cref="https://finalfantasy.fandom.com/wiki/Slots_(ability_type)#Final_Fantasy_VIII"/>
        public sealed class SelphieLimitBreakSets : IReadOnlyList<Slot>
        {
            #region Fields

            public const int ID = 27;

            public const int SectionCount = 16;

            public const int Size = 16;

            #endregion Fields

            #region Constructors

            private SelphieLimitBreakSets(BinaryReader br)
            => Slots = Slot.Read(br);

            #endregion Constructors

            #region Properties

            public int Count => Slots.Count;

            public IReadOnlyList<Slot> Slots { get; }

            #endregion Properties

            #region Indexers

            public Slot this[int index] => Slots[index];

            #endregion Indexers

            #region Methods

            public static SelphieLimitBreakSets CreateInstance(BinaryReader br) => new SelphieLimitBreakSets(br);

            public static IReadOnlyList<SelphieLimitBreakSets> Read(BinaryReader br)
                => Enumerable.Range(0, SectionCount).Select(x => CreateInstance(br)).ToList().AsReadOnly();

            public IEnumerator<Slot> GetEnumerator() => Slots.GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)Slots).GetEnumerator();

            #endregion Methods
        }
    }
}
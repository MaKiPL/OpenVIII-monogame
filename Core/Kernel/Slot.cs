using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;

namespace OpenVIII
{
    namespace Kernel
    {
        /// <summary>
        /// Slot Magic Data
        /// </summary>
        /// <see cref="https://github.com/alexfilth/doomtrain/wiki/Selphie-limit-break-sets"/>
        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        public sealed class Slot
        {
            private const int Count = 8;
            #region Fields

            private readonly byte _count;

            private readonly byte _magicID;

            #endregion Fields

            #region Constructors

            private Slot(BinaryReader br) : this(magicID: br.ReadByte(), count: br.ReadByte()) =>
                (_magicID, _count) = (br.ReadByte(), br.ReadByte());

            private Slot((byte, byte) valueTuple) => (_magicID, _count) = valueTuple;

            private Slot(byte magicID, byte count) => (_magicID, _count) = (magicID, count);

            #endregion Constructors

            #region Properties

            public byte Casts => checked((byte)(Memory.Random.Next(_count) + 1));

            public MagicData MagicData => Memory.Kernel_Bin.MagicData != null && _magicID < Memory.Kernel_Bin.MagicData.Count ? Memory.Kernel_Bin.MagicData[_magicID] : null;

            #endregion Properties

            #region Methods

            public static Slot CreateInstance((byte, byte) valueTuple) => new Slot(valueTuple);

            public static Slot CreateInstance(byte magicID, byte count) => new Slot(magicID, count);

            public static Slot CreateInstance(BinaryReader br) => new Slot(br);

            public static IReadOnlyList<Slot> Read(BinaryReader br)
                => Enumerable.Range(0, Count).Select(x => CreateInstance(br)).ToList().AsReadOnly();

            #endregion Methods
        }
    }
}
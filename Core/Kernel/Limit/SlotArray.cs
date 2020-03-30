using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OpenVIII
{
    namespace Kernel
    {
        /// <summary>
        /// Slot Array Data
        /// </summary>
        /// <see cref="https://github.com/alexfilth/doomtrain/wiki/Slot-array"/>
        public sealed class SlotArray
        {
            #region Fields

            public const int Count = 60;

            public const int ID = 26;

            public const int Size = 1;

            #endregion Fields

            #region Constructors

            private SlotArray(byte slotID) => SlotID = slotID;

            #endregion Constructors

            #region Properties

            public byte SlotID { get; }

            #endregion Properties

            #region Methods

            private static SlotArray CreateInstance(BinaryReader br) => new SlotArray(br.ReadByte());

            private static SlotArray CreateInstance(byte slotID) => new SlotArray(slotID);

            public static implicit operator byte(SlotArray slotArray) => slotArray.SlotID;

            public static implicit operator SlotArray(byte slotID) => CreateInstance(slotID);

            public static IReadOnlyList<SlotArray> Read(BinaryReader br) =>
                Enumerable.Range(0, Count).Select(x => CreateInstance(br)).ToList().AsReadOnly();

            public override string ToString()
            {
                return $"{SlotID:D2}";
            }

            #endregion Methods
        }
    }
}
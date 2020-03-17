using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OpenVIII.Battle.Dat
{
    public struct AnimationSequence : IReadOnlyList<byte>
    {
        #region Fields

        private readonly IReadOnlyList<byte> Data;
        public readonly int ID;
        public readonly uint Offset;

        #endregion Fields

        #region Constructors

        private AnimationSequence(BinaryReader br, uint start, uint end, int id) : this()
        {
            br.BaseStream.Seek(start, SeekOrigin.Begin);

            ID = id;
            Offset = start;
            Data = br.ReadBytes(((int)(start - end)));
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Test-Reason for list is so i can go read the data with out removing it.
        /// </summary>
        public IReadOnlyList<byte> AnimationQueue { get; private set; }

        #endregion Properties

        #region Methods

        public static IReadOnlyList<AnimationSequence> CreateInstances(BinaryReader br, uint start, uint end)
        {
            // nothing final in here just was trying to dump data to see what was there.
            br.BaseStream.Seek(start, SeekOrigin.Begin);
            uint[] offsets = new uint[br.ReadUInt16()];
            for (ushort i = 0; i < offsets.Length; i++)
            {
                ushort offset = br.ReadUInt16();
                if (offset == 0)
                    continue;
                offsets[i] = offset + start;
            }

            IReadOnlyList<uint> orderedEnumerable = offsets.Where(x => x > 0).Distinct().OrderBy(x => x).ToList().AsReadOnly();
            return orderedEnumerable.Select((x, i) => new AnimationSequence(br, x, orderedEnumerable.Count > i + 1 ? orderedEnumerable[i + 1] : end, i))
                .ToList().AsReadOnly();
        }

        #endregion Methods

        public IEnumerator<byte> GetEnumerator()
        {
            return Data.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Data.GetEnumerator();
        }

        public int Count => Data.Count;

        public byte this[int index] => Data[index];
    }
}
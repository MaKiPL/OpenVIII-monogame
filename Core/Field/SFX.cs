using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace OpenVIII.Fields
{
    /// <summary>
    /// Sound IDs
    /// </summary>
    /// <see cref="http://wiki.ffrtt.ru/index.php?title=FF8/FileFormat_SFX"/>
    public class SFX : IReadOnlyList<uint>
    {
        #region Fields

        private readonly IReadOnlyList<uint> _sndIDs;

        #endregion Fields

        #region Constructors

        public SFX(byte[] sfxB)
        {
            if (sfxB == null || sfxB.Length < 4) return;
            var sndIDs = new List<uint>(sfxB.Length / 4);
            MemoryStream ms;
            using (var br = new BinaryReader(ms = new MemoryStream(sfxB)))
                while (ms.Position < ms.Length)
                    sndIDs.Add(br.ReadUInt32());
            _sndIDs = sndIDs.AsReadOnly();
        }

        #endregion Constructors

        #region Properties

        public int Count => _sndIDs.Count;

        #endregion Properties

        #region Indexers

        public uint this[int index] => _sndIDs[index];

        #endregion Indexers

        #region Methods

        public IEnumerator<uint> GetEnumerator() => _sndIDs.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => _sndIDs.GetEnumerator();

        #endregion Methods
    }
}
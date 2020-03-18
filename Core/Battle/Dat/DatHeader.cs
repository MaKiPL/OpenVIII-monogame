using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OpenVIII.Battle.Dat
{
    /// <summary>
    /// Dat file header based on monster's 11 sections
    /// </summary>
    /// <see cref="http://wiki.ffrtt.ru/index.php/FF8/FileFormat_DAT#Header"/>
    public struct DatHeader : IReadOnlyList<uint>
    {
        #region Fields

        public readonly uint Eof;
        private readonly IReadOnlyList<uint> _pSections;

        #endregion Fields

        #region Constructors

        private DatHeader(BinaryReader br)
        {
            int cSections = br.ReadInt32();
            _pSections = Enumerable.Range(0, cSections).Select(_ => br.ReadUInt32()).ToArray();
            Eof = br.ReadUInt32();
        }

        #endregion Constructors

        #region Properties

        public int Count => _pSections.Count;

        #endregion Properties

        #region Indexers

        public uint this[int index] => _pSections[index];

        #endregion Indexers

        #region Methods

        public static DatHeader CreateInstance(BinaryReader br) => new DatHeader(br);

        public IEnumerator<uint> GetEnumerator() => _pSections.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_pSections).GetEnumerator();

        #endregion Methods
    }
}
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace OpenVIII.Battle.Dat
{
    /// <summary>
    /// Dat file header based on monster's 11 sections
    /// </summary>
    /// <see cref="http://wiki.ffrtt.ru/index.php/FF8/FileFormat_DAT#Header"/>
    public struct DatFile : IReadOnlyList<uint>
    {
        #region Fields

        private readonly IReadOnlyList<uint> _pSections;

        public readonly uint Eof;

        #endregion Fields

        #region Constructors

        private DatFile(BinaryReader br)
        {
            int cSections = br.ReadInt32();
            _pSections = Enumerable.Range(0, cSections).Select(_ => br.ReadUInt32()).ToArray();
            Eof = br.ReadUInt32();
        }

        #endregion Constructors

        #region Methods

        public static DatFile CreateInstance(BinaryReader br) => new DatFile(br);

        #endregion Methods

        public IEnumerator<uint> GetEnumerator()
        {
            return _pSections.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable) _pSections).GetEnumerator();
        }

        public int Count => _pSections.Count;

        public uint this[int index] => _pSections[index];
    }
}
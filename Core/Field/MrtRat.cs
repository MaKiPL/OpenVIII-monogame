using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OpenVIII.Fields
{
    /// <summary>
    /// Contains Field Rate and battle Formations (Encounter)
    /// </summary>
    /// <see cref="http://wiki.ffrtt.ru/index.php?title=FF8/FileFormat_RAT_MRT"/>
    public class MrtRat : IReadOnlyDictionary<ushort, byte>
    {
        /*
                private const byte NoEncounters = 0;
        */

        #region Fields

        private readonly IReadOnlyDictionary<ushort, byte> _mrtrat;

        #endregion Fields

        #region Constructors

        public MrtRat(byte[] mrtB, byte[] ratB)
        {
            ushort[] mrt = new ushort[4];
            using (BinaryReader br = new BinaryReader(new MemoryStream(mrtB)))
                foreach (int i in Enumerable.Range(0, mrt.Length))
                    mrt[i] = br.ReadUInt16();
            using (BinaryReader br = new BinaryReader(new MemoryStream(ratB)))
                _mrtrat = mrt.Distinct().ToDictionary(x => x, x => br.ReadByte());
        }

        #endregion Constructors

        #region Properties

        public int Count => _mrtrat.Count;
        public IEnumerable<ushort> Keys => _mrtrat.Keys;
        public IEnumerable<byte> Values => _mrtrat.Values;

        #endregion Properties

        /*
                private bool NoEncountersOnField => _mrtrat.All(x => x.Value == NoEncounters);
        */

        #region Indexers

        public byte this[ushort key] => _mrtrat[key];

        #endregion Indexers

        #region Methods

        public bool ContainsKey(ushort key) => _mrtrat.ContainsKey(key);

        public IEnumerator<KeyValuePair<ushort, byte>> GetEnumerator() => _mrtrat.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => _mrtrat.GetEnumerator();

        public bool TryGetValue(ushort key, out byte value) => _mrtrat.TryGetValue(key, out value);

        #endregion Methods
    }
}
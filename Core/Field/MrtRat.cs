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
        #region Fields

        private const byte NO_ENCOUNTERS = 0;
        private Dictionary<ushort, byte> mrtrat;

        #endregion Fields

        #region Constructors

        public MrtRat(byte[] mrtb, byte[] ratb)
        {
            MemoryStream ms;
            ushort[] mrt = new ushort[4];
            using (BinaryReader br = new BinaryReader(ms = new MemoryStream(mrtb)))
                foreach (int i in Enumerable.Range(0, mrt.Length))
                    mrt[i] = br.ReadUInt16();
            using (BinaryReader br = new BinaryReader(ms = new MemoryStream(ratb)))
                mrtrat = mrt.Distinct().ToDictionary(x => x, x => br.ReadByte());
        }

        #endregion Constructors

        #region Properties

        public int Count => ((IReadOnlyDictionary<ushort, byte>)mrtrat).Count;
        public IEnumerable<ushort> Keys => ((IReadOnlyDictionary<ushort, byte>)mrtrat).Keys;
        public IEnumerable<byte> Values => ((IReadOnlyDictionary<ushort, byte>)mrtrat).Values;

        private bool No_Encounters_On_Field => mrtrat.All(x => x.Value == NO_ENCOUNTERS);

        #endregion Properties

        #region Indexers

        public byte this[ushort key] => ((IReadOnlyDictionary<ushort, byte>)mrtrat)[key];

        #endregion Indexers

        #region Methods

        public bool ContainsKey(ushort key) => ((IReadOnlyDictionary<ushort, byte>)mrtrat).ContainsKey(key);

        public IEnumerator<KeyValuePair<ushort, byte>> GetEnumerator() => ((IReadOnlyDictionary<ushort, byte>)mrtrat).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => ((IReadOnlyDictionary<ushort, byte>)mrtrat).GetEnumerator();

        public bool TryGetValue(ushort key, out byte value) => ((IReadOnlyDictionary<ushort, byte>)mrtrat).TryGetValue(key, out value);

        #endregion Methods
    }
}
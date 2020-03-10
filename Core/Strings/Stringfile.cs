using System.Collections;
using System.Collections.Generic;

namespace OpenVIII
{
    public sealed class StringFile : IReadOnlyDictionary<int, List<FF8StringReference>>
    {
        #region Fields

        public Dictionary<int, List<FF8StringReference>> SPositions { get; }
        public List<Loc> SubPositions { get; }

        #endregion Fields

        #region Constructors

        public StringFile(int count = 0)
        {
            SPositions = new Dictionary<int, List<FF8StringReference>>(count);
            SubPositions = new List<Loc>(count);
        }
        public FF8StringReference this[int i, int j]
        {
            get
            {
                if (SPositions != null && SPositions.TryGetValue(i, out List<FF8StringReference> listOfStrings) && listOfStrings.Count > j)
                    return SPositions[i][j];
                return null;
            }
        }

        public bool ContainsKey(int key)
        {
            return SPositions.ContainsKey(key);
        }

        public bool TryGetValue(int key, out List<FF8StringReference> value)
        {
            return SPositions.TryGetValue(key, out value);
        }

        List<FF8StringReference> IReadOnlyDictionary<int, List<FF8StringReference>>.this[int key] => SPositions[key];

        public IEnumerable<int> Keys => ((IReadOnlyDictionary<int, List<FF8StringReference>>) SPositions).Keys;

        public IEnumerable<List<FF8StringReference>> Values => ((IReadOnlyDictionary<int, List<FF8StringReference>>) SPositions).Values;

        public Loc this[int i] => SubPositions[i];


        #endregion Constructors
        
        public IEnumerator<KeyValuePair<int, List<FF8StringReference>>> GetEnumerator()
        {
            return SPositions.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable) SPositions).GetEnumerator();
        }

        public int Count => SPositions.Count;
    }
}
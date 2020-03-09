using System.Collections.Generic;

namespace OpenVIII
{
    public sealed class StringFile
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

        public Loc this[int i] => SubPositions[i];


        #endregion Constructors
    }
}
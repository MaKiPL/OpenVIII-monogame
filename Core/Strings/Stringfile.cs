using System.Collections.Generic;

namespace OpenVIII
{
    public class StringFile
    {
        #region Fields

        public Dictionary<uint, List<FF8StringReference>> sPositions;
        public List<Loc> subPositions;

        #endregion Fields

        #region Constructors

        public StringFile(int count = 0)
        {
            sPositions = new Dictionary<uint, List<FF8StringReference>>(count);
            subPositions = new List<Loc>(count);
        }
        public FF8StringReference this[uint i, int j]
        {
            get
            {
                if (sPositions != null && sPositions.TryGetValue(i, out List<FF8StringReference> listofstrings) && listofstrings.Count > j)
                    return sPositions[i][j];
                else return null;
            }
        }

        public Loc this[int i] => subPositions[i];


        #endregion Constructors
    }
}
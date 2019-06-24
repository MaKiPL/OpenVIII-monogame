using System.Collections.Generic;

namespace OpenVIII
{
    public class Stringfile
    {
        #region Fields

        public Dictionary<uint, List<FF8StringReference>> sPositions;
        public List<Loc> subPositions;

        #endregion Fields

        #region Constructors

        public Stringfile(Dictionary<uint, List<FF8StringReference>> sPositions, List<Loc> subPositions)
        {
            this.sPositions = sPositions;
            this.subPositions = subPositions;
        }
        public FF8StringReference this[uint i,int j] => sPositions[i][j];
        public Loc this[int i] => subPositions[i];


        #endregion Constructors
    }
}
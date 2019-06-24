using System.Collections.Generic;

namespace OpenVIII
{
    public class Stringfile
    {
        #region Fields

        public Dictionary<uint, List<uint>> sPositions;
        public List<Loc> subPositions;

        #endregion Fields

        #region Constructors

        public Stringfile(Dictionary<uint, List<uint>> sPositions, List<Loc> subPositions)
        {
            this.sPositions = sPositions;
            this.subPositions = subPositions;
        }



        #endregion Constructors
    }
}
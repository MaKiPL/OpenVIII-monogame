using System.Collections.Generic;

namespace FF8
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

        /// <summary>
        /// do not use this.
        /// </summary>
        private Stringfile()
        {
            this.sPositions = null;
            this.subPositions = null;
        }

        #endregion Constructors
    }
}
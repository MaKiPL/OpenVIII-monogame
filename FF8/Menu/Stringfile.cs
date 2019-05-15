using System.Collections.Generic;

namespace FF8
{
    internal class Stringfile
    {
        #region Fields

        internal Dictionary<uint, List<uint>> sPositions;
        internal List<Loc> subPositions;

        #endregion Fields

        #region Constructors

        internal Stringfile(Dictionary<uint, List<uint>> sPositions, List<Loc> subPositions)
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
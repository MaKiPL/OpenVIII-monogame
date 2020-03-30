using System.Collections.Generic;

namespace OpenVIII.Fields.Scripts
{
    public static partial class Jsm
    {
        #region Interfaces

        public interface IJsmControl
        {
            #region Methods

            IEnumerable<Segment> EnumerateSegments();

            #endregion Methods
        }

        #endregion Interfaces
    }
}
using System.Collections.Generic;

namespace OpenVIII.Fields
{
    public static partial class Field
    {
        #region Interfaces

        public interface ILookupService
        {
            #region Methods

            IEnumerable<Info> EnumerateAll();

            #endregion Methods
        }

        #endregion Interfaces
    }
}
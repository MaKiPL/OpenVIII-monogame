using System.Collections.Generic;

namespace OpenVIII.Fields
{
    public static partial class Field
    {
        public interface ILookupService
        {
            IEnumerable<Info> EnumerateAll();
        }
    }
}
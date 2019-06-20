using System.Collections.Generic;

namespace OpenVIII
{
    public static partial class Field
    {
        public interface ILookupService
        {
            IEnumerable<Info> EnumerateAll();
        }
    }
}
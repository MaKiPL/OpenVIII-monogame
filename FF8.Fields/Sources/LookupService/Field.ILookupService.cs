using System.Collections.Generic;

namespace FF8.Fields
{
    public static partial class Field
    {
        public interface ILookupService
        {
            IEnumerable<Info> EnumerateAll();
        }
    }
}
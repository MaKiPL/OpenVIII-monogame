using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace OpenVIII.Fields
{
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public static class EnumerableExtensionMethods
    {
        public static IEnumerable<T> Prepend<T>(this IEnumerable<T> enumerable, T prefix)
        {
            yield return prefix;
         
            foreach (var item in enumerable)
                yield return item;
        }
    }
}
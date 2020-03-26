using System;
using System.Collections.Generic;

namespace OpenVIII
{
    public static class IListExtensionMethods
    {
        public static Int32 LastIndex<T>(this IList<T> list)
        {
            if (list.Count < 1)
                throw new ArgumentException("Collection is empty.", nameof(list));
            
            return list.Count - 1;
        }

        public static T RemoveLast<T>(this IList<T> list)
        {
            var lastIndex = list.LastIndex();
            var removedItem = list[lastIndex];
            
            list.RemoveAt(lastIndex);
            
            return removedItem;
        }
    }
}
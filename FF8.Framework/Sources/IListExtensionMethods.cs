using System;
using System.Collections.Generic;

namespace FF8.Framework
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
            Int32 lastIndex = list.LastIndex();
            T removedItem = list[lastIndex];
            
            list.RemoveAt(lastIndex);
            
            return removedItem;
        }
    }
}
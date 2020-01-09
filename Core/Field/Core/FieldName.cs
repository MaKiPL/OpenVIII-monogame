using System;
using System.Collections.Generic;

namespace OpenVIII.Fields
{
    public static class FieldName
    {
        public static String Get(int id)
        {
            if (_dic.TryGetValue(id, out var name))
                return name;

            return $"Unknown field: {id}";
        }

        //TODO
        /// <summary>
        /// This should be eventually initialized with real location names. Currently I don't know where they are ~Maki
        /// </summary>
        private static readonly Dictionary<int, String> _dic = new Dictionary<int, String>()
        {
            
        };
    }
}
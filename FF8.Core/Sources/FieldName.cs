using System;
using System.Collections.Generic;

namespace FF8.Core
{
    public static class FieldName
    {
        public static String Get(FieldId id)
        {
            if (_dic.TryGetValue(id, out var name))
                return name;

            return $"Unknown field: {id}";
        }

        private static readonly Dictionary<FieldId, String> _dic = new Dictionary<FieldId, String>()
        {
            {FieldId.bghoke_2, "B-Garden - Infirmary"}
        };
    }
}
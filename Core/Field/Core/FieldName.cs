using System;
using System.Collections.Generic;

namespace OpenVIII.Fields
{
    public static class FieldName
    {
        #region Fields

        //TODO
        /// <summary>
        /// This should be eventually initialized with real location names. Currently I don't know where they are ~Maki
        /// </summary>
        private static readonly Dictionary<int, string> _dic = new Dictionary<int, string>()
        {
        };

        #endregion Fields

        #region Methods

        public static string Get(int id)
        {
            if (_dic.TryGetValue(id, out var name))
                return name;

            return $"Unknown field: {id}";
        }

        #endregion Methods
    }
}
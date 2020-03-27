using System;
using System.Collections.Generic;

namespace OpenVIII.Fields
{
    public static class SoundEffectName
    {
        #region Fields

        private static readonly Dictionary<int, string> _dic = new Dictionary<int, string>()
        {
        };

        #endregion Fields

        #region Methods

        public static string Get(int id)
        {
            return _dic.TryGetValue(id, out var name) ? name : $"Unknown sound effect: {id}";
        }

        #endregion Methods
    }
}
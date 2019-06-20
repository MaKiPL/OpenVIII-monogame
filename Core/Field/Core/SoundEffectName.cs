using System;
using System.Collections.Generic;

namespace OpenVIII
{
    public static class SoundEffectName
    {
        public static String Get(SoundEffectId id)
        {
            if (_dic.TryGetValue(id, out var name))
                return name;

            return $"Unknown sound effect: {id}";
        }

        private static readonly Dictionary<SoundEffectId, String> _dic = new Dictionary<SoundEffectId, String>()
        {
        };
    }
}
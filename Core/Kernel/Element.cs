using System;
using System.Diagnostics.CodeAnalysis;

namespace OpenVIII
{
    namespace Kernel
    {
        /// <summary>
        /// Element types
        /// </summary>
        /// <see cref="https://github.com/alexfilth/doomtrain/wiki/Elements"/>
        [Flags]
        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        public enum Element : byte
        {
            NonElemental = 0x00, Fire = 0x01, Ice = 0x02, Thunder = 0x04,
            Earth = 0x08, Poison = 0x10, Wind = 0x20, Water = 0x40, Holy = 0x80,
        }
    }
}
using System;
using System.Diagnostics.CodeAnalysis;

namespace OpenVIII
{
    namespace Kernel
    {
        /// <summary>
        /// Status flags for junctions
        /// </summary>
        [Flags]
        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        public enum JunctionStatuses : ushort
        {
            None = 0x0,
            Death = 0x1,
            Poison = 0x2,
            Petrify = 0x4,
            Darkness = 0x8,
            Silence = 0x10,
            Berserk = 0x20,
            Zombie = 0x40,
            Sleep = 0x80,
            Slow = 0x100,
            Stop = 0x200,
            /// <summary>
            /// Curse; unused for attack
            /// </summary>
            Curse = 0x400,
            Confusion = 0x800,
            Drain = 0x1000,
        }
    }
}
using System;

namespace OpenVIII
{
    namespace Kernel
    {
        /// <summary>
        /// Status flags for junctions
        /// </summary>
        [Flags]
        public enum J_Statuses : ushort
        {
            None = 0x0000,
            Death = 0x0001,
            Poison = 0x0002,
            Petrify = 0x0004,
            Darkness = 0x008,
            Silence = 0x0010,
            Berserk = 0x0020,
            Zombie = 0x0040,
            Sleep = 0x0080,
            Slow = 0x0100,
            Stop = 0x0200,
            /// <summary>
            /// Curse; unused for attack
            /// </summary>
            Curse = 0x0400,
            Confusion = 0x0800,
            Drain = 0x1000,
        }
    }
}
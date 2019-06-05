using System;

namespace FF8
{
    public partial class Kernel_bin
    {
        /// <summary>
        /// Status flags for junctions
        /// </summary>
        [Flags]
        public enum J_Statuses : ushort
        {
            None = 0x0001,
            Death = 0x0002,
            Poison = 0x0004,
            Petrify = 0x0008,
            Darkness = 0x0010,
            Silence = 0x0020,
            Berserk = 0x0040,
            Zombie = 0x0080,
            Sleep = 0x0100,
            Slow = 0x0200,
            Stop = 0x0400,
            /// <summary>
            /// Curse; unused for attack
            /// </summary>
            Curse = 0x0800,
            Confusion = 0x1000,
            Drain = 0x0010,
        }
    }
}
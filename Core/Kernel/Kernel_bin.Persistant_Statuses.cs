using System;

namespace OpenVIII
{
    public partial class Kernel_bin
    {
        /// <summary>
        /// Statues that persist out of battle.
        /// </summary>
        /// <see cref="https://github.com/alexfilth/doomtrain/wiki/Statuses-0"/>
        /// <seealso cref="http://forums.qhimm.com/index.php?topic=17644.msg251133#msg251133"/>
        [Flags]
        public enum Persistant_Statuses : ushort
        {
            None = 0x00, Death = 0x01, Poison = 0x02, Petrify = 0x04,
            Darkness = 0x08, Silence = 0x10, Berserk = 0x20, Zombie = 0x40, UNK0x80 = 0x80,
            UNK0x0100 = 0x0100,
            UNK0x0200 = 0x0200,
            UNK0x0400 = 0x0400,
            UNK0x0800 = 0x0800,
            UNK0x1000 = 0x1000,
            UNK0x2000 = 0x2000,
            UNK0x4000 = 0x4000,
            UNK0x8000 = 0x8000,
        }
    }
}
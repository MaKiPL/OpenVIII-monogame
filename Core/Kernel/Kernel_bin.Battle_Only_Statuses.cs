using System;

namespace OpenVIII
{
    namespace Kernel
    {
        /// <summary>
        /// Statues that only exist in battle.
        /// </summary>
        /// <see cref="https://github.com/alexfilth/doomtrain/wiki/Statuses-1"/>
        /// <seealso cref="http://forums.qhimm.com/index.php?topic=17644.msg251133#msg251133"/>
        [Flags]
        public enum Battle_Only_Statuses : uint
        {
            None = 0x0, Sleep = 0x1, Haste = 0x2, Slow = 0x4, Stop = 0x8, Regen = 0x10,
            Protect = 0x20, Shell = 0x40, Reflect = 0x80, Aura = 0x100, Curse = 0x200,
            Doom = 0x400, Invincible = 0x800, Petrifying = 0x1000, Float = 0x2000,
            Confuse = 0x4000, Drain = 0x8000, Eject = 0x10000, Double = 0x20000,
            Triple = 0x40000, Defend = 0x80000, UNK0x100000 = 0x100000, UNK0x200000 = 0x200000,
            Charged = 0x400000, Back_Attack = 0x800000, Vit0 = 0x1000000, Angel_Wing = 0x2000000,
            UNK0x4000000 = 0x4000000, UNK0x8000000 = 0x8000000, UNK0x10000000 = 0x10000000,
            UNK0x20000000 = 0x20000000, Has_Magic = 0x40000000, Summon_GF = 0x80000000,
        }
    }
}
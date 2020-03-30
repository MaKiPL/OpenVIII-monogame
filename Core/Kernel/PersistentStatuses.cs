using System;
using System.Diagnostics.CodeAnalysis;

namespace OpenVIII
{
    namespace Kernel
    {
        /// <summary>
        /// Statues that persist out of battle.
        /// </summary>
        /// <see cref="https://github.com/alexfilth/doomtrain/wiki/Statuses-0"/>
        /// <seealso cref="http://forums.qhimm.com/index.php?topic=17644.msg251133#msg251133"/>
        [Flags]
        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        public enum PersistentStatuses : ushort
        {
            None = 0x00, Death = 0x01, Poison = 0x02, Petrify = 0x04,
            Darkness = 0x08, Silence = 0x10, Berserk = 0x20, Zombie = 0x40, Unk0X0080 = 0x80,
            Unk0X0100 = 0x0100,
            Unk0X0200 = 0x0200,
            Unk0X0400 = 0x0400,
            Unk0X0800 = 0x0800,
            Unk0X1000 = 0x1000,
            Unk0X2000 = 0x2000,
            Unk0X4000 = 0x4000,
            Unk0X8000 = 0x8000,
        }
    }
}
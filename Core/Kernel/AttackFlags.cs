using System;
using System.Diagnostics.CodeAnalysis;

namespace OpenVIII
{
    namespace Kernel
    {
        /// <summary>
        /// Attack Flags effects how the attack can be treated.
        /// </summary>
        /// <see cref="https://github.com/alexfilth/doomtrain/blob/master/Doomtrain/MainForm.cs"/>
        [Flags]
        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        public enum AttackFlags
        {
            None = 0x0,
            Shelled = 0x1,
            Unk0X2 = 0x2,
            Unk0X4 = 0x4,
            BreakDamageLimit = 0x8,
            Reflected = 0x10,
            Unk0X20 = 0x20,
            Unk0X40 = 0x40,
            Revive = 0x80
        }
    }
}
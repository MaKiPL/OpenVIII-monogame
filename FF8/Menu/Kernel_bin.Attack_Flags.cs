using System;

namespace FF8
{
    internal partial class Kernel_bin
    {
        /// <summary>
        /// Attack Flags
        /// </summary>
        /// <see cref="https://github.com/alexfilth/doomtrain/blob/master/Doomtrain/MainForm.cs"/>
        [Flags]
        internal enum Attack_Flags
        {
            None = 0x0,
            Shelled = 0x1,
            UNK0x2 = 0x2,
            UNK0x4 = 0x4,
            Break_Damage_Limit = 0x8,
            Reflected = 0x10,
            UNK0x20 = 0x20,
            UNK0x40 = 0x40,
            Revive_ = 0x80
        }
    }
}
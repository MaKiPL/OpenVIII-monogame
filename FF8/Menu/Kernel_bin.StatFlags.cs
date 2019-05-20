using System;

namespace FF8
{
    public partial class Kernel_bin
    {
        [Flags]
        private enum StatFlags
        {
            None = 0x00,
            STR = 0x01,
            VIT = 0x02,
            MAG = 0x04,
            SPR = 0x08,
            SPD = 0x10,
        }
    }
}
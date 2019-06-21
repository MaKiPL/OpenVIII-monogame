using System;

namespace OpenVIII
{
    public partial class Kernel_bin
    {
        [Flags]
        private enum StatFlags :byte
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
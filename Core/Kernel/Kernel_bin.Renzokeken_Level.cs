using System;

namespace FF8
{
    public partial class Kernel_bin
    {
        [Flags]
        public enum Renzokeken_Level : byte
        {
            Rough_Divide = 0x01,
            Fated_Circle = 0x02,
            Blasting_Zone = 0x04,
            Lion_Heart = 0x08,
        }
    }
}
using System;

namespace OpenVIII
{
    namespace Kernel
    {
        [Flags]
        public enum Renzokeken_Finisher : byte
        {
            Rough_Divide = 0x01,
            Fated_Circle = 0x02,
            Blasting_Zone = 0x04,
            Lion_Heart = 0x08,
        }
    }
}
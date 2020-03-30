using System;
using System.Diagnostics.CodeAnalysis;

namespace OpenVIII
{
    namespace Kernel
    {
        [Flags]
        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        public enum StatFlags :byte
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
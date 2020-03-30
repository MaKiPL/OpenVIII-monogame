using System;
using System.Diagnostics.CodeAnalysis;

namespace OpenVIII
{
    namespace Kernel
    {
        /// <summary>
        /// Renzokuken Finishing moves
        /// </summary>
        /// <see cref="https://finalfantasy.fandom.com/wiki/Renzokuken#Finishing_moves"/>
        [Flags]
        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        public enum RenzokukenFinisher : byte
        {
            RoughDivide = 0x01,
            FatedCircle = 0x02,
            BlastingZone = 0x04,
            LionHeart = 0x08,
        }
    }
}
using System;
using System.Diagnostics.CodeAnalysis;

namespace OpenVIII
{
    namespace Kernel
    {
        /// <summary>
        /// Target info
        /// </summary>
        /// <see cref="https://github.com/alexfilth/doomtrain/blob/master/Doomtrain/MainForm.cs"/>
        [Flags]
        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        public enum Target : byte
        {
            None = 0x0,
            Dead = 0x1,
            Unk02 = 0x2,

            /// <summary>
            /// was unk04
            /// </summary>
            Ally = 0x4,

            SingleSide = 0x8,
            SingleTarget = 0x10,
            Unk20 = 0x20,
            Enemy = 0x40,
            Unk80 = 0x80,
        }
    }
}
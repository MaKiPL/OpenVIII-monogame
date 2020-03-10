using System;
using System.Diagnostics.CodeAnalysis;

namespace OpenVIII
{
    namespace Kernel
    {
        [Flags]
        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        public enum Quantity : byte
        {
            /// <summary>
            /// 0% = 0x00
            /// </summary>
            _0f = 0x00,

            /// <summary>
            /// 6.25% = 0x01,
            /// </summary>
            _0625f = 0x01,

            /// <summary>
            /// 12.50% = 0x02
            /// </summary>
            _1250f = 0x02,

            /// <summary>
            /// 25% = 0x04
            /// </summary>
            _25f = 0x04,

            /// <summary>
            /// 50% = 0x08
            /// </summary>
            _50f = 0x08,

            /// <summary>
            /// 100% = 0x10
            /// </summary>
            _1f = 0x10,
        }
    }
}
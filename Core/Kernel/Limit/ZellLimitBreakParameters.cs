using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace OpenVIII
{
    namespace Kernel
    {
        /// <summary>
        /// Zell Limit Break Parameters
        /// </summary>
        /// <see cref="https://github.com/alexfilth/doomtrain/wiki/Zell-limit-break-parameters"/>
        [StructLayout(LayoutKind.Explicit, Size = Size, Pack = 0)]
        public sealed class ZellLimitBreakParameters
        {
            private static ZellLimitBreakParameters CreateInstance(BinaryReader br) => new ZellLimitBreakParameters(br);

            public const int Count = 24;
            public const int ID = 23;
            public const int Size = 4;

            [field: FieldOffset(0x0)]
            [field: MarshalAs(UnmanagedType.ByValArray, SizeConst = Size)]
            public IReadOnlyList<byte> Moves { get; }

            private ZellLimitBreakParameters(BinaryReader br) => Moves = br.ReadBytes(Size);

            public static IReadOnlyList<ZellLimitBreakParameters> Read(BinaryReader br)
            => Enumerable.Range(0, Count).Select(_ => CreateInstance(br)).ToList();
        }
    }
}
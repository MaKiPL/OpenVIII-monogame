using System;
using System.Diagnostics.CodeAnalysis;

namespace OpenVIII.Battle.Dat
{
    [Flags]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public enum KernelFlag : byte
    {
        None = 0,
        Unk0 = 0x1,
        Magic = 0x2,
        Item = 0x4,
        Monster = 0x8,
        Unk1 = 0x10,
        Unk2 = 0x20,
        Unk3 = 0x40,
        Unk4 = 0x80,
    }
}
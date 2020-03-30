using System;

namespace OpenVIII.Battle
{
    /// <summary>
    /// Just a guess.
    /// </summary>
    [Flags]
    public enum GPU : byte
    {
        v0 = 0x0,
        v1 = 0x1,
        v2_add = 0x2,
        v3 = 0x4,
        v4 = 0x8,
        v5 = 0x10,
        v6 = 0x20,
        v7 = 0x40,
        v8 = 0x80,
    }
}
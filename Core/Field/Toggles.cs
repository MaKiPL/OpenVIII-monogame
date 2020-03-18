using System;
using System.Diagnostics.CodeAnalysis;

namespace OpenVIII.Fields
{
    [Flags]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public enum Toggles : byte
    {
        DumpingData = 0x1,
        ClassicSpriteBatch = 0x2,
        Quad = 0x4,
        WalkMesh = 0x8,
        Deswizzle = 0x10,
        Perspective = 0x20,
        Menu = 0x40,
    }
}
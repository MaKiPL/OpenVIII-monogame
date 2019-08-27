using System;

namespace OpenVIII
{
    [Flags]
    public enum Button_Flags : ushort
    {
        Up = 0x10,
        Right = 0x20,
        Down = 0x40,
        Left = 0x80,
        L2 = 0x100,
        R2 = 0x200,
        L1 = 0x400,
        R1 = 0x800,
        Triangle = 0x1000,
        Circle = 0x2000,
        Cross = 0x4000,
        Square = 0x8000
    }
}
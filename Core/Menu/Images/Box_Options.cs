using System;

namespace OpenVIII
{
    [Flags]
    public enum Box_Options : byte
    {
        Default = 0x0,
        Indent = 0x1,
        Buttom = 0x2,
        SkipDraw = 0x4,
        Center = 0x8,
        Middle = 0x10,
        Top = 0x20,
        Right = 0x40,
        Left = 0x80,
    }
}
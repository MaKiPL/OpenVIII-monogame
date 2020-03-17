using System;
using System.Diagnostics.CodeAnalysis;

namespace OpenVIII.Battle.Dat
{
    /// <summary>
    /// Section 7: Information Flags 1
    /// </summary>
    /// <seealso cref="http://wiki.ffrtt.ru/index.php/FF8/FileFormat_DAT#Section_7:_Informations_.26_stats"/>
    [Flags]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public enum Flag1 : byte
    {
        None = 0,
        Zombie = 0x1,
        Fly = 0x2,
        Zz1 = 0x4,
        Zz2 = 0x8,
        Zz3 = 0x10,
        AutoReflect = 0x20,
        AutoShell = 0x40,
        AutoProtect = 0x80,
    }
}
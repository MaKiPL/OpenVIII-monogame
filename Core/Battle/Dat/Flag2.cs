using System;
using System.Diagnostics.CodeAnalysis;

namespace OpenVIII.Battle.Dat
{
    /// <summary>
    /// Section 7: Information Flags 2
    /// </summary>
    /// <seealso cref="http://wiki.ffrtt.ru/index.php/FF8/FileFormat_DAT#Section_7:_Informations_.26_stats"/>
    [Flags]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public enum Flag2 : byte
    {
        None = 0,
        Zz1 = 0x1,
        Zz2 = 0x2,
        Unused1 = 0x4,
        Unused2 = 0x8,
        Unused3 = 0x10,
        Unused4 = 0x20,
        DiablosMissed = 0x40,
        AlwaysCard = 0x80,
    }
}
using System;
using System.Diagnostics.CodeAnalysis;

namespace OpenVIII.Battle.Dat
{
    /// <summary>
    /// Section 7: Information Unknown Flags
    /// </summary>
    /// <seealso cref="http://wiki.ffrtt.ru/index.php/FF8/FileFormat_DAT#Section_7:_Informations_.26_stats"/>
    [Flags]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public enum UnkFlag : byte
    {
        None = 0,
        Unk0 = 0x1,
        Unk1 = 0x2,
        Unk2 = 0x4,
        Unk3 = 0x8,
        Unk4 = 0x10,
        Unk5 = 0x20,
        Unk6 = 0x40,
        Unk7 = 0x80,
    }
}
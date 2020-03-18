using System;
using System.Diagnostics.CodeAnalysis;

namespace OpenVIII.Battle
{
    [Flags]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public enum EncounterFlag : byte
    {
        None,
        CanNotEscape = 0x1,
        NoVictorySequence = 0x2,
        ShowTimer = 0x4,
        NoExp = 0x8,
        SkipExpScreen = 0x10,
        SurpriseAttack = 0x20,
        BackAttacked = 0x40,
        IsScriptedBattle = 0x80,
    }
}
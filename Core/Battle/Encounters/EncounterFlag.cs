using System;

namespace OpenVIII.Battle
{
    [Flags]
    public enum EncounterFlag : byte
    {
        None,
        CantEspace = 0x1,
        NoVictorySequence = 0x2,
        ShowTimer = 0x4,
        NoEXP = 0x8,
        SkipEXPScreen = 0x10,
        SurpriseAttack = 0x20,
        BackAttacked = 0x40,
        isScriptedBattle = 0x80,
    }
}
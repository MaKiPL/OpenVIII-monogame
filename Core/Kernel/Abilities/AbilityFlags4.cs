using System.Diagnostics.CodeAnalysis;

namespace OpenVIII.Kernel
{
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public enum AbilityFlags4 :uint
    {

        Alert = 0x2000,
        MoveFind = 0x4000,
        EncHalf = 0x8000,
        EncNone = 0x10000,
        RareItem = 0x20000,
        SumMag10 = 0x40000,
        SumMag20 = 0x80000,
        SumMag30 = 0x100000,
        SumMag40 = 0x200000,
        GFHP10 = 0x400000,
        GFHP20 = 0x800000,
        GFHP30 = 0x1000000,
        GFHP40 = 0x2000000,
        Boost = 0x4000000,
        Haggle = 0x8000000,
        SellHigh = 0x10000000,
        Familiar = 0x20000000,
        CallShop = 0x40000000,
        JunkShop = 0x80000000,
        LightningMagRF = 0x1,
        IceMagRF = 0x2,
        FireMagRF = 0x4,
        LifeMagRF = 0x8,
        TimeMagRF = 0x10,
        StMagRF = 0x20,
        SuptMagRF = 0x40,
        ForbidMagRF = 0x80,
        RecoveryMedRF = 0x100,
        StMedRF = 0x200,
        AmmoRF = 0x400,
        ToolRF = 0x800,
        ForbidMedRF = 0x1000,
        GFRecoveryMedRF = 0x2000,
        GFAblMedRF = 0x4000,
        MidMagRF = 0x8000,
        HighMagRF = 0x10000,
        MedLvUp = 0x20000,
        CardMod = 0x40000,
    }
}
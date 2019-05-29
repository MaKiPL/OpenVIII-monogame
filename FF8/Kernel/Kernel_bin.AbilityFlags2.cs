using System;

namespace FF8
{
    public partial class Kernel_bin
    {
        ///TODO remove any flag group that isn't used.
        ///TODO if are used correct the flag values.
        /// <summary>
        /// remaning abilities that doen't fit in AbilityFlags.
        /// </summary>
        /// <see cref="https://github.com/alexfilth/doomtrain/wiki/Junctionable-Abilities"/>
        [Flags]
        public enum AbilityFlags2 : uint
        {
            Magic = 0x80000,
            GF = 0x100000,
            Draw = 0x200000,
            Item = 0x400000,
            Empty = 0x800000,
            Card = 0x1000000,
            Doom = 0x2000000,
            MadRush = 0x4000000,
            Treatment = 0x8000000,
            Defend = 0x10000000,
            Darkside = 0x20000000,
            Recover = 0x40000000,
            Absorb = 0x80000000,
            Revive = 0x1,
            LVDown = 0x2,
            LVUp = 0x4,
            Kamikaze = 0x8,
            Devour = 0x10,
            MiniMog = 0x20,
            HP_20 = 0x40,
            HP_40 = 0x80,
            HP_80 = 0x100,
            STR_20 = 0x200,
            STR_40 = 0x400,
            STR_60 = 0x800,
            VIT_20 = 0x1000,
            VIT_40 = 0x2000,
            VIT_60 = 0x4000,
            MAG_20 = 0x8000,
            MAG_40 = 0x10000,
            MAG_60 = 0x20000,
            SPR_20 = 0x40000,
            SPR_40 = 0x80000,
            SPR_60 = 0x100000,
            SPD_20 = 0x200000,
            SPD_40 = 0x400000,
            EVA_30 = 0x800000,
            LUCK_50 = 0x1000000,
            
        }
        public enum AbilityFlags4 :uint
        {

            Alert = 0x2000,
            Move_Find = 0x4000,
            Enc_Half = 0x8000,
            Enc_None = 0x10000,
            RareItem = 0x20000,
            SumMag_10 = 0x40000,
            SumMag_20 = 0x80000,
            SumMag_30 = 0x100000,
            SumMag_40 = 0x200000,
            GFHP_10 = 0x400000,
            GFHP_20 = 0x800000,
            GFHP_30 = 0x1000000,
            GFHP_40 = 0x2000000,
            Boost = 0x4000000,
            Haggle = 0x8000000,
            Sell_High = 0x10000000,
            Familiar = 0x20000000,
            CallShop = 0x40000000,
            JunkShop = 0x80000000,
            TMag_RF = 0x1,
            IMag_RF = 0x2,
            FMag_RF = 0x4,
            LMag_RF = 0x8,
            TimeMag_RF = 0x10,
            STMag_RF = 0x20,
            SuptMag_RF = 0x40,
            ForbidMag_RF = 0x80,
            RecovMed_RF = 0x100,
            STMed_RF = 0x200,
            Ammo_RF = 0x400,
            Tool_RF = 0x800,
            ForbidMed_RF = 0x1000,
            GFRecovMed_RF = 0x2000,
            GFAblMed_RF = 0x4000,
            MidMag_RF = 0x8000,
            HighMag_RF = 0x10000,
            MedLVUp = 0x20000,
            CardMod = 0x40000,
        }
    }
}
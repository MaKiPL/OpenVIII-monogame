using System.Diagnostics.CodeAnalysis;

namespace OpenVIII
{
    namespace Kernel
    {
        /// <summary>
        /// 115 abilities. GF unlockable ones only.
        /// </summary>
        /// <see cref="https://github.com/alexfilth/doomtrain/wiki/Junctionable-Abilities"/>
        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        public enum Abilities :byte
        {
            // https://github.com/alexfilth/doomtrain/wiki/Junction-abilities
            None,
            /// <summary>
            /// Enables HP junction
            /// </summary>
            // ReSharper disable once InconsistentNaming
            HPJ,
            /// <summary>
            /// Enables Strength junction
            /// </summary>
            StrJ,
            /// <summary>
            /// Enables Vitality junction
            /// </summary>
            VitJ,
            /// <summary>
            /// Enables Magic junction
            /// </summary>
            MagJ,
            /// <summary>
            /// Enables Spirit junction
            /// </summary>
            SprJ,
            /// <summary>
            /// Enables Speed junction
            /// </summary>
            SpdJ,
            /// <summary>
            /// Enables Evasion junction
            /// </summary>
            EvaJ,
            /// <summary>
            /// Enables Hit junction
            /// </summary>
            HitJ,
            /// <summary>
            /// Enables Luck junction
            /// </summary>
            LuckJ,
            /// <summary>
            /// Increased elemental attack slot Count to 1
            /// </summary>
            ElAtkJ,
            /// <summary>
            /// Increased status attack slot Count to 1
            /// </summary>
            StAtkJ,
            /// <summary>
            /// Increased elemental defense slot Count to 1
            /// </summary>
            ElDefJ,
            /// <summary>
            /// Increased status defense slot Count to 1
            /// </summary>
            StDefJ,
            /// <summary>
            /// Increased elemental defense slot Count to 2
            /// </summary>
            ElDefJ2,
            /// <summary>
            /// Increased elemental defense slot Count to 4
            /// </summary>
            ElDefJ4,
            /// <summary>
            /// Increased status defense slot Count to 2
            /// </summary>
            StDefJ2,
            /// <summary>
            /// Increased status defense slot Count to 4
            /// </summary>
            StDefJ4,
            /// <summary>
            /// Increases ability slot Count to 3
            /// </summary>
            Ability3,
            /// <summary>
            /// Increases ability slot Count to 4
            /// </summary>
            Ability4,

            //Equippable commands start here
            // https://github.com/alexfilth/doomtrain/wiki/Command-abilities
            Magic,
            GF,
            Draw,
            Item,
            Empty,
            Card,
            Doom,
            MadRush,
            Treatment,
            Defend,
            Darkside,
            Recover,
            Absorb,
            Revive,
            LvDown,
            LvUp,
            Kamikaze,
            Devour,
            MiniMog,
            //Equippable commands end here

            //Equippable abilities start here
            // https://github.com/alexfilth/doomtrain/wiki/Stat-percentage-increasing-abilities
            HP20,
            HP40,
            HP80,
            Str20,
            Str40,
            Str60,
            Vit20,
            Vit40,
            Vit60,
            Mag20,
            Mag40,
            Mag60,
            Spr20,
            Spr40,
            Spr60,
            Spd20,
            Spd40,
            Eva30,
            Luck50,

            //https://github.com/alexfilth/doomtrain/wiki/Character-abilities
            Mug,
            MedData,
            Counter,
            ReturnDamage,
            Cover,
            Initiative,
            MoveHPUp,
            HPBonus,
            StrBonus,
            VitBonus,
            MagBonus,
            SprBonus,
            AutoProtect,
            AutoShell,
            AutoReflect,
            AutoHaste,
            AutoPotion,
            Expend2,
            Expend3,
            Ribbon,

            // https://github.com/alexfilth/doomtrain/wiki/Party-abilities
            Alert,
            MoveFind,
            EncHalf,
            EncNone,
            RareItem,

            // https://github.com/alexfilth/doomtrain/wiki/GF-abilities
            SumMag10,
            SumMag20,
            SumMag30,
            SumMag40,
            GFHP10,
            GFHP20,
            GFHP30,
            GFHP40,
            Boost,
            //Equippable abilities end here

            // https://github.com/alexfilth/doomtrain/wiki/Menu-abilities
            Haggle,
            SellHigh,
            Familiar,
            CallShop,
            JunkShop,
            ThunderMagRF,
            IceMagRF,
            FireMagRF,
            LifeMagRF,
            TimeMagRF,
            StatusMagRF,
            SuptMagRF,
            ForbidMagRF,
            RecoveryMedRF,
            StatusMedRF,
            AmmoRF,
            ToolRF,
            ForbidMedRF,
            GFRecoveryMedRF,
            GFAblMedRF,
            MidMagRF,
            HighMagRF,
            MedLvUp,
            CardMod,
        }
    }
}
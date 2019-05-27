namespace FF8
{
    public partial class Kernel_bin
    {
        /// <summary>
        /// 115 abilities. Flags version in AbilityFlags and AbilitiyFlags2
        /// </summary>
        /// <see cref="https://github.com/alexfilth/doomtrain/wiki/Junctionable-Abilities"/>
        public enum Abilities :byte
        {
            // https://github.com/alexfilth/doomtrain/wiki/Junction-abilities
            None,
            /// <summary>
            /// Enables HP junction
            /// </summary>
            HP_J,
            /// <summary>
            /// Enables Strength junction
            /// </summary>
            Str_J,
            /// <summary>
            /// Enables Vitality junction
            /// </summary>
            Vit_J,
            /// <summary>
            /// Enables Magic junction
            /// </summary>
            Mag_J,
            /// <summary>
            /// Enables Spirit junction
            /// </summary>
            Spr_J,
            /// <summary>
            /// Enables Speed junction
            /// </summary>
            Spd_J,
            /// <summary>
            /// Enables Evasion junction
            /// </summary>
            Eva_J,
            /// <summary>
            /// Enables Hit junction
            /// </summary>
            Hit_J,
            /// <summary>
            /// Enables Luck junction
            /// </summary>
            Luck_J,
            /// <summary>
            /// Increased elemental attack slot count to 1
            /// </summary>
            Elem_Atk_J,
            /// <summary>
            /// Increased status attack slot count to 1
            /// </summary>
            ST_Atk_J,
            /// <summary>
            /// Increased elemental defense slot count to 1
            /// </summary>
            Elem_Def_J,
            /// <summary>
            /// Increased status defense slot count to 1
            /// </summary>
            ST_Def_J,
            /// <summary>
            /// Increased elemental defense slot count to 2
            /// </summary>
            Elem_Defx2,
            /// <summary>
            /// Increased elemental defense slot count to 4
            /// </summary>
            Elem_Defx4,
            /// <summary>
            /// Increased status defense slot count to 2
            /// </summary>
            ST_Def_Jx2,
            /// <summary>
            /// Increased status defense slot count to 4
            /// </summary>
            ST_Def_Jx4,
            /// <summary>
            /// Increases ability slot count to 3
            /// </summary>
            Abilityx3,
            /// <summary>
            /// Increases ability slot count to 4
            /// </summary>
            Abilityx4,

            //Equipable commands start here
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
            LVDown,
            LVUp,
            Kamikaze,
            Devour,
            MiniMog,
            //Equipable commands end here

            //Equipable abilities start here
            // https://github.com/alexfilth/doomtrain/wiki/Stat-percentage-increasing-abilities
            HP_20,
            HP_40,
            HP_80,
            STR_20,
            STR_40,
            STR_60,
            VIT_20,
            VIT_40,
            VIT_60,
            MAG_20,
            MAG_40,
            MAG_60,
            SPR_20,
            SPR_40,
            SPR_60,
            SPD_20,
            SPD_40,
            EVA_30,
            LUCK_50,

            //https://github.com/alexfilth/doomtrain/wiki/Character-abilities
            Mug,
            MedData,
            Counter,
            Return_Damage,
            Cover,
            Initiative,
            Move_HPUp,
            HPBonus,
            StrBonus,
            VitBonus,
            MagBonus,
            SprBonus,
            Auto_Protect,
            Auto_Shell,
            Auto_Reflect,
            Auto_Haste,
            AutoPotion,
            Expendx2_1,
            Expendx3_1,
            Ribbon,

            // https://github.com/alexfilth/doomtrain/wiki/Party-abilities
            Alert,
            Move_Find,
            Enc_Half,
            Enc_None,
            RareItem,

            // https://github.com/alexfilth/doomtrain/wiki/GF-abilities
            SumMag_10,
            SumMag_20,
            SumMag_30,
            SumMag_40,
            GFHP_10,
            GFHP_20,
            GFHP_30,
            GFHP_40,
            Boost,
            //Equipable abilities end here

            // https://github.com/alexfilth/doomtrain/wiki/Menu-abilities
            Haggle,
            Sell_High,
            Familiar,
            CallShop,
            JunkShop,
            TMag_RF,
            IMag_RF,
            FMag_RF,
            LMag_RF,
            TimeMag_RF,
            STMag_RF,
            SuptMag_RF,
            ForbidMag_RF,
            RecovMed_RF,
            STMed_RF,
            Ammo_RF,
            Tool_RF,
            ForbidMed_RF,
            GFRecovMed_RF,
            GFAblMed_RF,
            MidMag_RF,
            HighMag_RF,
            MedLVUp,
            CardMod,
        }
    }
}
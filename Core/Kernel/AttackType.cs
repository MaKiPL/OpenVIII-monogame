namespace OpenVIII
{
    namespace Kernel
    {
        /// <summary>
        /// Attack Type
        /// </summary>
        /// <see cref="https://github.com/alexfilth/doomtrain/blob/master/Doomtrain/Resources/Attack_Type_List.txt"/>
        public enum AttackType
        {
            None,
            PhysicalAttack,
            MagicAttack,
            CurativeMagic,
            CurativeItem,
            Revive,
            ReviveAtFullHP,
            PhysicalDamage,
            MagicDamage,
            RenzokukenFinisher,
            SquallGunbladeAttack,
            GF,
            Scan,
            LvDown,
            SummonItem,
            GFIgnoreTargetSPR,
            LvUp,
            Card,
            Kamikaze,
            Devour,
            GFDamage,
            Unknown1,
            MagicAttackIgnoreTargetSPR,
            AngeloSearch,
            MoogleDance,
            WhiteWindQuistis,
            LvAttack,
            FixedDamage,
            TargetCurrentHP1,
            FixedMagicDamageBasedOnGFLevel,
            Unknown2,
            Unknown3,
            GivePercentageHP,
            Unknown4,
            EveryoneGrudge,
            _1_HP_Damage,
            PhysicalAttackIgnoreTargetVIT
        }
    }
}
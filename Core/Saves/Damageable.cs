using System;
using System.Collections.Generic;

namespace OpenVIII
{
    public abstract class Damageable
    {
        protected ushort _CurrentHP; //0x00 -- forgot this one heh
        private Kernel_bin.Persistant_Statuses _statuses0;
        private Kernel_bin.Battle_Only_Statuses _statuses1;

        private Dictionary<Kernel_bin.Attack_Type, Func<int, Kernel_bin.Attack_Flags, int>> damageActions;
        public virtual bool Critical()
        {
            return CurrentHP() <= CriticalHP();
        }

        public virtual ushort CriticalHP() => (ushort)((MaxHP() / 4)-1);

        protected Damageable() => damageActions = new Dictionary<Kernel_bin.Attack_Type, Func<int, Kernel_bin.Attack_Flags, int>>
        {
            { Kernel_bin.Attack_Type.Physical_Attack, Damage_Physical_Attack_Action },
            { Kernel_bin.Attack_Type.Magic_Attack, Damage_Magic_Attack_Action },
            { Kernel_bin.Attack_Type.Curative_Magic, Damage_Curative_Magic_Action },
            { Kernel_bin.Attack_Type.Curative_Item, Damage_Curative_Item_Action },
            { Kernel_bin.Attack_Type.Revive, Damage_Revive_Action },
            { Kernel_bin.Attack_Type.Revive_At_Full_HP, Damage_Revive_At_Full_HP_Action },
            { Kernel_bin.Attack_Type.Physical_Damage, Damage_Physical_Damage_Action },
            { Kernel_bin.Attack_Type.Magic_Damage, Damage_Magic_Damage_Action },
            { Kernel_bin.Attack_Type.Renzokuken_Finisher, Damage_Renzokuken_Finisher_Action },
            { Kernel_bin.Attack_Type.Squall_Gunblade_Attack, Damage_Squall_Gunblade_Attack_Action },
            { Kernel_bin.Attack_Type.GF, Damage_GF_Action },
            { Kernel_bin.Attack_Type.Scan, Damage_Scan_Action },
            { Kernel_bin.Attack_Type.LV_Down, Damage_LV_Down_Action },
            { Kernel_bin.Attack_Type.Summon_Item, Damage_Summon_Item_Action },
            { Kernel_bin.Attack_Type.GF_Ignore_Target_SPR, Damage_GF_Ignore_Target_SPR_Action },
            { Kernel_bin.Attack_Type.LV_Up, Damage_LV_Up_Action },
            { Kernel_bin.Attack_Type.Card, Damage_Card_Action },
            { Kernel_bin.Attack_Type.Kamikaze, Damage_Kamikaze_Action },
            { Kernel_bin.Attack_Type.Devour, Damage_Devour_Action },
            { Kernel_bin.Attack_Type.GF_Damage, Damage_GF_Damage_Action },
            { Kernel_bin.Attack_Type.Unknown_1, Damage_Unknown_1_Action },
            { Kernel_bin.Attack_Type.Magic_Attack_Ignore_Target_SPR, Damage_Magic_Attack_Ignore_Target_SPR_Action },
            { Kernel_bin.Attack_Type.Angelo_Search, Damage_Angelo_Search_Action },
            { Kernel_bin.Attack_Type.Moogle_Dance, Damage_Moogle_Dance_Action },
            { Kernel_bin.Attack_Type.White_WindQuistis, Damage_White_WindQuistis_Action },
            { Kernel_bin.Attack_Type.LV_Attack, Damage_LV_Attack_Action },
            { Kernel_bin.Attack_Type.Fixed_Damage, Damage_Fixed_Damage_Action },
            { Kernel_bin.Attack_Type.Target_Current_HP_1, Damage_Target_Current_HP_1_Action },
            { Kernel_bin.Attack_Type.Fixed_Magic_Damage_Based_on_GF_Level, Damage_Fixed_Magic_Damage_Based_on_GF_Level_Action },
            { Kernel_bin.Attack_Type.Unknown_2, Damage_Unknown_2_Action },
            { Kernel_bin.Attack_Type.Unknown_3, Damage_Unknown_3_Action },
            { Kernel_bin.Attack_Type.Give_Percentage_HP, Damage_Give_Percentage_HP_Action },
            { Kernel_bin.Attack_Type.Unknown_4, Damage_Unknown_4_Action },
            { Kernel_bin.Attack_Type.Everyones_Grudge, Damage_Everyones_Grudge_Action },
            { Kernel_bin.Attack_Type._1_HP_Damage, Damage__1_HP_Damage_Action },
            { Kernel_bin.Attack_Type.Physical_AttackIgnore_Target_VIT, Damage_Physical_AttackIgnore_Target_VIT_Action },
        };

        private int Damage_Physical_Attack_Action(int dmg, Kernel_bin.Attack_Flags flags) => throw new NotImplementedException();

        private int Damage_Magic_Attack_Action(int dmg, Kernel_bin.Attack_Flags flags) => throw new NotImplementedException();

        private int Damage_Curative_Magic_Action(int dmg, Kernel_bin.Attack_Flags flags) => throw new NotImplementedException();

        private int Damage_Curative_Item_Action(int dmg, Kernel_bin.Attack_Flags flags)
        {
            bool noheal = (Statuses0 & (Kernel_bin.Persistant_Statuses.Death | Kernel_bin.Persistant_Statuses.Petrify)) != 0 || (Statuses1 & (Kernel_bin.Battle_Only_Statuses.Summon_GF)) != 0 || _CurrentHP == 0;
            bool healisdmg = (Statuses0 & (Kernel_bin.Persistant_Statuses.Zombie)) != 0;
            if (!noheal)
            {
                dmg = (healisdmg ? dmg : -dmg);
            }
            changeHP(dmg);
            return changeHP(dmg) ? dmg : 0;
        }

        private int Damage_Revive_Action(int dmg, Kernel_bin.Attack_Flags flags)
        {
            if((Statuses0 & Kernel_bin.Persistant_Statuses.Death) !=0 || _CurrentHP == 0)
            {
                Statuses0 &= ~Kernel_bin.Persistant_Statuses.Death;
                _CurrentHP = ReviveHP();
                    return -_CurrentHP;
            }
            return 0;
        }

        public virtual ushort ReviveHP() => (ushort)(MaxHP() / 8);
        private int Damage_Revive_At_Full_HP_Action(int dmg, Kernel_bin.Attack_Flags flags)
        {
            if ((Statuses0 & Kernel_bin.Persistant_Statuses.Death) != 0 || _CurrentHP == 0)
            {
                Statuses0 &= ~Kernel_bin.Persistant_Statuses.Death;
                _CurrentHP = MaxHP();
                return -_CurrentHP;
            }
            return 0;
        }
        private int Damage_Physical_Damage_Action(int dmg, Kernel_bin.Attack_Flags flags) => throw new NotImplementedException();

        private int Damage_Magic_Damage_Action(int dmg, Kernel_bin.Attack_Flags flags) => throw new NotImplementedException();

        private int Damage_Renzokuken_Finisher_Action(int dmg, Kernel_bin.Attack_Flags flags) => throw new NotImplementedException();

        private int Damage_Squall_Gunblade_Attack_Action(int dmg, Kernel_bin.Attack_Flags flags) => throw new NotImplementedException();

        private int Damage_GF_Action(int dmg, Kernel_bin.Attack_Flags flags) => throw new NotImplementedException();

        private int Damage_Scan_Action(int dmg, Kernel_bin.Attack_Flags flags) => throw new NotImplementedException();

        private int Damage_LV_Down_Action(int dmg, Kernel_bin.Attack_Flags flags) => throw new NotImplementedException();

        private int Damage_Summon_Item_Action(int dmg, Kernel_bin.Attack_Flags flags) => throw new NotImplementedException();

        private int Damage_GF_Ignore_Target_SPR_Action(int dmg, Kernel_bin.Attack_Flags flags) => throw new NotImplementedException();

        private int Damage_LV_Up_Action(int dmg, Kernel_bin.Attack_Flags flags) => throw new NotImplementedException();

        private int Damage_Card_Action(int dmg, Kernel_bin.Attack_Flags flags) => throw new NotImplementedException();

        private int Damage_Kamikaze_Action(int dmg, Kernel_bin.Attack_Flags flags) => throw new NotImplementedException();

        private int Damage_Devour_Action(int dmg, Kernel_bin.Attack_Flags flags) => throw new NotImplementedException();

        private int Damage_GF_Damage_Action(int dmg, Kernel_bin.Attack_Flags flags) => throw new NotImplementedException();

        private int Damage_Unknown_1_Action(int dmg, Kernel_bin.Attack_Flags flags) => throw new NotImplementedException();

        private int Damage_Magic_Attack_Ignore_Target_SPR_Action(int dmg, Kernel_bin.Attack_Flags flags) => throw new NotImplementedException();

        private int Damage_Angelo_Search_Action(int dmg, Kernel_bin.Attack_Flags flags) => throw new NotImplementedException();

        private int Damage_Moogle_Dance_Action(int dmg, Kernel_bin.Attack_Flags flags) => throw new NotImplementedException();

        private int Damage_White_WindQuistis_Action(int dmg, Kernel_bin.Attack_Flags flags) => throw new NotImplementedException();

        private int Damage_LV_Attack_Action(int dmg, Kernel_bin.Attack_Flags flags) => throw new NotImplementedException();

        private int Damage_Fixed_Damage_Action(int dmg, Kernel_bin.Attack_Flags flags) => throw new NotImplementedException();

        private int Damage_Target_Current_HP_1_Action(int dmg, Kernel_bin.Attack_Flags flags) => throw new NotImplementedException();

        private int Damage_Fixed_Magic_Damage_Based_on_GF_Level_Action(int dmg, Kernel_bin.Attack_Flags flags) => throw new NotImplementedException();

        private int Damage_Unknown_2_Action(int dmg, Kernel_bin.Attack_Flags flags) => throw new NotImplementedException();

        private int Damage_Unknown_3_Action(int dmg, Kernel_bin.Attack_Flags flags) => throw new NotImplementedException();

        private int Damage_Give_Percentage_HP_Action(int dmg, Kernel_bin.Attack_Flags flags) => throw new NotImplementedException();

        private int Damage_Unknown_4_Action(int dmg, Kernel_bin.Attack_Flags flags) => throw new NotImplementedException();

        private int Damage_Everyones_Grudge_Action(int dmg, Kernel_bin.Attack_Flags flags) => throw new NotImplementedException();

        private int Damage__1_HP_Damage_Action(int dmg, Kernel_bin.Attack_Flags flags) => throw new NotImplementedException();

        private int Damage_Physical_AttackIgnore_Target_VIT_Action(int dmg, Kernel_bin.Attack_Flags flags) => throw new NotImplementedException();

        public bool DealDamage(int dmg, Kernel_bin.Attack_Type type, Kernel_bin.Attack_Flags flags)
        {
            // check max and min values
            if (dmg > Kernel_bin.MAX_HP_VALUE) dmg = Kernel_bin.MAX_HP_VALUE;
            if (dmg < 0) return false;
            // depending on damage type see if damage is correct
            dmg = damageActions[type](dmg, flags);
            return dmg != 0;
        }

        protected bool changeHP(int dmg)
        {
            if (dmg < 0 && (Statuses1 & Kernel_bin.Battle_Only_Statuses.Invincible) != 0)
                return false;
            int hp = _CurrentHP + dmg;
            if (hp == _CurrentHP) return false;
            else if (hp <= 0)
            {
                Statuses0 |= (Kernel_bin.Persistant_Statuses.Death);
                hp = 0;
            }
            else
            {
                _CurrentHP = (ushort)(hp > ushort.MaxValue ? ushort.MaxValue : hp);
                //use a CurrentHP method or property to adjust this to correct maxhp.
                //if teamlaguna the max hp can be different and each gf has there own.
            }
            return true;
        }

        public virtual float PercentFullHP() => (float)CurrentHP() / MaxHP();

        /// <summary>
        /// Override This: to check vs your max hp value.
        /// </summary>
        /// <returns></returns>
        public virtual ushort CurrentHP()
        {
            ushort max = MaxHP();
            if (_CurrentHP > max) _CurrentHP = max;
            return _CurrentHP;
        }

        /// <summary>
        /// If status immune statuses aren't set and return none.
        /// </summary>
        public bool StatusImmune { get; protected set; } = false;

        /// <summary>
        /// Persistant_Statuses are saved and last between battles.
        /// </summary>
        public virtual Kernel_bin.Persistant_Statuses Statuses0
        {
            get
            {
                if (!StatusImmune)
                    return _statuses0;
                else
                    return Kernel_bin.Persistant_Statuses.None;
            }

            set
            {
                if (!StatusImmune)
                    _statuses0 = value;
            }
        }

        /// <summary>
        /// Battle_Only_Statuses only exist in battle. Please set to None at end and/or start of battle.
        /// </summary>
        public virtual Kernel_bin.Battle_Only_Statuses Statuses1
        {
            get
            {
                if (!StatusImmune)
                    return _statuses1;
                else
                    return Kernel_bin.Battle_Only_Statuses.None;
            }
            set
            {
                if (!StatusImmune)
                    _statuses1 = value;
            }
        }

        /// <summary>
        /// Overload this: To get correct MaxHP value
        /// </summary>
        /// <returns></returns>
        public virtual ushort MaxHP() => ushort.MaxValue;
    }
}
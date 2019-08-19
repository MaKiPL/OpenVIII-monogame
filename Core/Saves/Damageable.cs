using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace OpenVIII
{
    public class Enemy : Damageable
    {
        public static List<Enemy> EnemyParty { get; set; } = new List<Enemy>(6) {
            new Enemy { Name = "Jellyeye" },
            new Enemy { Name = "Jellyeye" },
            new Enemy { Name = "Jellyeye" },
            new Enemy { Name = "Jellyeye" },
            new Enemy { Name = "Jellyeye" },
            new Enemy { Name = "Jellyeye" }, };
    }
    public abstract class Damageable
    {
        #region Fields

        /// <summary>
        /// Name
        /// </summary>
        /// <remarks>not saved to file</remarks>
        public FF8String Name { get; set; }

        protected ushort _CurrentHP;

        private Dictionary<Kernel_bin.Attack_Type, Func<int, Kernel_bin.Attack_Flags, int>> _damageActions;

        private Dictionary<Kernel_bin.Attack_Type, Func<Kernel_bin.Persistant_Statuses, Kernel_bin.Battle_Only_Statuses, Kernel_bin.Attack_Flags, int>> _statusesActions;

        private Kernel_bin.Persistant_Statuses _statuses0;

        private Kernel_bin.Battle_Only_Statuses _statuses1;

        #endregion Fields

        #region Properties

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
        /// If status immune statuses aren't set and return none.
        /// </summary>
        public bool StatusImmune { get; protected set; } = false;

        public IReadOnlyDictionary<Kernel_bin.Attack_Type, Func<int, Kernel_bin.Attack_Flags, int>> DamageActions
        {
            get
            {
                if (_damageActions == null)
                    _damageActions = new Dictionary<Kernel_bin.Attack_Type, Func<int, Kernel_bin.Attack_Flags, int>>
            {   { Kernel_bin.Attack_Type.Physical_Attack, Damage_Physical_Attack_Action},
                { Kernel_bin.Attack_Type.Magic_Attack, Damage_Magic_Attack_Action},
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

                return _damageActions;
            }
        }

        public IReadOnlyDictionary<Kernel_bin.Attack_Type, Func<Kernel_bin.Persistant_Statuses, Kernel_bin.Battle_Only_Statuses, Kernel_bin.Attack_Flags, int>> StatusesActions
        {
            get
            {
                if (_statusesActions == null)
                    _statusesActions = new Dictionary<Kernel_bin.Attack_Type, Func<Kernel_bin.Persistant_Statuses, Kernel_bin.Battle_Only_Statuses, Kernel_bin.Attack_Flags, int>>
            {
                { Kernel_bin.Attack_Type.Physical_Attack, Statuses_Physical_Attack_Action },
                { Kernel_bin.Attack_Type.Magic_Attack, Statuses_Magic_Attack_Action },
                { Kernel_bin.Attack_Type.Curative_Magic, Statuses_Curative_Magic_Action },
                { Kernel_bin.Attack_Type.Curative_Item, Statuses_Curative_Item_Action },
                { Kernel_bin.Attack_Type.Revive, Statuses_Revive_Action },
                { Kernel_bin.Attack_Type.Revive_At_Full_HP, Statuses_Revive_At_Full_HP_Action },
                { Kernel_bin.Attack_Type.Physical_Damage, Statuses_Physical_Statuses_Action },
                { Kernel_bin.Attack_Type.Magic_Damage, Statuses_Magic_Statuses_Action },
                { Kernel_bin.Attack_Type.Renzokuken_Finisher, Statuses_Renzokuken_Finisher_Action },
                { Kernel_bin.Attack_Type.Squall_Gunblade_Attack, Statuses_Squall_Gunblade_Attack_Action },
                { Kernel_bin.Attack_Type.GF, Statuses_GF_Action },
                { Kernel_bin.Attack_Type.Scan, Statuses_Scan_Action },
                { Kernel_bin.Attack_Type.LV_Down, Statuses_LV_Down_Action },
                { Kernel_bin.Attack_Type.Summon_Item, Statuses_Summon_Item_Action },
                { Kernel_bin.Attack_Type.GF_Ignore_Target_SPR, Statuses_GF_Ignore_Target_SPR_Action },
                { Kernel_bin.Attack_Type.LV_Up, Statuses_LV_Up_Action },
                { Kernel_bin.Attack_Type.Card, Statuses_Card_Action },
                { Kernel_bin.Attack_Type.Kamikaze, Statuses_Kamikaze_Action },
                { Kernel_bin.Attack_Type.Devour, Statuses_Devour_Action },
                { Kernel_bin.Attack_Type.GF_Damage, Statuses_GF_Statuses_Action },
                { Kernel_bin.Attack_Type.Unknown_1, Statuses_Unknown_1_Action },
                { Kernel_bin.Attack_Type.Magic_Attack_Ignore_Target_SPR, Statuses_Magic_Attack_Ignore_Target_SPR_Action },
                { Kernel_bin.Attack_Type.Angelo_Search, Statuses_Angelo_Search_Action },
                { Kernel_bin.Attack_Type.Moogle_Dance, Statuses_Moogle_Dance_Action },
                { Kernel_bin.Attack_Type.White_WindQuistis, Statuses_White_WindQuistis_Action },
                { Kernel_bin.Attack_Type.LV_Attack, Statuses_LV_Attack_Action },
                { Kernel_bin.Attack_Type.Fixed_Damage, Statuses_Fixed_Statuses_Action },
                { Kernel_bin.Attack_Type.Target_Current_HP_1, Statuses_Target_Current_HP_1_Action },
                { Kernel_bin.Attack_Type.Fixed_Magic_Damage_Based_on_GF_Level, Statuses_Fixed_Magic_Statuses_Based_on_GF_Level_Action },
                { Kernel_bin.Attack_Type.Unknown_2, Statuses_Unknown_2_Action },
                { Kernel_bin.Attack_Type.Unknown_3, Statuses_Unknown_3_Action },
                { Kernel_bin.Attack_Type.Give_Percentage_HP, Statuses_Give_Percentage_HP_Action },
                { Kernel_bin.Attack_Type.Unknown_4, Statuses_Unknown_4_Action },
                { Kernel_bin.Attack_Type.Everyones_Grudge, Statuses_Everyones_Grudge_Action },
                { Kernel_bin.Attack_Type._1_HP_Damage, Statuses__1_HP_Statuses_Action },
                { Kernel_bin.Attack_Type.Physical_AttackIgnore_Target_VIT, Statuses_Physical_AttackIgnore_Target_VIT_Action },
            };
                return _statusesActions;
            }
        }

        #endregion Properties

        #region Methods

        public virtual bool Critical() => CurrentHP() <= CriticalHP();

        public virtual ushort CriticalHP() => (ushort)((MaxHP() / 4) - 1);

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

        public bool DealDamage(int dmg, Kernel_bin.Attack_Type type, Kernel_bin.Attack_Flags? flags)
        {
            if (DamageActions.ContainsKey(type))
            {
                //ChangeHP(-dmg);
                // check max and min values
                if (dmg > Kernel_bin.MAX_HP_VALUE && type != Kernel_bin.Attack_Type.Give_Percentage_HP) dmg = Kernel_bin.MAX_HP_VALUE;
                if (dmg < 0) return false;
                // depending on damage type see if damage is correct
                dmg = DamageActions[type](dmg, flags ?? Kernel_bin.Attack_Flags.None);

                return dmg != 0;
            }
            return false;
        }

        public bool DealStatus(
            Kernel_bin.Persistant_Statuses? statuses0,
            Kernel_bin.Battle_Only_Statuses? statuses1,
            Kernel_bin.Attack_Type type,
            Kernel_bin.Attack_Flags? flags)
        {
            if (StatusesActions.ContainsKey(type))
            {
                int total = StatusesActions[type](
                statuses0 ?? Kernel_bin.Persistant_Statuses.None,
                statuses1 ?? Kernel_bin.Battle_Only_Statuses.None,
                flags ?? Kernel_bin.Attack_Flags.None);
                return total != 0;
            }
            return false;
        }

        /// <summary>
        /// Overload this: To get correct MaxHP value
        /// </summary>
        /// <returns></returns>
        public virtual ushort MaxHP() => ushort.MaxValue;

        public virtual float PercentFullHP() => (float)CurrentHP() / MaxHP();

        public virtual ushort ReviveHP() => (ushort)(MaxHP() / 8);

        public bool ChangeHP(int dmg)
        {
            if (dmg == 0 || (Statuses1 & Kernel_bin.Battle_Only_Statuses.Invincible) != 0)
                return false;
            int hp = _CurrentHP - dmg;
            ushort lasthp = _CurrentHP;
            if (hp <= 0)
            {
                Statuses0 |= (Kernel_bin.Persistant_Statuses.Death);
                _CurrentHP = 0;
            }
            else
            {
                _CurrentHP = (ushort)(hp > ushort.MaxValue ? ushort.MaxValue : hp);
                //use a CurrentHP method or property to adjust this to correct maxhp.
                //if teamlaguna the max hp can be different and each gf has there own.
            }
            CurrentHP();
            if (lasthp == _CurrentHP) return false;
            Debug.WriteLine($"{this}: Dealt {dmg}, previous hp: {lasthp}, current hp: {_CurrentHP}");
            return true;
        }
        public bool IsDead => CurrentHP() == 0 || (Statuses0 & Kernel_bin.Persistant_Statuses.Death) != 0;
        public bool IsPetrify => (Statuses0 & (Kernel_bin.Persistant_Statuses.Petrify)) != 0;
        /// <summary>
        /// If all partymemembers are in gameover trigger Phoenix Pinion if CanPhoenixPinion or trigger Game over
        /// </summary>
        public bool IsGameOver => IsDead || IsPetrify || (Statuses1 & (Kernel_bin.Battle_Only_Statuses.Eject)) != 0;

        /// <summary>
        /// 25.4% chance to cast automaticly on gameover, if used once in battle
        /// </summary>
        /// <remarks>Memory.State.Fieldvars. has a value that tracks if PhoenixPinion is used just need to find it</remarks>
        public bool CanPhoenixPinion => IsDead && !(IsPetrify || (Statuses1 & (Kernel_bin.Battle_Only_Statuses.Eject)) != 0) && Memory.State.Items.Where(m => m.ID == 31 && m.QTY >= 1).Count() > 0;
        /// <summary>
        /// ATB frozen
        /// </summary>
        public bool IsInactive => IsGameOver ||
            (Statuses1 & (Kernel_bin.Battle_Only_Statuses.Stop))!=0;
        /// <summary>
        /// Menu disabled
        /// </summary>
        public bool IsNonInteractive => IsInactive ||
            (Statuses0 & Kernel_bin.Persistant_Statuses.Berserk) != 0;
        private int Damage__1_HP_Damage_Action(int dmg, Kernel_bin.Attack_Flags flags) => throw new NotImplementedException();

        private int Damage_Angelo_Search_Action(int dmg, Kernel_bin.Attack_Flags flags) => throw new NotImplementedException();

        private int Damage_Card_Action(int dmg, Kernel_bin.Attack_Flags flags) => throw new NotImplementedException();

        private int Damage_Curative_Item_Action(int dmg, Kernel_bin.Attack_Flags flags)
        {
            //ChangeHP(-dmg);
            bool noheal = (Statuses0 & (Kernel_bin.Persistant_Statuses.Death | Kernel_bin.Persistant_Statuses.Petrify)) != 0 || (Statuses1 & (Kernel_bin.Battle_Only_Statuses.Summon_GF)) != 0 || _CurrentHP == 0;
            bool healisdmg = (Statuses0 & (Kernel_bin.Persistant_Statuses.Zombie)) != 0;
            if (!noheal)
            {
                dmg = (healisdmg ? dmg : -dmg);
            }
            return ChangeHP(dmg) ? dmg : 0;
        }

        private int Damage_Curative_Magic_Action(int dmg, Kernel_bin.Attack_Flags flags) => throw new NotImplementedException();

        private int Damage_Devour_Action(int dmg, Kernel_bin.Attack_Flags flags) => throw new NotImplementedException();

        private int Damage_Everyones_Grudge_Action(int dmg, Kernel_bin.Attack_Flags flags) => throw new NotImplementedException();

        private int Damage_Fixed_Damage_Action(int dmg, Kernel_bin.Attack_Flags flags) => throw new NotImplementedException();

        private int Damage_Fixed_Magic_Damage_Based_on_GF_Level_Action(int dmg, Kernel_bin.Attack_Flags flags) => throw new NotImplementedException();

        private int Damage_GF_Action(int dmg, Kernel_bin.Attack_Flags flags) => throw new NotImplementedException();

        private int Damage_GF_Damage_Action(int dmg, Kernel_bin.Attack_Flags flags) => throw new NotImplementedException();

        private int Damage_GF_Ignore_Target_SPR_Action(int dmg, Kernel_bin.Attack_Flags flags) => throw new NotImplementedException();

        private int Damage_Give_Percentage_HP_Action(int dmg, Kernel_bin.Attack_Flags flags) => Damage_Curative_Item_Action(MaxHP() * dmg / 100, flags);

        private int Damage_Kamikaze_Action(int dmg, Kernel_bin.Attack_Flags flags) => throw new NotImplementedException();

        private int Damage_LV_Attack_Action(int dmg, Kernel_bin.Attack_Flags flags) => throw new NotImplementedException();

        private int Damage_LV_Down_Action(int dmg, Kernel_bin.Attack_Flags flags) => throw new NotImplementedException();

        private int Damage_LV_Up_Action(int dmg, Kernel_bin.Attack_Flags flags) => throw new NotImplementedException();

        private int Damage_Magic_Attack_Action(int dmg, Kernel_bin.Attack_Flags flags) => throw new NotImplementedException();

        private int Damage_Magic_Attack_Ignore_Target_SPR_Action(int dmg, Kernel_bin.Attack_Flags flags) => throw new NotImplementedException();

        private int Damage_Magic_Damage_Action(int dmg, Kernel_bin.Attack_Flags flags) => throw new NotImplementedException();

        private int Damage_Moogle_Dance_Action(int dmg, Kernel_bin.Attack_Flags flags) => throw new NotImplementedException();

        private int Damage_Physical_Attack_Action(int dmg, Kernel_bin.Attack_Flags flags) => throw new NotImplementedException();

        private int Damage_Physical_AttackIgnore_Target_VIT_Action(int dmg, Kernel_bin.Attack_Flags flags) => throw new NotImplementedException();

        private int Damage_Physical_Damage_Action(int dmg, Kernel_bin.Attack_Flags flags) => throw new NotImplementedException();

        private int Damage_Renzokuken_Finisher_Action(int dmg, Kernel_bin.Attack_Flags flags) => throw new NotImplementedException();

        private int Damage_Revive_Action(int dmg, Kernel_bin.Attack_Flags flags)
        {

            ushort r = ReviveHP();

            if ((Statuses0 & Kernel_bin.Persistant_Statuses.Zombie) != 0)
            {
                r = MaxHP();
                //Debug.WriteLine($"{this}: Dealt {r}, previous hp: {_CurrentHP}, current hp: {_CurrentHP - r}");
                return Damage_Curative_Item_Action(r, flags);
            }
            else if (_CurrentHP != r)
            {
                Debug.WriteLine($"{this}: Dealt {-r}, previous hp: {0}, current hp: {r}");
                return _CurrentHP = r;
            }
            return 0;
        }

        private int Damage_Revive_At_Full_HP_Action(int dmg, Kernel_bin.Attack_Flags flags)
        {
            ushort r = MaxHP();
            if ((Statuses0 & Kernel_bin.Persistant_Statuses.Zombie) != 0)
            { 
                //Debug.WriteLine($"{this}: Dealt {r}, previous hp: {_CurrentHP}, current hp: {_CurrentHP-r}");
                return Damage_Curative_Item_Action(MaxHP(), flags);
            }
            if (_CurrentHP != r)
            {
                Debug.WriteLine($"{this}: Dealt {-r}, previous hp: {0}, current hp: {r}");
                return _CurrentHP = r;
            }
            return 0;
        }

        private int Damage_Scan_Action(int dmg, Kernel_bin.Attack_Flags flags) => throw new NotImplementedException();

        private int Damage_Squall_Gunblade_Attack_Action(int dmg, Kernel_bin.Attack_Flags flags) => throw new NotImplementedException();

        private int Damage_Summon_Item_Action(int dmg, Kernel_bin.Attack_Flags flags) => throw new NotImplementedException();

        private int Damage_Target_Current_HP_1_Action(int dmg, Kernel_bin.Attack_Flags flags) => throw new NotImplementedException();

        private int Damage_Unknown_1_Action(int dmg, Kernel_bin.Attack_Flags flags) => throw new NotImplementedException();

        private int Damage_Unknown_2_Action(int dmg, Kernel_bin.Attack_Flags flags) => throw new NotImplementedException();

        private int Damage_Unknown_3_Action(int dmg, Kernel_bin.Attack_Flags flags) => throw new NotImplementedException();

        private int Damage_Unknown_4_Action(int dmg, Kernel_bin.Attack_Flags flags) => throw new NotImplementedException();

        private int Damage_White_WindQuistis_Action(int dmg, Kernel_bin.Attack_Flags flags) => throw new NotImplementedException();

        private int Statuses__1_HP_Statuses_Action(Kernel_bin.Persistant_Statuses statuses0, Kernel_bin.Battle_Only_Statuses statuses1, Kernel_bin.Attack_Flags flags) => throw new NotImplementedException();

        private int Statuses_Angelo_Search_Action(Kernel_bin.Persistant_Statuses statuses0, Kernel_bin.Battle_Only_Statuses statuses1, Kernel_bin.Attack_Flags flags) => throw new NotImplementedException();

        private int Statuses_Card_Action(Kernel_bin.Persistant_Statuses statuses0, Kernel_bin.Battle_Only_Statuses statuses1, Kernel_bin.Attack_Flags flags) => throw new NotImplementedException();

        private int Statuses_Curative_Item_Action(Kernel_bin.Persistant_Statuses statuses0, Kernel_bin.Battle_Only_Statuses statuses1, Kernel_bin.Attack_Flags flags)
        {
            Kernel_bin.Persistant_Statuses bak0 = Statuses0;
            Kernel_bin.Battle_Only_Statuses bak1 = Statuses1;
            Statuses0 &= ~statuses0;
            Statuses1 &= ~statuses1;
            if (!bak0.Equals(Statuses0) || !bak1.Equals(Statuses1))
                return 1;
            return 0;
        }

        private int Statuses_Curative_Magic_Action(Kernel_bin.Persistant_Statuses statuses0, Kernel_bin.Battle_Only_Statuses statuses1, Kernel_bin.Attack_Flags flags) => throw new NotImplementedException();

        private int Statuses_Devour_Action(Kernel_bin.Persistant_Statuses statuses0, Kernel_bin.Battle_Only_Statuses statuses1, Kernel_bin.Attack_Flags flags) => throw new NotImplementedException();

        private int Statuses_Everyones_Grudge_Action(Kernel_bin.Persistant_Statuses statuses0, Kernel_bin.Battle_Only_Statuses statuses1, Kernel_bin.Attack_Flags flags) => throw new NotImplementedException();

        private int Statuses_Fixed_Magic_Statuses_Based_on_GF_Level_Action(Kernel_bin.Persistant_Statuses statuses0, Kernel_bin.Battle_Only_Statuses statuses1, Kernel_bin.Attack_Flags flags) => throw new NotImplementedException();

        private int Statuses_Fixed_Statuses_Action(Kernel_bin.Persistant_Statuses statuses0, Kernel_bin.Battle_Only_Statuses statuses1, Kernel_bin.Attack_Flags flags) => throw new NotImplementedException();

        private int Statuses_GF_Action(Kernel_bin.Persistant_Statuses statuses0, Kernel_bin.Battle_Only_Statuses statuses1, Kernel_bin.Attack_Flags flags) => throw new NotImplementedException();

        private int Statuses_GF_Ignore_Target_SPR_Action(Kernel_bin.Persistant_Statuses statuses0, Kernel_bin.Battle_Only_Statuses statuses1, Kernel_bin.Attack_Flags flags) => throw new NotImplementedException();

        private int Statuses_GF_Statuses_Action(Kernel_bin.Persistant_Statuses statuses0, Kernel_bin.Battle_Only_Statuses statuses1, Kernel_bin.Attack_Flags flags) => throw new NotImplementedException();

        private int Statuses_Give_Percentage_HP_Action(Kernel_bin.Persistant_Statuses statuses0, Kernel_bin.Battle_Only_Statuses statuses1, Kernel_bin.Attack_Flags flags) =>
            Statuses_Curative_Item_Action(statuses0, Statuses1, flags);

        private int Statuses_Kamikaze_Action(Kernel_bin.Persistant_Statuses statuses0, Kernel_bin.Battle_Only_Statuses statuses1, Kernel_bin.Attack_Flags flags) => throw new NotImplementedException();

        private int Statuses_LV_Attack_Action(Kernel_bin.Persistant_Statuses statuses0, Kernel_bin.Battle_Only_Statuses statuses1, Kernel_bin.Attack_Flags flags) => throw new NotImplementedException();

        private int Statuses_LV_Down_Action(Kernel_bin.Persistant_Statuses statuses0, Kernel_bin.Battle_Only_Statuses statuses1, Kernel_bin.Attack_Flags flags) => throw new NotImplementedException();

        private int Statuses_LV_Up_Action(Kernel_bin.Persistant_Statuses statuses0, Kernel_bin.Battle_Only_Statuses statuses1, Kernel_bin.Attack_Flags flags) => throw new NotImplementedException();

        private int Statuses_Magic_Attack_Action(Kernel_bin.Persistant_Statuses statuses0, Kernel_bin.Battle_Only_Statuses statuses1, Kernel_bin.Attack_Flags flags) => throw new NotImplementedException();

        private int Statuses_Magic_Attack_Ignore_Target_SPR_Action(Kernel_bin.Persistant_Statuses statuses0, Kernel_bin.Battle_Only_Statuses statuses1, Kernel_bin.Attack_Flags flags) => throw new NotImplementedException();

        private int Statuses_Magic_Statuses_Action(Kernel_bin.Persistant_Statuses statuses0, Kernel_bin.Battle_Only_Statuses statuses1, Kernel_bin.Attack_Flags flags) => throw new NotImplementedException();

        private int Statuses_Moogle_Dance_Action(Kernel_bin.Persistant_Statuses statuses0, Kernel_bin.Battle_Only_Statuses statuses1, Kernel_bin.Attack_Flags flags) => throw new NotImplementedException();

        private int Statuses_Physical_Attack_Action(Kernel_bin.Persistant_Statuses statuses0, Kernel_bin.Battle_Only_Statuses statuses1, Kernel_bin.Attack_Flags flags) => throw new NotImplementedException();

        private int Statuses_Physical_AttackIgnore_Target_VIT_Action(Kernel_bin.Persistant_Statuses statuses0, Kernel_bin.Battle_Only_Statuses statuses1, Kernel_bin.Attack_Flags flags) => throw new NotImplementedException();

        private int Statuses_Physical_Statuses_Action(Kernel_bin.Persistant_Statuses statuses0, Kernel_bin.Battle_Only_Statuses statuses1, Kernel_bin.Attack_Flags flags) => throw new NotImplementedException();

        private int Statuses_Renzokuken_Finisher_Action(Kernel_bin.Persistant_Statuses statuses0, Kernel_bin.Battle_Only_Statuses statuses1, Kernel_bin.Attack_Flags flags) => throw new NotImplementedException();

        private int Statuses_Revive_Action(Kernel_bin.Persistant_Statuses statuses0, Kernel_bin.Battle_Only_Statuses statuses1, Kernel_bin.Attack_Flags flags)
        {
            if ((Statuses0 & Kernel_bin.Persistant_Statuses.Death) != 0 || _CurrentHP == 0)
            {
                Statuses0 = Kernel_bin.Persistant_Statuses.None;
                Statuses1 = Kernel_bin.Battle_Only_Statuses.None;
                _CurrentHP = 0;
                return 1;
            }
            return 0;
        }

        private int Statuses_Revive_At_Full_HP_Action(Kernel_bin.Persistant_Statuses statuses0, Kernel_bin.Battle_Only_Statuses statuses1, Kernel_bin.Attack_Flags flags) =>
            Statuses_Revive_Action(statuses0, statuses1, flags);

        private int Statuses_Scan_Action(Kernel_bin.Persistant_Statuses statuses0, Kernel_bin.Battle_Only_Statuses statuses1, Kernel_bin.Attack_Flags flags) => throw new NotImplementedException();

        private int Statuses_Squall_Gunblade_Attack_Action(Kernel_bin.Persistant_Statuses statuses0, Kernel_bin.Battle_Only_Statuses statuses1, Kernel_bin.Attack_Flags flags) => throw new NotImplementedException();

        private int Statuses_Summon_Item_Action(Kernel_bin.Persistant_Statuses statuses0, Kernel_bin.Battle_Only_Statuses statuses1, Kernel_bin.Attack_Flags flags) => throw new NotImplementedException();

        private int Statuses_Target_Current_HP_1_Action(Kernel_bin.Persistant_Statuses statuses0, Kernel_bin.Battle_Only_Statuses statuses1, Kernel_bin.Attack_Flags flags) => throw new NotImplementedException();

        private int Statuses_Unknown_1_Action(Kernel_bin.Persistant_Statuses statuses0, Kernel_bin.Battle_Only_Statuses statuses1, Kernel_bin.Attack_Flags flags) => throw new NotImplementedException();

        private int Statuses_Unknown_2_Action(Kernel_bin.Persistant_Statuses statuses0, Kernel_bin.Battle_Only_Statuses statuses1, Kernel_bin.Attack_Flags flags) => throw new NotImplementedException();

        private int Statuses_Unknown_3_Action(Kernel_bin.Persistant_Statuses statuses0, Kernel_bin.Battle_Only_Statuses statuses1, Kernel_bin.Attack_Flags flags) => throw new NotImplementedException();

        private int Statuses_Unknown_4_Action(Kernel_bin.Persistant_Statuses statuses0, Kernel_bin.Battle_Only_Statuses statuses1, Kernel_bin.Attack_Flags flags) => throw new NotImplementedException();

        private int Statuses_White_WindQuistis_Action(Kernel_bin.Persistant_Statuses statuses0, Kernel_bin.Battle_Only_Statuses statuses1, Kernel_bin.Attack_Flags flags) => throw new NotImplementedException();

        #endregion Methods
    }
}
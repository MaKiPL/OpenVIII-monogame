using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace OpenVIII
{
    /// <summary>
    /// Character/Enemy/GF that can be damaged or die.
    /// </summary>
    public abstract class Damageable : IDamageable
    {
        #region Fields

        protected ushort _CurrentHP;
        private Enum _battlemode;
        private Dictionary<Kernel_bin.Attack_Type, Func<int, Kernel_bin.Attack_Flags, int>> _damageActions;
        private Kernel_bin.Persistent_Statuses _statuses0;
        private Kernel_bin.Battle_Only_Statuses _statuses1;
        private Dictionary<Kernel_bin.Attack_Type, Func<Kernel_bin.Persistent_Statuses, Kernel_bin.Battle_Only_Statuses, Kernel_bin.Attack_Flags, int>> _statusesActions;
        private ATBTimer ATBTimer;

        #endregion Fields

        #region Constructors

        protected Damageable()
        {
        }

        #endregion Constructors

        #region Events

        public event EventHandler<Enum> BattleModeChangeEventHandler;

        #endregion Events

        #region Enums

        public enum BattleMode : byte
        {
            /// <summary>
            /// ATB bar filling
            /// </summary>
            /// <remarks>Orange Bar precent filled. ATB/Full ATB</remarks>
            ATB_Charging,

            /// <summary>
            /// ATB bar charged, waiting for your turn
            /// </summary>
            /// <remarks>Yellow Bar</remarks>
            ATB_Charged,

            /// <summary>
            /// Your turn
            /// </summary>
            /// <remarks>Yellow Bar/Name/HP Blinking</remarks>
            YourTurn,

            /// <summary>
            /// GF Cast
            /// </summary>
            /// <remarks>Show GF name/hp and blueish bar.</remarks>
            GF_Charging,

            EndTurn,
        }

        #endregion Enums

        #region Properties

        /// <summary>
        /// Max bar value
        /// </summary>
        /// <see cref="https://gamefaqs.gamespot.com/ps/197343-final-fantasy-viii/faqs/58936"/>
        public virtual int ATBBarSize => (int)Memory.CurrentBattleSpeed * 4000;

        public float ATBPercent => ATBTimer.Percent;

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
                int Damage__1_HP_Damage_Action(int dmg, Kernel_bin.Attack_Flags flags) => throw new NotImplementedException();

                int Damage_Angelo_Search_Action(int dmg, Kernel_bin.Attack_Flags flags) => throw new NotImplementedException();

                int Damage_Card_Action(int dmg, Kernel_bin.Attack_Flags flags) => throw new NotImplementedException();

                int Damage_Curative_Item_Action(int dmg, Kernel_bin.Attack_Flags flags)
                {
                    //ChangeHP(-dmg);
                    bool noheal = (Statuses0 & (Kernel_bin.Persistent_Statuses.Death | Kernel_bin.Persistent_Statuses.Petrify)) != 0 || (Statuses1 & (Kernel_bin.Battle_Only_Statuses.Summon_GF)) != 0 || _CurrentHP == 0;
                    bool healisdmg = (Statuses0 & (Kernel_bin.Persistent_Statuses.Zombie)) != 0;
                    if (!noheal)
                    {
                        dmg = (healisdmg ? dmg : -dmg);
                    }
                    return ChangeHP(dmg) ? dmg : 0;
                }

                int Damage_Curative_Magic_Action(int dmg, Kernel_bin.Attack_Flags flags) => throw new NotImplementedException();

                int Damage_Devour_Action(int dmg, Kernel_bin.Attack_Flags flags) => throw new NotImplementedException();

                int Damage_Everyones_Grudge_Action(int dmg, Kernel_bin.Attack_Flags flags) => throw new NotImplementedException();

                int Damage_Fixed_Damage_Action(int dmg, Kernel_bin.Attack_Flags flags) => throw new NotImplementedException();

                int Damage_Fixed_Magic_Damage_Based_on_GF_Level_Action(int dmg, Kernel_bin.Attack_Flags flags) => throw new NotImplementedException();

                int Damage_GF_Action(int dmg, Kernel_bin.Attack_Flags flags) => throw new NotImplementedException();

                int Damage_GF_Damage_Action(int dmg, Kernel_bin.Attack_Flags flags) => throw new NotImplementedException();

                int Damage_GF_Ignore_Target_SPR_Action(int dmg, Kernel_bin.Attack_Flags flags) => throw new NotImplementedException();

                int Damage_Give_Percentage_HP_Action(int dmg, Kernel_bin.Attack_Flags flags) => Damage_Curative_Item_Action(MaxHP() * dmg / 100, flags);

                int Damage_Kamikaze_Action(int dmg, Kernel_bin.Attack_Flags flags) => throw new NotImplementedException();

                int Damage_LV_Attack_Action(int dmg, Kernel_bin.Attack_Flags flags) => throw new NotImplementedException();

                int Damage_LV_Down_Action(int dmg, Kernel_bin.Attack_Flags flags) => throw new NotImplementedException();

                int Damage_LV_Up_Action(int dmg, Kernel_bin.Attack_Flags flags) => throw new NotImplementedException();

                int Damage_Magic_Attack_Action(int dmg, Kernel_bin.Attack_Flags flags) => throw new NotImplementedException();

                int Damage_Magic_Attack_Ignore_Target_SPR_Action(int dmg, Kernel_bin.Attack_Flags flags) => throw new NotImplementedException();

                int Damage_Magic_Damage_Action(int dmg, Kernel_bin.Attack_Flags flags) => throw new NotImplementedException();

                int Damage_Moogle_Dance_Action(int dmg, Kernel_bin.Attack_Flags flags) => throw new NotImplementedException();

                int Damage_Physical_Attack_Action(int dmg, Kernel_bin.Attack_Flags flags) => throw new NotImplementedException();

                int Damage_Physical_AttackIgnore_Target_VIT_Action(int dmg, Kernel_bin.Attack_Flags flags) => throw new NotImplementedException();

                int Damage_Physical_Damage_Action(int dmg, Kernel_bin.Attack_Flags flags) => throw new NotImplementedException();

                int Damage_Renzokuken_Finisher_Action(int dmg, Kernel_bin.Attack_Flags flags) => throw new NotImplementedException();

                int Damage_Revive_Action(int dmg, Kernel_bin.Attack_Flags flags)
                {
                    ushort r = ReviveHP();

                    if ((Statuses0 & Kernel_bin.Persistent_Statuses.Zombie) != 0)
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

                int Damage_Revive_At_Full_HP_Action(int dmg, Kernel_bin.Attack_Flags flags)
                {
                    ushort r = MaxHP();
                    if ((Statuses0 & Kernel_bin.Persistent_Statuses.Zombie) != 0)
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

                int Damage_Scan_Action(int dmg, Kernel_bin.Attack_Flags flags) => throw new NotImplementedException();

                int Damage_Squall_Gunblade_Attack_Action(int dmg, Kernel_bin.Attack_Flags flags) => throw new NotImplementedException();

                int Damage_Summon_Item_Action(int dmg, Kernel_bin.Attack_Flags flags) => throw new NotImplementedException();

                int Damage_Target_Current_HP_1_Action(int dmg, Kernel_bin.Attack_Flags flags) => throw new NotImplementedException();

                int Damage_Unknown_1_Action(int dmg, Kernel_bin.Attack_Flags flags) => throw new NotImplementedException();

                int Damage_Unknown_2_Action(int dmg, Kernel_bin.Attack_Flags flags) => throw new NotImplementedException();

                int Damage_Unknown_3_Action(int dmg, Kernel_bin.Attack_Flags flags) => throw new NotImplementedException();

                int Damage_Unknown_4_Action(int dmg, Kernel_bin.Attack_Flags flags) => throw new NotImplementedException();

                int Damage_White_WindQuistis_Action(int dmg, Kernel_bin.Attack_Flags flags) => throw new NotImplementedException();
            }
        }

        public abstract byte EVA { get; }

        public abstract int EXP { get; }

        public abstract byte HIT { get; }

        public ushort HP => CurrentHP();

        public bool IsDead => CurrentHP() == 0 || (Statuses0 & Kernel_bin.Persistent_Statuses.Death) != 0;

        /// <summary>
        /// If all partymemembers are in gameover trigger Phoenix Pinion if CanPhoenixPinion or
        /// trigger Game over
        /// </summary>
        public bool IsGameOver => IsDead || IsPetrify || (Statuses1 & (Kernel_bin.Battle_Only_Statuses.Eject)) != 0;

        /// <summary>
        /// ATB frozen
        /// </summary>
        public bool IsInactive => IsGameOver ||
            (Statuses1 & (Kernel_bin.Battle_Only_Statuses.Stop)) != 0;

        /// <summary>
        /// Menu disabled
        /// </summary>
        public bool IsNonInteractive => IsInactive ||
            (Statuses0 & Kernel_bin.Persistent_Statuses.Berserk) != 0;

        public bool IsPetrify => (Statuses0 & (Kernel_bin.Persistent_Statuses.Petrify)) != 0;

        public abstract byte Level { get; }

        public abstract byte LUCK { get; }

        public abstract byte MAG { get; }

        /// <summary>
        /// Name
        /// </summary>
        /// <remarks>not saved to file</remarks>
        public virtual FF8String Name { get; set; }

        public abstract byte SPD { get; }

        public abstract byte SPR { get; }

        /// <summary>
        /// Persistant_Statuses are saved and last between battles.
        /// </summary>
        public virtual Kernel_bin.Persistent_Statuses Statuses0
        {
            get
            {
                if (!StatusImmune)
                    return _statuses0;
                else
                    return Kernel_bin.Persistent_Statuses.None;
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

        public IReadOnlyDictionary<Kernel_bin.Attack_Type, Func<Kernel_bin.Persistent_Statuses, Kernel_bin.Battle_Only_Statuses, Kernel_bin.Attack_Flags, int>> StatusesActions
        {
            get
            {
                if (_statusesActions == null)
                    _statusesActions = new Dictionary<Kernel_bin.Attack_Type, Func<Kernel_bin.Persistent_Statuses, Kernel_bin.Battle_Only_Statuses, Kernel_bin.Attack_Flags, int>>
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

                int Statuses__1_HP_Statuses_Action(Kernel_bin.Persistent_Statuses statuses0, Kernel_bin.Battle_Only_Statuses statuses1, Kernel_bin.Attack_Flags flags) => throw new NotImplementedException();

                int Statuses_Angelo_Search_Action(Kernel_bin.Persistent_Statuses statuses0, Kernel_bin.Battle_Only_Statuses statuses1, Kernel_bin.Attack_Flags flags) => throw new NotImplementedException();

                int Statuses_Card_Action(Kernel_bin.Persistent_Statuses statuses0, Kernel_bin.Battle_Only_Statuses statuses1, Kernel_bin.Attack_Flags flags) => throw new NotImplementedException();

                int Statuses_Curative_Item_Action(Kernel_bin.Persistent_Statuses statuses0, Kernel_bin.Battle_Only_Statuses statuses1, Kernel_bin.Attack_Flags flags)
                {
                    Kernel_bin.Persistent_Statuses bak0 = Statuses0;
                    Kernel_bin.Battle_Only_Statuses bak1 = Statuses1;
                    Statuses0 &= ~statuses0;
                    Statuses1 &= ~statuses1;
                    if (!bak0.Equals(Statuses0) || !bak1.Equals(Statuses1))
                        return 1;
                    return 0;
                }

                int Statuses_Curative_Magic_Action(Kernel_bin.Persistent_Statuses statuses0, Kernel_bin.Battle_Only_Statuses statuses1, Kernel_bin.Attack_Flags flags) => throw new NotImplementedException();

                int Statuses_Devour_Action(Kernel_bin.Persistent_Statuses statuses0, Kernel_bin.Battle_Only_Statuses statuses1, Kernel_bin.Attack_Flags flags) => throw new NotImplementedException();

                int Statuses_Everyones_Grudge_Action(Kernel_bin.Persistent_Statuses statuses0, Kernel_bin.Battle_Only_Statuses statuses1, Kernel_bin.Attack_Flags flags) => throw new NotImplementedException();

                int Statuses_Fixed_Magic_Statuses_Based_on_GF_Level_Action(Kernel_bin.Persistent_Statuses statuses0, Kernel_bin.Battle_Only_Statuses statuses1, Kernel_bin.Attack_Flags flags) => throw new NotImplementedException();

                int Statuses_Fixed_Statuses_Action(Kernel_bin.Persistent_Statuses statuses0, Kernel_bin.Battle_Only_Statuses statuses1, Kernel_bin.Attack_Flags flags) => throw new NotImplementedException();

                int Statuses_GF_Action(Kernel_bin.Persistent_Statuses statuses0, Kernel_bin.Battle_Only_Statuses statuses1, Kernel_bin.Attack_Flags flags) => throw new NotImplementedException();

                int Statuses_GF_Ignore_Target_SPR_Action(Kernel_bin.Persistent_Statuses statuses0, Kernel_bin.Battle_Only_Statuses statuses1, Kernel_bin.Attack_Flags flags) => throw new NotImplementedException();

                int Statuses_GF_Statuses_Action(Kernel_bin.Persistent_Statuses statuses0, Kernel_bin.Battle_Only_Statuses statuses1, Kernel_bin.Attack_Flags flags) => throw new NotImplementedException();

                int Statuses_Give_Percentage_HP_Action(Kernel_bin.Persistent_Statuses statuses0, Kernel_bin.Battle_Only_Statuses statuses1, Kernel_bin.Attack_Flags flags) =>
                           Statuses_Curative_Item_Action(statuses0, Statuses1, flags);

                int Statuses_Kamikaze_Action(Kernel_bin.Persistent_Statuses statuses0, Kernel_bin.Battle_Only_Statuses statuses1, Kernel_bin.Attack_Flags flags) => throw new NotImplementedException();

                int Statuses_LV_Attack_Action(Kernel_bin.Persistent_Statuses statuses0, Kernel_bin.Battle_Only_Statuses statuses1, Kernel_bin.Attack_Flags flags) => throw new NotImplementedException();

                int Statuses_LV_Down_Action(Kernel_bin.Persistent_Statuses statuses0, Kernel_bin.Battle_Only_Statuses statuses1, Kernel_bin.Attack_Flags flags) => throw new NotImplementedException();

                int Statuses_LV_Up_Action(Kernel_bin.Persistent_Statuses statuses0, Kernel_bin.Battle_Only_Statuses statuses1, Kernel_bin.Attack_Flags flags) => throw new NotImplementedException();

                int Statuses_Magic_Attack_Action(Kernel_bin.Persistent_Statuses statuses0, Kernel_bin.Battle_Only_Statuses statuses1, Kernel_bin.Attack_Flags flags) => throw new NotImplementedException();

                int Statuses_Magic_Attack_Ignore_Target_SPR_Action(Kernel_bin.Persistent_Statuses statuses0, Kernel_bin.Battle_Only_Statuses statuses1, Kernel_bin.Attack_Flags flags) => throw new NotImplementedException();

                int Statuses_Magic_Statuses_Action(Kernel_bin.Persistent_Statuses statuses0, Kernel_bin.Battle_Only_Statuses statuses1, Kernel_bin.Attack_Flags flags) => throw new NotImplementedException();

                int Statuses_Moogle_Dance_Action(Kernel_bin.Persistent_Statuses statuses0, Kernel_bin.Battle_Only_Statuses statuses1, Kernel_bin.Attack_Flags flags) => throw new NotImplementedException();

                int Statuses_Physical_Attack_Action(Kernel_bin.Persistent_Statuses statuses0, Kernel_bin.Battle_Only_Statuses statuses1, Kernel_bin.Attack_Flags flags) => throw new NotImplementedException();

                int Statuses_Physical_AttackIgnore_Target_VIT_Action(Kernel_bin.Persistent_Statuses statuses0, Kernel_bin.Battle_Only_Statuses statuses1, Kernel_bin.Attack_Flags flags) => throw new NotImplementedException();

                int Statuses_Physical_Statuses_Action(Kernel_bin.Persistent_Statuses statuses0, Kernel_bin.Battle_Only_Statuses statuses1, Kernel_bin.Attack_Flags flags) => throw new NotImplementedException();

                int Statuses_Renzokuken_Finisher_Action(Kernel_bin.Persistent_Statuses statuses0, Kernel_bin.Battle_Only_Statuses statuses1, Kernel_bin.Attack_Flags flags) => throw new NotImplementedException();

                int Statuses_Revive_Action(Kernel_bin.Persistent_Statuses statuses0, Kernel_bin.Battle_Only_Statuses statuses1, Kernel_bin.Attack_Flags flags)
                {
                    if ((Statuses0 & Kernel_bin.Persistent_Statuses.Death) != 0 || _CurrentHP == 0)
                    {
                        Statuses0 = Kernel_bin.Persistent_Statuses.None;
                        Statuses1 = Kernel_bin.Battle_Only_Statuses.None;
                        _CurrentHP = 0;
                        return 1;
                    }
                    return 0;
                }

                int Statuses_Revive_At_Full_HP_Action(Kernel_bin.Persistent_Statuses statuses0, Kernel_bin.Battle_Only_Statuses statuses1, Kernel_bin.Attack_Flags flags) =>
                           Statuses_Revive_Action(statuses0, statuses1, flags);

                int Statuses_Scan_Action(Kernel_bin.Persistent_Statuses statuses0, Kernel_bin.Battle_Only_Statuses statuses1, Kernel_bin.Attack_Flags flags) => throw new NotImplementedException();

                int Statuses_Squall_Gunblade_Attack_Action(Kernel_bin.Persistent_Statuses statuses0, Kernel_bin.Battle_Only_Statuses statuses1, Kernel_bin.Attack_Flags flags) => throw new NotImplementedException();

                int Statuses_Summon_Item_Action(Kernel_bin.Persistent_Statuses statuses0, Kernel_bin.Battle_Only_Statuses statuses1, Kernel_bin.Attack_Flags flags) => throw new NotImplementedException();

                int Statuses_Target_Current_HP_1_Action(Kernel_bin.Persistent_Statuses statuses0, Kernel_bin.Battle_Only_Statuses statuses1, Kernel_bin.Attack_Flags flags) => throw new NotImplementedException();

                int Statuses_Unknown_1_Action(Kernel_bin.Persistent_Statuses statuses0, Kernel_bin.Battle_Only_Statuses statuses1, Kernel_bin.Attack_Flags flags) => throw new NotImplementedException();

                int Statuses_Unknown_2_Action(Kernel_bin.Persistent_Statuses statuses0, Kernel_bin.Battle_Only_Statuses statuses1, Kernel_bin.Attack_Flags flags) => throw new NotImplementedException();

                int Statuses_Unknown_3_Action(Kernel_bin.Persistent_Statuses statuses0, Kernel_bin.Battle_Only_Statuses statuses1, Kernel_bin.Attack_Flags flags) => throw new NotImplementedException();

                int Statuses_Unknown_4_Action(Kernel_bin.Persistent_Statuses statuses0, Kernel_bin.Battle_Only_Statuses statuses1, Kernel_bin.Attack_Flags flags) => throw new NotImplementedException();

                int Statuses_White_WindQuistis_Action(Kernel_bin.Persistent_Statuses statuses0, Kernel_bin.Battle_Only_Statuses statuses1, Kernel_bin.Attack_Flags flags) => throw new NotImplementedException();
            }
        }

        /// <summary>
        /// If status immune statuses aren't set and return none.
        /// </summary>
        public bool StatusImmune { get; protected set; } = false;

        public abstract byte STR { get; }

        public Saves.GFData SummonedGF { get; private set; }

        public abstract byte VIT { get; }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Per tick increment
        /// </summary>
        /// <param name="spd"></param>
        /// <param name="speedMod"></param>
        /// <returns></returns>
        /// <see cref="https://gamefaqs.gamespot.com/ps/197343-final-fantasy-viii/faqs/58936"/>
        public static int BarIncrement(int spd, SpeedMod speedMod = SpeedMod.Normal) => (spd + 30) * ((byte)speedMod / 2);

        public static T Load<T>(BinaryReader br, Enum @enum) where T : Damageable, new()
        {
            T r = new T();
            r.Init();
            r.ReadData(br, @enum);
            return r;
        }

        /// <see cref="https://gamefaqs.gamespot.com/ps/197343-final-fantasy-viii/faqs/58936"/>
        public static int TimeToFillBarGF(int spd) => 200 * (int)Memory.CurrentBattleSpeed / (3 * (spd + 30));

        /// <summary>
        /// Starting value
        /// </summary>
        /// <param name="spd"></param>
        /// <returns></returns>
        /// <see cref="https://gamefaqs.gamespot.com/ps/197343-final-fantasy-viii/faqs/58936"/>
        public virtual int ATBBarStart(int spd)
        {
            int i = ((spd / 4) + Memory.Random.Next(128) - 34) * (int)Memory.CurrentBattleSpeed * 40;
            if (i > 0 && i < ATBBarSize)
                return i;
            else if (i < 0) return 0;
            else return ATBBarSize;
        }

        public int ATBBarStart()
        {
            if (IsGameOver)
                return 0;
            return ATBBarStart(SPD);
        }

        public virtual bool ATBCharged()
        {
            if (GetBattleMode().Equals(BattleMode.ATB_Charging))
            {
                SetBattleMode(BattleMode.ATB_Charged);
                return true;
            }
            return false;
        }

        public virtual int BarIncrement() => BarIncrement(SPD, GetSpeedMod());

        public bool ChangeHP(int dmg)
        {
            if (dmg == 0 || (Statuses1 & Kernel_bin.Battle_Only_Statuses.Invincible) != 0)
                return false;
            int hp = _CurrentHP - dmg;
            ushort lasthp = _CurrentHP;
            if (hp <= 0)
            {
                Statuses0 |= (Kernel_bin.Persistent_Statuses.Death);
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

        public virtual bool ChargeGF()
        {
            if (GetBattleMode().Equals(BattleMode.YourTurn) && (this.GetType().Equals(typeof(Saves.CharacterData))))
            {
                SetBattleMode(BattleMode.ATB_Charged);
                return true;
            }
            return false;
        }

        public abstract Damageable Clone();

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
            Kernel_bin.Persistent_Statuses? statuses0,
            Kernel_bin.Battle_Only_Statuses? statuses1,
            Kernel_bin.Attack_Type type,
            Kernel_bin.Attack_Flags? flags)
        {
            if (StatusesActions.ContainsKey(type))
            {
                int total = StatusesActions[type](
                statuses0 ?? Kernel_bin.Persistent_Statuses.None,
                statuses1 ?? Kernel_bin.Battle_Only_Statuses.None,
                flags ?? Kernel_bin.Attack_Flags.None);
                return total != 0;
            }
            return false;
        }

        public abstract short ElementalResistance(Kernel_bin.Element @in);

        public virtual bool EndTurn(bool force = false)
        {
            if ( force ||
                GetBattleMode().Equals(BattleMode.YourTurn) ||
                GetBattleMode().Equals(BattleMode.GF_Charging)
               )
            {
                SetBattleMode(BattleMode.EndTurn); // trigger any end of turn clean up.
                SetBattleMode(BattleMode.ATB_Charging); //start charging next turn.
                Refresh();
                return true;
            }
            return false;
        }

        public virtual Enum GetBattleMode() => _battlemode ?? BattleMode.ATB_Charging;

        public bool GetCast<T>(out T cast) where T : Damageable
        {
            if (GetType() == typeof(T))
            {
                cast = (T)this;
                return true;
            }
            cast = null;
            return false;
        }

        public bool GetCharacterData(out Saves.CharacterData character)
        => GetCast(out character);

        public bool GetEnemy(out Enemy enemy)
        => GetCast(out enemy);

        public bool GetGFData(out Saves.GFData gf)
        => GetCast(out gf);

        public SpeedMod GetSpeedMod()
        {
            if ((Statuses1 & Kernel_bin.Battle_Only_Statuses.Haste) != 0)
                return SpeedMod.Haste;
            else if ((Statuses1 & Kernel_bin.Battle_Only_Statuses.Stop) != 0 || IsGameOver)
                return SpeedMod.Stop;
            else if ((Statuses1 & Kernel_bin.Battle_Only_Statuses.Slow) != 0)
                return SpeedMod.Slow;
            return SpeedMod.Normal;
        }

        public virtual bool GFDiedWhileCharging()
        {
            if (GetBattleMode().Equals(BattleMode.GF_Charging))
            {
                SetBattleMode(BattleMode.YourTurn);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Overload this: To get correct MaxHP value
        /// </summary>
        /// <returns></returns>
        public virtual ushort MaxHP() => ushort.MaxValue;

        public virtual float PercentFullHP() => (float)CurrentHP() / MaxHP();

        public virtual void Refresh() => ATBTimer.Refresh(this);

        public virtual void Reset()
        {
            ATBTimer.Reset();
            Statuses1 = 0;
        }

        public virtual ushort ReviveHP() => (ushort)(MaxHP() / 8);

        protected virtual bool SetBattleMode(Enum mode)
        {
            if (!(_battlemode?.Equals(mode) ?? false))
            {
                _battlemode = mode;
                BattleModeChangeEventHandler?.Invoke(this, mode);
                return true;
            }
            return false;
        }

        public void SetSummon(Saves.GFData gfdata)
        {
            SummonedGF = gfdata;
            if (SummonedGF != null)
            {
                SetBattleMode(BattleMode.GF_Charging);
                SummonedGF.EndTurn(true);
            }
        }

        /// <summary>
        /// Summon GF
        /// </summary>
        /// <param name="gf"></param>
        public void SetSummon(GFs gf)
        {
            Saves.GFData gfdata = null;
            if (Memory.State?.GFs?.TryGetValue(gf, out gfdata) ?? false)
            {
                // found a gf.
            }
            SetSummon(gfdata);
        }

        public virtual bool StartTurn()
        {
            if (
                GetBattleMode().Equals(BattleMode.ATB_Charged)
               )
            {
                SetBattleMode(BattleMode.YourTurn); //it's your turn.
                Refresh();
                return true;
            }
            return false;
        }

        public abstract sbyte StatusResistance(Kernel_bin.Battle_Only_Statuses s);

        public abstract sbyte StatusResistance(Kernel_bin.Persistent_Statuses s);

        public virtual bool Switch()
        {
            if (GetBattleMode().Equals(BattleMode.YourTurn))
            {
                SetBattleMode(BattleMode.ATB_Charged);
                return true;
            }
            return false;
        }

        public float TicksToFillBar(int start, int spd, SpeedMod speedMod = SpeedMod.Normal)
        {
            int top = (ATBBarSize - start);
            int bot = BarIncrement(spd, speedMod);
            if (bot == 0)
                return float.MinValue;
            return (top / bot);
        }

        public float TicksToFillBar() => TicksToFillBar(ATBBarStart(), SPD, GetSpeedMod());

        public float TimeToFillBar(int start, int spd, SpeedMod speedMod = SpeedMod.Normal)
        {
            float tickspersec = 60f;
            return TicksToFillBar(start, spd, speedMod) / tickspersec;
        }

        public float TimeToFillBar() => TimeToFillBar(ATBBarStart(), SPD, GetSpeedMod());

        public int TimeToFillBarGF() => TimeToFillBarGF(SPD);

        public abstract ushort TotalStat(Kernel_bin.Stat s);

        public virtual bool Update(bool force = false)
        {
            if (GetBattleMode().Equals(BattleMode.ATB_Charging) || force)
            {
                if(!Module_battle_debug.PauseATB)
                    ATBTimer.Update();
                if (ATBTimer.Done && ATBCharged())
                {
                    //Your turn is ready.
                }
                return true;
            }
            else if (GetBattleMode().Equals(BattleMode.GF_Charging))
            {
                SummonedGF.Update();
                if(SummonedGF.IsGameOver || (SummonedGF.ATBTimer.Done && EndTurn()))
                {
                    //Summon GF end turn.
                    //SetSummon(null);
                }
                return true;
            }
                    return false;
        }

        protected virtual void Init()
        {
            SetBattleMode(BattleMode.ATB_Charging);
            ATBTimer = new ATBTimer(this);
        }

        protected abstract void ReadData(BinaryReader br, Enum @enum);

        #endregion Methods
    }
}
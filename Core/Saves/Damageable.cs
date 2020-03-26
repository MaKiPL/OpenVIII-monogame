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
        private Dictionary<Kernel.AttackType, Func<int, Kernel.AttackFlags, int>> _damageActions;
        private Kernel.PersistentStatuses _statuses0;
        private Kernel.BattleOnlyStatuses _statuses1;
        private Dictionary<Kernel.AttackType, Func<Kernel.PersistentStatuses, Kernel.BattleOnlyStatuses, Kernel.AttackFlags, int>> _statusesActions;
        protected ATBTimer ATBTimer;

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

        public bool Charging() => SetBattleMode(BattleMode.ATB_Charging);

        public IReadOnlyDictionary<Kernel.AttackType, Func<int, Kernel.AttackFlags, int>> DamageActions
        {
            get
            {
                if (_damageActions == null)
                    _damageActions = new Dictionary<Kernel.AttackType, Func<int, Kernel.AttackFlags, int>>
            {   { Kernel.AttackType.PhysicalAttack, Damage_Physical_Attack_Action},
                { Kernel.AttackType.MagicAttack, Damage_Magic_Attack_Action},
                { Kernel.AttackType.CurativeMagic, Damage_Curative_Magic_Action },
                { Kernel.AttackType.CurativeItem, Damage_Curative_Item_Action },
                { Kernel.AttackType.Revive, Damage_Revive_Action },
                { Kernel.AttackType.ReviveAtFullHP, Damage_Revive_At_Full_HP_Action },
                { Kernel.AttackType.PhysicalDamage, Damage_Physical_Damage_Action },
                { Kernel.AttackType.MagicDamage, Damage_Magic_Damage_Action },
                { Kernel.AttackType.RenzokukenFinisher, Damage_Renzokuken_Finisher_Action },
                { Kernel.AttackType.SquallGunbladeAttack, Damage_Squall_Gunblade_Attack_Action },
                { Kernel.AttackType.GF, Damage_GF_Action },
                { Kernel.AttackType.Scan, Damage_Scan_Action },
                { Kernel.AttackType.LvDown, Damage_LV_Down_Action },
                { Kernel.AttackType.SummonItem, Damage_Summon_Item_Action },
                { Kernel.AttackType.GFIgnoreTargetSPR, Damage_GF_Ignore_Target_SPR_Action },
                { Kernel.AttackType.LvUp, Damage_LV_Up_Action },
                { Kernel.AttackType.Card, Damage_Card_Action },
                { Kernel.AttackType.Kamikaze, Damage_Kamikaze_Action },
                { Kernel.AttackType.Devour, Damage_Devour_Action },
                { Kernel.AttackType.GFDamage, Damage_GF_Damage_Action },
                { Kernel.AttackType.Unknown1, Damage_Unknown_1_Action },
                { Kernel.AttackType.MagicAttackIgnoreTargetSPR, Damage_Magic_Attack_Ignore_Target_SPR_Action },
                { Kernel.AttackType.AngeloSearch, Damage_Angelo_Search_Action },
                { Kernel.AttackType.MoogleDance, Damage_Moogle_Dance_Action },
                { Kernel.AttackType.WhiteWindQuistis, Damage_White_WindQuistis_Action },
                { Kernel.AttackType.LvAttack, Damage_LV_Attack_Action },
                { Kernel.AttackType.FixedDamage, Damage_Fixed_Damage_Action },
                { Kernel.AttackType.TargetCurrentHP1, Damage_Target_Current_HP_1_Action },
                { Kernel.AttackType.FixedMagicDamageBasedOnGFLevel, Damage_Fixed_Magic_Damage_Based_on_GF_Level_Action },
                { Kernel.AttackType.Unknown2, Damage_Unknown_2_Action },
                { Kernel.AttackType.Unknown3, Damage_Unknown_3_Action },
                { Kernel.AttackType.GivePercentageHP, Damage_Give_Percentage_HP_Action },
                { Kernel.AttackType.Unknown4, Damage_Unknown_4_Action },
                { Kernel.AttackType.EveryoneGrudge, Damage_Everyones_Grudge_Action },
                { Kernel.AttackType._1_HP_Damage, Damage__1_HP_Damage_Action },
                { Kernel.AttackType.PhysicalAttackIgnoreTargetVIT, Damage_Physical_AttackIgnore_Target_VIT_Action },
            };

                return _damageActions;
                int Damage__1_HP_Damage_Action(int dmg, Kernel.AttackFlags flags) => throw new NotImplementedException();

                int Damage_Angelo_Search_Action(int dmg, Kernel.AttackFlags flags) => throw new NotImplementedException();

                int Damage_Card_Action(int dmg, Kernel.AttackFlags flags) => throw new NotImplementedException();

                int Damage_Curative_Item_Action(int dmg, Kernel.AttackFlags flags)
                {
                    //ChangeHP(-dmg);
                    var noheal = (Statuses0 & (Kernel.PersistentStatuses.Death | Kernel.PersistentStatuses.Petrify)) != 0 || (Statuses1 & (Kernel.BattleOnlyStatuses.SummonGF)) != 0 || _CurrentHP == 0;
                    var healisdmg = (Statuses0 & (Kernel.PersistentStatuses.Zombie)) != 0;
                    if (!noheal)
                    {
                        dmg = (healisdmg ? dmg : -dmg);
                    }
                    return ChangeHP(dmg) ? dmg : 0;
                }

                int Damage_Curative_Magic_Action(int dmg, Kernel.AttackFlags flags) => throw new NotImplementedException();

                int Damage_Devour_Action(int dmg, Kernel.AttackFlags flags) => throw new NotImplementedException();

                int Damage_Everyones_Grudge_Action(int dmg, Kernel.AttackFlags flags) => throw new NotImplementedException();

                int Damage_Fixed_Damage_Action(int dmg, Kernel.AttackFlags flags) => throw new NotImplementedException();

                int Damage_Fixed_Magic_Damage_Based_on_GF_Level_Action(int dmg, Kernel.AttackFlags flags) => throw new NotImplementedException();

                int Damage_GF_Action(int dmg, Kernel.AttackFlags flags) => throw new NotImplementedException();

                int Damage_GF_Damage_Action(int dmg, Kernel.AttackFlags flags) => throw new NotImplementedException();

                int Damage_GF_Ignore_Target_SPR_Action(int dmg, Kernel.AttackFlags flags) => throw new NotImplementedException();

                int Damage_Give_Percentage_HP_Action(int dmg, Kernel.AttackFlags flags) => Damage_Curative_Item_Action(MaxHP() * dmg / 100, flags);

                int Damage_Kamikaze_Action(int dmg, Kernel.AttackFlags flags) => throw new NotImplementedException();

                int Damage_LV_Attack_Action(int dmg, Kernel.AttackFlags flags) => throw new NotImplementedException();

                int Damage_LV_Down_Action(int dmg, Kernel.AttackFlags flags) => throw new NotImplementedException();

                int Damage_LV_Up_Action(int dmg, Kernel.AttackFlags flags) => throw new NotImplementedException();

                int Damage_Magic_Attack_Action(int dmg, Kernel.AttackFlags flags) => throw new NotImplementedException();

                int Damage_Magic_Attack_Ignore_Target_SPR_Action(int dmg, Kernel.AttackFlags flags) => throw new NotImplementedException();

                int Damage_Magic_Damage_Action(int dmg, Kernel.AttackFlags flags) => throw new NotImplementedException();

                int Damage_Moogle_Dance_Action(int dmg, Kernel.AttackFlags flags) => throw new NotImplementedException();

                int Damage_Physical_Attack_Action(int dmg, Kernel.AttackFlags flags) => throw new NotImplementedException();

                int Damage_Physical_AttackIgnore_Target_VIT_Action(int dmg, Kernel.AttackFlags flags) => throw new NotImplementedException();

                int Damage_Physical_Damage_Action(int dmg, Kernel.AttackFlags flags) => throw new NotImplementedException();

                int Damage_Renzokuken_Finisher_Action(int dmg, Kernel.AttackFlags flags) => throw new NotImplementedException();

                int Damage_Revive_Action(int dmg, Kernel.AttackFlags flags)
                {
                    var r = ReviveHP();

                    if ((Statuses0 & Kernel.PersistentStatuses.Zombie) != 0)
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

                int Damage_Revive_At_Full_HP_Action(int dmg, Kernel.AttackFlags flags)
                {
                    var r = MaxHP();
                    if ((Statuses0 & Kernel.PersistentStatuses.Zombie) != 0)
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

                int Damage_Scan_Action(int dmg, Kernel.AttackFlags flags) => throw new NotImplementedException();

                int Damage_Squall_Gunblade_Attack_Action(int dmg, Kernel.AttackFlags flags) => throw new NotImplementedException();

                int Damage_Summon_Item_Action(int dmg, Kernel.AttackFlags flags) => throw new NotImplementedException();

                int Damage_Target_Current_HP_1_Action(int dmg, Kernel.AttackFlags flags) => throw new NotImplementedException();

                int Damage_Unknown_1_Action(int dmg, Kernel.AttackFlags flags) => throw new NotImplementedException();

                int Damage_Unknown_2_Action(int dmg, Kernel.AttackFlags flags) => throw new NotImplementedException();

                int Damage_Unknown_3_Action(int dmg, Kernel.AttackFlags flags) => throw new NotImplementedException();

                int Damage_Unknown_4_Action(int dmg, Kernel.AttackFlags flags) => throw new NotImplementedException();

                int Damage_White_WindQuistis_Action(int dmg, Kernel.AttackFlags flags) => throw new NotImplementedException();
            }
        }

        public abstract byte EVA { get; }

        public abstract int EXP { get; }

        public abstract byte HIT { get; }

        public ushort HP => CurrentHP();

        public bool IsDead => CurrentHP() == 0 || (Statuses0 & Kernel.PersistentStatuses.Death) != 0;

        /// <summary>
        /// If all partymemembers are in gameover trigger Phoenix Pinion if CanPhoenixPinion or
        /// trigger Game over
        /// </summary>
        public bool IsGameOver => IsDead || IsPetrify || (Statuses1 & (Kernel.BattleOnlyStatuses.Eject)) != 0;

        /// <summary>
        /// ATB frozen
        /// </summary>
        public bool IsInactive => IsGameOver ||
            (Statuses1 & (Kernel.BattleOnlyStatuses.Stop)) != 0;

        /// <summary>
        /// Menu disabled
        /// </summary>
        public bool IsNonInteractive => IsInactive ||
            (Statuses0 & Kernel.PersistentStatuses.Berserk) != 0;

        public bool IsPetrify => (Statuses0 & (Kernel.PersistentStatuses.Petrify)) != 0;

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
        public virtual Kernel.PersistentStatuses Statuses0
        {
            get
            {
                if (!StatusImmune)
                    return _statuses0;
                else
                    return Kernel.PersistentStatuses.None;
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
        public virtual Kernel.BattleOnlyStatuses Statuses1
        {
            get
            {
                if (!StatusImmune)
                    return _statuses1;
                else
                    return Kernel.BattleOnlyStatuses.None;
            }
            set
            {
                if (!StatusImmune)
                    _statuses1 = value;
            }
        }

        public IReadOnlyDictionary<Kernel.AttackType, Func<Kernel.PersistentStatuses, Kernel.BattleOnlyStatuses, Kernel.AttackFlags, int>> StatusesActions
        {
            get
            {
                if (_statusesActions == null)
                    _statusesActions = new Dictionary<Kernel.AttackType, Func<Kernel.PersistentStatuses, Kernel.BattleOnlyStatuses, Kernel.AttackFlags, int>>
            {
                { Kernel.AttackType.PhysicalAttack, Statuses_Physical_Attack_Action },
                { Kernel.AttackType.MagicAttack, Statuses_Magic_Attack_Action },
                { Kernel.AttackType.CurativeMagic, Statuses_Curative_Magic_Action },
                { Kernel.AttackType.CurativeItem, Statuses_Curative_Item_Action },
                { Kernel.AttackType.Revive, Statuses_Revive_Action },
                { Kernel.AttackType.ReviveAtFullHP, Statuses_Revive_At_Full_HP_Action },
                { Kernel.AttackType.PhysicalDamage, Statuses_Physical_Statuses_Action },
                { Kernel.AttackType.MagicDamage, Statuses_Magic_Statuses_Action },
                { Kernel.AttackType.RenzokukenFinisher, Statuses_Renzokuken_Finisher_Action },
                { Kernel.AttackType.SquallGunbladeAttack, Statuses_Squall_Gunblade_Attack_Action },
                { Kernel.AttackType.GF, Statuses_GF_Action },
                { Kernel.AttackType.Scan, Statuses_Scan_Action },
                { Kernel.AttackType.LvDown, Statuses_LV_Down_Action },
                { Kernel.AttackType.SummonItem, Statuses_Summon_Item_Action },
                { Kernel.AttackType.GFIgnoreTargetSPR, Statuses_GF_Ignore_Target_SPR_Action },
                { Kernel.AttackType.LvUp, Statuses_LV_Up_Action },
                { Kernel.AttackType.Card, Statuses_Card_Action },
                { Kernel.AttackType.Kamikaze, Statuses_Kamikaze_Action },
                { Kernel.AttackType.Devour, Statuses_Devour_Action },
                { Kernel.AttackType.GFDamage, Statuses_GF_Statuses_Action },
                { Kernel.AttackType.Unknown1, Statuses_Unknown_1_Action },
                { Kernel.AttackType.MagicAttackIgnoreTargetSPR, Statuses_Magic_Attack_Ignore_Target_SPR_Action },
                { Kernel.AttackType.AngeloSearch, Statuses_Angelo_Search_Action },
                { Kernel.AttackType.MoogleDance, Statuses_Moogle_Dance_Action },
                { Kernel.AttackType.WhiteWindQuistis, Statuses_White_WindQuistis_Action },
                { Kernel.AttackType.LvAttack, Statuses_LV_Attack_Action },
                { Kernel.AttackType.FixedDamage, Statuses_Fixed_Statuses_Action },
                { Kernel.AttackType.TargetCurrentHP1, Statuses_Target_Current_HP_1_Action },
                { Kernel.AttackType.FixedMagicDamageBasedOnGFLevel, Statuses_Fixed_Magic_Statuses_Based_on_GF_Level_Action },
                { Kernel.AttackType.Unknown2, Statuses_Unknown_2_Action },
                { Kernel.AttackType.Unknown3, Statuses_Unknown_3_Action },
                { Kernel.AttackType.GivePercentageHP, Statuses_Give_Percentage_HP_Action },
                { Kernel.AttackType.Unknown4, Statuses_Unknown_4_Action },
                { Kernel.AttackType.EveryoneGrudge, Statuses_Everyones_Grudge_Action },
                { Kernel.AttackType._1_HP_Damage, Statuses__1_HP_Statuses_Action },
                { Kernel.AttackType.PhysicalAttackIgnoreTargetVIT, Statuses_Physical_AttackIgnore_Target_VIT_Action },
            };
                return _statusesActions;

                int Statuses__1_HP_Statuses_Action(Kernel.PersistentStatuses statuses0, Kernel.BattleOnlyStatuses statuses1, Kernel.AttackFlags flags) => throw new NotImplementedException();

                int Statuses_Angelo_Search_Action(Kernel.PersistentStatuses statuses0, Kernel.BattleOnlyStatuses statuses1, Kernel.AttackFlags flags) => throw new NotImplementedException();

                int Statuses_Card_Action(Kernel.PersistentStatuses statuses0, Kernel.BattleOnlyStatuses statuses1, Kernel.AttackFlags flags) => throw new NotImplementedException();

                int Statuses_Curative_Item_Action(Kernel.PersistentStatuses statuses0, Kernel.BattleOnlyStatuses statuses1, Kernel.AttackFlags flags)
                {
                    var bak0 = Statuses0;
                    var bak1 = Statuses1;
                    Statuses0 &= ~statuses0;
                    Statuses1 &= ~statuses1;
                    if (!bak0.Equals(Statuses0) || !bak1.Equals(Statuses1))
                        return 1;
                    return 0;
                }

                int Statuses_Curative_Magic_Action(Kernel.PersistentStatuses statuses0, Kernel.BattleOnlyStatuses statuses1, Kernel.AttackFlags flags) => throw new NotImplementedException();

                int Statuses_Devour_Action(Kernel.PersistentStatuses statuses0, Kernel.BattleOnlyStatuses statuses1, Kernel.AttackFlags flags) => throw new NotImplementedException();

                int Statuses_Everyones_Grudge_Action(Kernel.PersistentStatuses statuses0, Kernel.BattleOnlyStatuses statuses1, Kernel.AttackFlags flags) => throw new NotImplementedException();

                int Statuses_Fixed_Magic_Statuses_Based_on_GF_Level_Action(Kernel.PersistentStatuses statuses0, Kernel.BattleOnlyStatuses statuses1, Kernel.AttackFlags flags) => throw new NotImplementedException();

                int Statuses_Fixed_Statuses_Action(Kernel.PersistentStatuses statuses0, Kernel.BattleOnlyStatuses statuses1, Kernel.AttackFlags flags) => throw new NotImplementedException();

                int Statuses_GF_Action(Kernel.PersistentStatuses statuses0, Kernel.BattleOnlyStatuses statuses1, Kernel.AttackFlags flags) => throw new NotImplementedException();

                int Statuses_GF_Ignore_Target_SPR_Action(Kernel.PersistentStatuses statuses0, Kernel.BattleOnlyStatuses statuses1, Kernel.AttackFlags flags) => throw new NotImplementedException();

                int Statuses_GF_Statuses_Action(Kernel.PersistentStatuses statuses0, Kernel.BattleOnlyStatuses statuses1, Kernel.AttackFlags flags) => throw new NotImplementedException();

                int Statuses_Give_Percentage_HP_Action(Kernel.PersistentStatuses statuses0, Kernel.BattleOnlyStatuses statuses1, Kernel.AttackFlags flags) =>
                           Statuses_Curative_Item_Action(statuses0, Statuses1, flags);

                int Statuses_Kamikaze_Action(Kernel.PersistentStatuses statuses0, Kernel.BattleOnlyStatuses statuses1, Kernel.AttackFlags flags) => throw new NotImplementedException();

                int Statuses_LV_Attack_Action(Kernel.PersistentStatuses statuses0, Kernel.BattleOnlyStatuses statuses1, Kernel.AttackFlags flags) => throw new NotImplementedException();

                int Statuses_LV_Down_Action(Kernel.PersistentStatuses statuses0, Kernel.BattleOnlyStatuses statuses1, Kernel.AttackFlags flags) => throw new NotImplementedException();

                int Statuses_LV_Up_Action(Kernel.PersistentStatuses statuses0, Kernel.BattleOnlyStatuses statuses1, Kernel.AttackFlags flags) => throw new NotImplementedException();

                int Statuses_Magic_Attack_Action(Kernel.PersistentStatuses statuses0, Kernel.BattleOnlyStatuses statuses1, Kernel.AttackFlags flags) => throw new NotImplementedException();

                int Statuses_Magic_Attack_Ignore_Target_SPR_Action(Kernel.PersistentStatuses statuses0, Kernel.BattleOnlyStatuses statuses1, Kernel.AttackFlags flags) => throw new NotImplementedException();

                int Statuses_Magic_Statuses_Action(Kernel.PersistentStatuses statuses0, Kernel.BattleOnlyStatuses statuses1, Kernel.AttackFlags flags) => throw new NotImplementedException();

                int Statuses_Moogle_Dance_Action(Kernel.PersistentStatuses statuses0, Kernel.BattleOnlyStatuses statuses1, Kernel.AttackFlags flags) => throw new NotImplementedException();

                int Statuses_Physical_Attack_Action(Kernel.PersistentStatuses statuses0, Kernel.BattleOnlyStatuses statuses1, Kernel.AttackFlags flags) => throw new NotImplementedException();

                int Statuses_Physical_AttackIgnore_Target_VIT_Action(Kernel.PersistentStatuses statuses0, Kernel.BattleOnlyStatuses statuses1, Kernel.AttackFlags flags) => throw new NotImplementedException();

                int Statuses_Physical_Statuses_Action(Kernel.PersistentStatuses statuses0, Kernel.BattleOnlyStatuses statuses1, Kernel.AttackFlags flags) => throw new NotImplementedException();

                int Statuses_Renzokuken_Finisher_Action(Kernel.PersistentStatuses statuses0, Kernel.BattleOnlyStatuses statuses1, Kernel.AttackFlags flags) => throw new NotImplementedException();

                int Statuses_Revive_Action(Kernel.PersistentStatuses statuses0, Kernel.BattleOnlyStatuses statuses1, Kernel.AttackFlags flags)
                {
                    if ((Statuses0 & Kernel.PersistentStatuses.Death) != 0 || _CurrentHP == 0)
                    {
                        Statuses0 = Kernel.PersistentStatuses.None;
                        Statuses1 = Kernel.BattleOnlyStatuses.None;
                        _CurrentHP = 0;
                        return 1;
                    }
                    return 0;
                }

                int Statuses_Revive_At_Full_HP_Action(Kernel.PersistentStatuses statuses0, Kernel.BattleOnlyStatuses statuses1, Kernel.AttackFlags flags) =>
                           Statuses_Revive_Action(statuses0, statuses1, flags);

                int Statuses_Scan_Action(Kernel.PersistentStatuses statuses0, Kernel.BattleOnlyStatuses statuses1, Kernel.AttackFlags flags) => throw new NotImplementedException();

                int Statuses_Squall_Gunblade_Attack_Action(Kernel.PersistentStatuses statuses0, Kernel.BattleOnlyStatuses statuses1, Kernel.AttackFlags flags) => throw new NotImplementedException();

                int Statuses_Summon_Item_Action(Kernel.PersistentStatuses statuses0, Kernel.BattleOnlyStatuses statuses1, Kernel.AttackFlags flags) => throw new NotImplementedException();

                int Statuses_Target_Current_HP_1_Action(Kernel.PersistentStatuses statuses0, Kernel.BattleOnlyStatuses statuses1, Kernel.AttackFlags flags) => throw new NotImplementedException();

                int Statuses_Unknown_1_Action(Kernel.PersistentStatuses statuses0, Kernel.BattleOnlyStatuses statuses1, Kernel.AttackFlags flags) => throw new NotImplementedException();

                int Statuses_Unknown_2_Action(Kernel.PersistentStatuses statuses0, Kernel.BattleOnlyStatuses statuses1, Kernel.AttackFlags flags) => throw new NotImplementedException();

                int Statuses_Unknown_3_Action(Kernel.PersistentStatuses statuses0, Kernel.BattleOnlyStatuses statuses1, Kernel.AttackFlags flags) => throw new NotImplementedException();

                int Statuses_Unknown_4_Action(Kernel.PersistentStatuses statuses0, Kernel.BattleOnlyStatuses statuses1, Kernel.AttackFlags flags) => throw new NotImplementedException();

                int Statuses_White_WindQuistis_Action(Kernel.PersistentStatuses statuses0, Kernel.BattleOnlyStatuses statuses1, Kernel.AttackFlags flags) => throw new NotImplementedException();
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

        public static T Load<T>(BinaryReader br, Enum @enum, Saves.Data data) where T : Damageable, new()
        {
            var r = new T { Data = data };
            r.ReadData(br, @enum);
            r.Init();
            return r;
        }

        protected Saves.Data Data { get; set; }

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
            var i = 0;
            //this verison loops till it gets a value between 0 and ATBBarSize. Unsure which is best.
            //do
            //{
            //    i = ((spd / 4) + Memory.Random.Next(128) - 34) * (int)Memory.CurrentBattleSpeed * 40;
            //}
            //while (i <= 0 || i > ATBBarSize);
            //return i;
            //this version will return ATBBarSize if larger and 0 if less than 0.
            i = ((spd / 4) + Memory.Random.Next(128) - 34) * (int)Memory.CurrentBattleSpeed * 40;
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
            if (dmg == 0 || (Statuses1 & Kernel.BattleOnlyStatuses.Invincible) != 0)
                return false;
            var hp = _CurrentHP - dmg;
            var lasthp = _CurrentHP;
            if (hp <= 0)
            {
                Statuses0 |= (Kernel.PersistentStatuses.Death);
                _CurrentHP = 0;
            }
            else
            {
                _CurrentHP = (ushort)(hp > ushort.MaxValue ? ushort.MaxValue : hp);
                //use a CurrentHP method or property to adjust this to correct maxhp.
                //if team Laguna the max hp can be different and each gf has there own.
            }
            CurrentHP();
            if (lasthp == _CurrentHP) return false;
            Debug.WriteLine($"{this}: Dealt {dmg}, previous hp: {lasthp}, current hp: {_CurrentHP}");
            return true;
        }

        public virtual bool ChargeGF()
        {
            if (!GetBattleMode().Equals(BattleMode.YourTurn) ||
                (!(this is Saves.CharacterData))) return false;
            SetBattleMode(BattleMode.ATB_Charged);
            return true;
        }

        public abstract Damageable Clone();

        public virtual bool IsCritical => CurrentHP() <= CriticalHP();

        public virtual ushort CriticalHP() => (ushort)((MaxHP() / 4) - 1);

        /// <summary>
        /// Override This: to check vs your max hp value.
        /// </summary>
        /// <returns></returns>
        public virtual ushort CurrentHP()
        {
            var max = MaxHP();
            if (_CurrentHP > max) _CurrentHP = max;
            return _CurrentHP;
        }

        public bool DealDamage(int dmg, Kernel.AttackType type, Kernel.AttackFlags? flags)
        {
            if (!DamageActions.ContainsKey(type)) return false;
            //ChangeHP(-dmg);
            // check max and min values
            if (dmg > Kernel.KernelBin.MaxHPValue && type != Kernel.AttackType.GivePercentageHP) dmg = Kernel.KernelBin.MaxHPValue;
            if (dmg < 0) return false;
            // depending on damage type see if damage is correct
            dmg = DamageActions[type](dmg, flags ?? Kernel.AttackFlags.None);

            return dmg != 0;
        }

        public bool DealStatus(
            Kernel.PersistentStatuses? statuses0,
            Kernel.BattleOnlyStatuses? statuses1,
            Kernel.AttackType type,
            Kernel.AttackFlags? flags)
        {
            if (StatusesActions.ContainsKey(type))
            {
                var total = StatusesActions[type](
                statuses0 ?? Kernel.PersistentStatuses.None,
                statuses1 ?? Kernel.BattleOnlyStatuses.None,
                flags ?? Kernel.AttackFlags.None);
                return total != 0;
            }
            return false;
        }

        public abstract short ElementalResistance(Kernel.Element @in);

        public virtual bool EndTurn(bool force = false)
        {
            if (force ||
                GetBattleMode().Equals(BattleMode.YourTurn) ||
                GetBattleMode().Equals(BattleMode.GF_Charging)
               )
            {
                SetBattleMode(BattleMode.EndTurn); // trigger any end of turn clean up.
                SetBattleMode(BattleMode.ATB_Charging); //start charging next turn.
                ATBTimer.NewTurn();
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
            if ((Statuses1 & Kernel.BattleOnlyStatuses.Haste) != 0)
                return SpeedMod.Haste;
            else if ((Statuses1 & Kernel.BattleOnlyStatuses.Stop) != 0 || IsGameOver)
                return SpeedMod.Stop;
            else if ((Statuses1 & Kernel.BattleOnlyStatuses.Slow) != 0)
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

        public abstract sbyte StatusResistance(Kernel.BattleOnlyStatuses s);

        public abstract sbyte StatusResistance(Kernel.PersistentStatuses s);

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
            var top = (ATBBarSize - start);
            var bot = BarIncrement(spd, speedMod);
            if (bot == 0)
                return float.MinValue;
            return (top / bot);
        }

        public float TicksToFillBar() => TicksToFillBar(ATBBarStart(), SPD, GetSpeedMod());

        public float TimeToFillBar(int start, int spd, SpeedMod speedMod = SpeedMod.Normal)
        {
            var tickspersec = 60f;
            return TicksToFillBar(start, spd, speedMod) / tickspersec;
        }

        public float TimeToFillBar() => TimeToFillBar(ATBBarStart(), SPD, GetSpeedMod());

        public int TimeToFillBarGF() => TimeToFillBarGF(SPD);

        public abstract ushort TotalStat(Kernel.Stat s);

        public virtual bool Update(bool force = false)
        {
            if (GetBattleMode().Equals(BattleMode.ATB_Charging) || force)
            {
                if (!ModuleBattleDebug.PauseATB)
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
                if (SummonedGF.IsGameOver || (SummonedGF.ATBTimer.Done && EndTurn()))
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

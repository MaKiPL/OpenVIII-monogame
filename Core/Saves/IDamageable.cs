using System;
using System.Collections.Generic;

namespace OpenVIII
{
    public interface IStatusEffects
    {
        Kernel.Persistent_Statuses Statuses0 { get; set; }
        Kernel.BattleOnlyStatuses Statuses1 { get; set; }
        bool DealStatus(Kernel.Persistent_Statuses? statuses0, Kernel.BattleOnlyStatuses? statuses1, Kernel.AttackType type, Kernel.AttackFlags? flags);
        IReadOnlyDictionary<Kernel.AttackType, Func<Kernel.Persistent_Statuses, Kernel.BattleOnlyStatuses, Kernel.AttackFlags, int>> StatusesActions { get; }
        bool StatusImmune { get; }
        sbyte StatusResistance(Kernel.BattleOnlyStatuses s);
        sbyte StatusResistance(Kernel.Persistent_Statuses s);
    }
    public interface IElemental
    {
        short ElementalResistance(Kernel.Element @in);
    }
    public interface IDamageable : IStatusEffects, IStats,IElemental
    {
        IReadOnlyDictionary<Kernel.AttackType, Func<int, Kernel.AttackFlags, int>> DamageActions { get; }
        bool IsDead { get; }
        bool IsGameOver { get; }
        bool IsInactive { get; }
        bool IsNonInteractive { get; }
        bool IsPetrify { get; }
        FF8String Name { get; set; }

        bool ChangeHP(int dmg);
        bool IsCritical { get; }
        ushort CurrentHP();
        bool DealDamage(int dmg, Kernel.AttackType type, Kernel.AttackFlags? flags);
        ushort ReviveHP();
    }
    public interface IStats
    {
        ushort HP { get; }
        ushort TotalStat(Kernel.Stat s);
        byte SPD { get; }
        byte SPR { get; }
        byte STR { get; }
        byte VIT { get; }
        byte MAG { get; }
        byte HIT { get; }
        byte Level { get; }
        byte LUCK { get; }
        byte EVA { get; }
        int EXP { get; }
        ushort MaxHP();
        float PercentFullHP();
        ushort CriticalHP();
    }
}

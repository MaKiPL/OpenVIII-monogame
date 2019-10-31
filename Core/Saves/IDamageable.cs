using System;
using System.Collections.Generic;

namespace OpenVIII
{
    public interface IStatusEffects
    {
        Kernel_bin.Persistent_Statuses Statuses0 { get; set; }
        Kernel_bin.Battle_Only_Statuses Statuses1 { get; set; }
        bool DealStatus(Kernel_bin.Persistent_Statuses? statuses0, Kernel_bin.Battle_Only_Statuses? statuses1, Kernel_bin.Attack_Type type, Kernel_bin.Attack_Flags? flags);
        IReadOnlyDictionary<Kernel_bin.Attack_Type, Func<Kernel_bin.Persistent_Statuses, Kernel_bin.Battle_Only_Statuses, Kernel_bin.Attack_Flags, int>> StatusesActions { get; }
        bool StatusImmune { get; }
        sbyte StatusResistance(Kernel_bin.Battle_Only_Statuses s);
        sbyte StatusResistance(Kernel_bin.Persistent_Statuses s);
    }
    public interface IElemental
    {
        short ElementalResistance(Kernel_bin.Element @in);
    }
    public interface IDamageable : IStatusEffects, IStats,IElemental
    {
        IReadOnlyDictionary<Kernel_bin.Attack_Type, Func<int, Kernel_bin.Attack_Flags, int>> DamageActions { get; }
        bool IsDead { get; }
        bool IsGameOver { get; }
        bool IsInactive { get; }
        bool IsNonInteractive { get; }
        bool IsPetrify { get; }
        FF8String Name { get; set; }

        bool ChangeHP(int dmg);
        bool Critical();
        ushort CurrentHP();
        bool DealDamage(int dmg, Kernel_bin.Attack_Type type, Kernel_bin.Attack_Flags? flags);
        ushort ReviveHP();
    }
    public interface IStats
    {
        ushort HP { get; }
        ushort TotalStat(Kernel_bin.Stat s);
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
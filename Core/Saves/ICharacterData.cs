using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OpenVIII
{
    public interface IJunctionTo
    {
        void AutoATK();
        void AutoDEF();
        void AutoMAG();
        void RemoveAll();
        void RemoveMagic();
        void JunctionSpell(Kernel.Stat stat, byte spell);
    }
    public interface ICharacterData : IDamageable, IJunctionTo, IUnlockable
    {
        bool CanPhoenixPinion { get; }
        Kernel.CharacterStats CharacterStats { get; }
        Battle.CharacterInstanceInformation CII { get; }
        sbyte CurrentCrisisLevel { get; }
        uint Experience { get; set; }
        ushort ExperienceToNextLevel { get; }
        Characters ID { get; }
        bool SwitchLocked { get; }
        List<Kernel.Abilities> UnlockedGFAbilities { get; }

        void BattleStart(Battle.CharacterInstanceInformation cii);
        int CriticalHP(Characters value);
        ushort CurrentHP(Characters c);
        sbyte GenerateCrisisLevel();
        ushort MaxHP(Characters c);
        float PercentFullHP(Characters c);
        IOrderedEnumerable<Kernel.MagicData> SortedMagic(Kernel.Stat Stat);
        string ToString();
        bool Unlocked(List<Kernel.Abilities> unlocked, Kernel.Stat stat);
        bool Unlocked(Kernel.Stat stat);
        ushort TotalStat(Kernel.Stat s, Characters c = Characters.Blank);
    }

    public interface IUnlockable
    {
        bool Available { get; }
    }
}

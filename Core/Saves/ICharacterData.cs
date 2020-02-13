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
        void JunctionSpell(Kernel_bin.Stat stat, byte spell);
    }
    public interface ICharacterData : IDamageable, IJunctionTo, IUnlockable
    {
        bool CanPhoenixPinion { get; }
        Kernel_bin.Character_Stats CharacterStats { get; }
        Battle.CharacterInstanceInformation CII { get; }
        sbyte CurrentCrisisLevel { get; }
        uint Experience { get; set; }
        ushort ExperienceToNextLevel { get; }
        Characters ID { get; }
        bool SwitchLocked { get; }
        List<Kernel_bin.Abilities> UnlockedGFAbilities { get; }

        void BattleStart(Battle.CharacterInstanceInformation cii);
        int CriticalHP(Characters value);
        ushort CurrentHP(Characters c);
        sbyte GenerateCrisisLevel();
        ushort MaxHP(Characters c);
        float PercentFullHP(Characters c);
        IOrderedEnumerable<Kernel_bin.Magic_Data> SortedMagic(Kernel_bin.Stat Stat);
        string ToString();
        bool Unlocked(List<Kernel_bin.Abilities> unlocked, Kernel_bin.Stat stat);
        bool Unlocked(Kernel_bin.Stat stat);
        ushort TotalStat(Kernel_bin.Stat s, Characters c = Characters.Blank);
    }

    public interface IUnlockable
    {
        bool Available { get; }
    }
}
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using OpenVIII.Battle;
using OpenVIII.Kernel;

namespace OpenVIII
{
    public static partial class Saves
    {
        #region Enums

        /// <summary>
        /// </summary>
        /// <remarks>Hyne has switch locked 0 and 1</remarks>
        [Flags]
        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        public enum Exists : byte
        {
            Unavailable = 0x0,

            /// <summary>
            /// Shows in menu.
            /// </summary>
            Available = 0x1,

            /// <summary>
            /// Party members that cannot leave or be added
            /// </summary>
            SwitchLocked0 = 0x2,

            /// <summary>
            /// Party members that cannot leave or be added
            /// </summary>
            SwitchLocked1 = 0x4,

            /// <summary>
            /// Many have this set. I don't know what it does.
            /// </summary>
            Unk8 = 0x8,

            /// <summary>
            /// Many have this set. I don't know what it does.
            /// </summary>
            Unk10 = 0x10,

            /// <summary>
            /// Many have this set. I don't know what it does.
            /// </summary>
            Unk20 = 0x20,

            /// <summary>
            /// Many have this set. I don't know what it does.
            /// </summary>
            Unk30 = 0x40,

            /// <summary>
            /// Many have this set. I don't know what it does.
            /// </summary>
            Unk40 = 0x80
        }

        #endregion Enums

        #region Classes

        /// <summary>
        /// Data for each Character
        /// </summary>
        /// <see cref="http://wiki.ffrtt.ru/index.php/FF8/GameSaveFormat#Characters"/>
        public class CharacterData : Damageable, ICharacterData
        {
            #region Fields

            /// <summary>
            /// Raw HP buff from items.
            /// </summary>
            public ushort RawHP;

            /// <summary>
            /// Junctioned Abilities
            /// </summary>
            public List<Abilities> Abilities;

            /// <summary>
            /// <para>Alt Models/different costumes</para>
            /// <para>(Normal, SeeD, Soldier...)</para>
            /// </summary>
            public byte AlternativeModel;

            /// <summary>
            /// Junctioned Commands
            /// </summary>
            public List<Abilities> Commands;

            /// <summary>
            /// <para>Compatibility With GFs</para>
            /// <para>Effects Summon speed and such.</para>
            /// </summary>
            public Dictionary<GFs, CompatibilitywithGF> CompatibilityWithGFs;

            /// <summary>
            /// Value determines if a character shows in menu and can be added to party.
            /// <para>
            /// 15,9,7,4,1 shows on menu, 0 locked, 6 hidden // I think I wonder if this is a flags value.
            /// </para>
            /// </summary>
            public Exists Exists;

            public OrderedDictionary<byte, byte> Magics;

            /// <summary>
            /// Character Model
            /// </summary>
            public byte ModelID;

            /// <summary>
            /// Number of Kills
            /// </summary>
            public ushort NumberOfKills;

            /// <summary>
            /// Number of KOs
            /// </summary>
            public ushort NumberOfKOs;

            public byte PaddingOrUnusedCommand;

            /// <summary>
            /// Stats that can be increased via items. Except for HP because it's a ushort not a byte.
            /// </summary>
            public Dictionary<Stat, byte> RawStats;

            /// <summary>
            /// Junctioned Magic per stat.
            /// </summary>
            public Dictionary<Stat, byte> StatJ;

            public byte Unknown1;

            public byte Unknown2;

            public byte Unknown3;

            public byte Unknown4;

            /// <summary>
            /// Weapon
            /// </summary>
            public byte WeaponID;

            /// <summary>
            /// Total amount of spells the will be loaded/saved.
            /// </summary>
            private const int MagicCapacity = 32;

            /// <summary>
            /// Total Exp
            /// </summary>
            private uint _experience;

            private Characters _id;

            /// <summary>
            /// Junctioned GFs - raw value
            /// </summary>
            private GFflags _rawJunctionedGFs;

            #endregion Fields

            #region Properties

            /// <summary>
            /// Visible
            /// </summary>
            public bool Available => (Exists & Exists.Available) != 0;

            /// <summary>
            /// 25.4% chance to cast automatically on game over, if used once in battle
            /// </summary>
            /// <remarks>
            /// Memory.State.FieldVars. has a value that tracks if PhoenixPinion is used just need to
            /// find it
            /// </remarks>
            public bool CanPhoenixPinion => IsDead && !(IsPetrify || (Statuses1 & (BattleOnlyStatuses.Eject)) != 0) && Memory.State.Items.Any(m => m.ID == 31 && m.QTY >= 1);

            /// <summary>
            /// Kernel Stats
            /// </summary>
            public CharacterStats CharacterStats
            {
                get
                {
                    if (Memory.Kernel_Bin?.CharacterStats != null && Memory.Kernel_Bin.CharacterStats.TryGetValue(_id, out CharacterStats value))
                        return value;
                    return null;
                }
            }

            //public CharacterData(BinaryReader br, Characters c) => Read(br, c);
            public CharacterInstanceInformation CII { get; private set; }

            /// <summary>
            /// Set by GenerateCrisisLevel(), -1 means no limit break. &gt;=0 has a limit break.
            /// </summary>
            /// <returns>-1 - 4</returns>
            /// <remarks>https://finalfantasy.fandom.com/wiki/Crisis_Level</remarks>
            public sbyte CurrentCrisisLevel { get; private set; }

            public override byte EVA => checked((byte)TotalStat(Stat.EVA));

            public override int EXP => checked((int)Experience);

            public uint Experience
            {
                get => _experience; set
                {
                    if (_experience == 0)
                        _experience = value;
                    else if (!IsGameOver && _experience != value)
                        _experience = value; //trying to give my self a good break point
                }
            }

            public ushort ExperienceToNextLevel => (ushort)(Level == 100 ? 0 : MathHelper.Clamp(CharacterStats.Exp((byte)(Level + 1)) - Experience, 0, CharacterStats.Exp(2)));

            public override byte HIT => checked((byte)TotalStat(Stat.HIT));

            /// <summary>
            /// If TeamLaguna the BattleID will change to a Laguna Party member
            /// </summary>
            public Characters ID
            {
                get
                {
                    int ind;
                    if (Memory.State != null && Memory.State.TeamLaguna && (ind = Memory.State.PartyData.FindIndex(x => x.Equals(_id))) >= 0 && !Memory.State.Party.Contains(_id))
                        return Memory.State.Party[ind];
                    return _id;
                }
            }

            public override bool IsCritical => CurrentHP() <= CriticalHP();

            /// <summary>
            /// Junctioned GFs
            /// </summary>
            public IEnumerable<GFs> JunctionedGFs => Enum.GetValues(_rawJunctionedGFs.GetType()).Cast<GFflags>().Where(x => _rawJunctionedGFs.HasFlag(x) && ConvertGFEnum.ContainsKey(x)).Distinct().Select(x => ConvertGFEnum[x]);

            public override byte Level => CharacterStats?.Level(Experience) ?? 0;

            public override byte LUCK => checked((byte)TotalStat(Stat.Luck));

            public override byte MAG => checked((byte)TotalStat(Stat.MAG));

            /// <summary>
            /// If TeamLaguna the name will change to a Laguna Party member
            /// </summary>
            public override FF8String Name
            {
                get
                {
                    if (Memory.State != null && Memory.State.TeamLaguna)
                    {
                        return Memory.Strings.GetName(ID);
                    }
                    return base.Name;
                }
                set => base.Name = value;
            }

            public override byte SPD => checked((byte)TotalStat(Stat.SPD));

            public override byte SPR => checked((byte)TotalStat(Stat.SPR));

            public override byte STR => checked((byte)TotalStat(Stat.STR));

            /// <summary>
            /// Cannot remove from party or add to party.
            /// </summary>
            public bool SwitchLocked => (Exists & (Exists.SwitchLocked0 | Exists.SwitchLocked1)) != 0;

            public List<Abilities> UnlockedGFAbilities
            {
                get
                {
                    BitArray total = new BitArray(16 * 8);
                    List<Abilities> abilities = new List<Abilities>();
                    foreach (GFs gf in JunctionedGFs)
                    {
                        total.Or(Memory.State.GFs[gf].Complete);
                    }
                    for (int i = 1; i < total.Length; i++)//0 is none so skipping it.
                    {
                        if (total[i])
                            abilities.Add((Abilities)i);
                    }

                    return abilities;
                }
            }

            public override byte VIT => checked((byte)TotalStat(Stat.VIT));

            #endregion Properties

            #region Methods

            public static CharacterData Load(BinaryReader br, Characters @enum, Data data) => Load<CharacterData>(br, @enum, data);

            public void AutoATK() => Auto(KernelBin.AutoAtk);

            public void AutoDEF() => Auto(KernelBin.AutoDef);

            public void AutoMAG() => Auto(KernelBin.AutoMAG);

            public void BattleStart(CharacterInstanceInformation cii)
            {
                CII = cii;
                Statuses1 = BattleOnlyStatuses.None;
                if (Abilities.Contains(Kernel.Abilities.AutoHaste))
                    Statuses1 |= BattleOnlyStatuses.Haste;
                if (Abilities.Contains(Kernel.Abilities.AutoProtect))
                    Statuses1 |= BattleOnlyStatuses.Protect;
                if (Abilities.Contains(Kernel.Abilities.AutoReflect))
                    Statuses1 |= BattleOnlyStatuses.Reflect;
                if (Abilities.Contains(Kernel.Abilities.AutoShell))
                    Statuses1 |= BattleOnlyStatuses.Shell;
                //reset the ATB timer.
                ATBTimer.FirstTurn();
            }

            public override Damageable Clone()
            {
                //Shadow copy
                CharacterData c = (CharacterData)MemberwiseClone();
                //Deep copy
                c.Name = Name?.Clone();
                c.CompatibilityWithGFs = CompatibilityWithGFs?.ToDictionary(e => e.Key, e => e.Value);
                c.StatJ = StatJ?.ToDictionary(e => e.Key, e => e.Value);
                c.Magics = new OrderedDictionary<byte, byte>(Magics?.Count ?? 0);
                if (Magics != null)
                    foreach (KeyValuePair<byte, byte> magic in Magics)
                        c.Magics.Add(magic.Key, magic.Value);
                c.RawStats = RawStats?.ToDictionary(e => e.Key, e => e.Value);
                c.Commands = Commands?.ConvertAll(item => item);
                c.Abilities = Abilities?.ConvertAll(item => item);
                return c;
            }

            public OrderedDictionary<byte, byte> CloneMagic() => Magics.Clone();

            public Dictionary<Stat, byte> CloneMagicJunction() => new Dictionary<Stat, byte>(StatJ);

            public int CriticalHP(Characters value) => MaxHP(value) / 4 - 1;

            public override ushort CurrentHP() => CurrentHP(ID);

            public ushort CurrentHP(Characters c)
            {
                ushort max = MaxHP(c);
                if (max < _CurrentHP) _CurrentHP = max;
                return _CurrentHP;
            }

            public override short ElementalResistance(Element @in) => throw new NotImplementedException();

            /// <summary>
            /// <para>
            /// This Generates a Crisis Level. Run this each turn in battle. Though in real game it
            /// runs when the menu pops up.
            /// </para>
            /// <para>-1 means no limit break. &gt;=0 has a limit break.</para>
            /// </summary>
            /// <returns>-1 - 4</returns>
            /// <remarks>https://finalfantasy.fandom.com/wiki/Crisis_Level</remarks>
            /// <remarks>TODO: Need to confirm the formula is correct via reverse</remarks>
            public sbyte GenerateCrisisLevel()
            {
                ushort current = CurrentHP();
                ushort max = MaxHP();
                //if ((BattleID == Characters.Seifer_Almasy && CurrentHP() < (max * 84 / 100)))
                //{
                int hpMod = CharacterStats.Crisis * 10 * current / max;
                int deathBonus = Memory.State.DeadPartyMembers() * 200 + 1600;
                int statusBonus = (int)(Statuses0.Count() * 10); // I think this is status of all party members
                int randomMod = Memory.Random.Next(byte.MaxValue + 1) + 160;
                int crisisLevel = (statusBonus + deathBonus - hpMod) / randomMod; // better random number?
                switch (crisisLevel)
                {
                    case 5:
                        CurrentCrisisLevel = 0;
                        return CurrentCrisisLevel;

                    case 6:
                        CurrentCrisisLevel = 1;
                        return CurrentCrisisLevel;

                    case 7:
                        CurrentCrisisLevel = 2;
                        return CurrentCrisisLevel;

                    default:
                        {
                            if (crisisLevel < 8) return CurrentCrisisLevel = -1;
                            CurrentCrisisLevel = 3;
                            return CurrentCrisisLevel;
                        }
                }
            }

            // ReSharper disable once UnusedMember.Global
            public void JunctionGF(GFs gf) =>
                _rawJunctionedGFs |= ConvertGFEnum.FirstOrDefault(x => x.Value == gf).Key;

            public void JunctionSpell(Stat stat, byte spell)
            {
                //see if magic is in use, if so remove it
                if (StatJ.ContainsValue(spell))
                {
                    Stat key = StatJ.FirstOrDefault(x => x.Value == spell).Key;
                    StatJ[key] = 0;
                }
                //junction magic
                StatJ[stat] = spell;
            }

            /// <summary>
            /// Max HP
            /// </summary>
            /// <param name="c">Force another character's HP calculation</param>
            /// <returns></returns>
            public ushort MaxHP(Characters c) => TotalStat(Stat.HP, c);

            public override ushort MaxHP() => MaxHP(ID);

            public override float PercentFullHP() => PercentFullHP(ID);

            public float PercentFullHP(Characters c) => (float)CurrentHP(c) / MaxHP(c);

            public void RemoveAll()
            {
                StatJ = StatJ.ToDictionary(e => e.Key, e => (byte)0);
                Commands = Commands.ConvertAll(item => Kernel.Abilities.None);
                Abilities = Abilities.ConvertAll(item => Kernel.Abilities.None);
                _rawJunctionedGFs = GFflags.None;
            }

            public void RemoveJunctionedGF(GFs gf) =>
                _rawJunctionedGFs ^= ConvertGFEnum.FirstOrDefault(x => x.Value == gf).Key;

            public void RemoveMagic() => StatJ = StatJ.ToDictionary(e => e.Key, e => (byte)0);

            /// <summary>
            /// Sorted Enumerable based on best to worst for Stat. Uses character's total magic and
            /// kernel bin's stat value.
            /// </summary>
            /// <param name="stat">Stat sorting by.</param>
            /// <returns>Ordered Enumerable</returns>
            public IOrderedEnumerable<MagicData> SortedMagic(Stat stat) => Memory.Kernel_Bin.MagicData.OrderBy(x => (-x.TotalStatVal(stat) * (Magics.ContainsKey(x.MagicDataID) ? Magics[x.MagicDataID] : 0)) / 100);

            public override sbyte StatusResistance(BattleOnlyStatuses s) => throw new NotImplementedException();

            public override sbyte StatusResistance(PersistentStatuses s) => throw new NotImplementedException();

            public override string ToString() => Name.Length > 0 ? Name.ToString() : base.ToString();

            // ReSharper disable once MethodOverloadWithOptionalParameter
            public ushort TotalStat(Stat s, Characters c = Characters.Blank)
            {
                if (!Enum.IsDefined(typeof(Characters), c))
                    throw new InvalidEnumArgumentException(nameof(c), (int)c, typeof(Characters));
                if (c == Characters.Blank)
                    c = _id;
                if (c != _id && c < Characters.Laguna_Loire)
                    throw new ArgumentException($"{this}::Wrong visible character value({c}). Must match ({_id}) unless Laguna Kiros or Ward!");

                int total = 0;
                if (Memory.Kernel_Bin.StatPercentAbilities != null)
                    foreach (Abilities i in Abilities)
                    {
                        if (Memory.Kernel_Bin.StatPercentAbilities.TryGetValue(i, out StatPercentageAbilities ability) && ability.Stat == s)
                            total += ability.Value;
                    }

                if (CharacterStats == null) return 0;
                // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
                switch (s)
                {
                    case Stat.HP:
                        return CharacterStats.HP((sbyte)Level, StatJ[s], StatJ[s] == 0 ? 0 : Magics[StatJ[s]], RawHP, total);

                    case Stat.EVA:
                        //TODO confirm if there is no flat stat buff for eva. If there isn't then remove from function.
                        return CharacterStats.Eva((sbyte)Level, StatJ[s], StatJ[s] == 0 ? 0 : Magics[StatJ[s]], 0, TotalStat(Stat.SPD, c), total);

                    case Stat.SPD:
                        return CharacterStats.SPD((sbyte)Level, StatJ[s], StatJ[s] == 0 ? 0 : Magics[StatJ[s]], RawStats[s], total);

                    case Stat.HIT:
                        return CharacterStats.Hit(StatJ[s], StatJ[s] == 0 ? 0 : Magics[StatJ[s]], WeaponID);

                    case Stat.Luck:
                        return CharacterStats.Luck((sbyte)Level, StatJ[s], StatJ[s] == 0 ? 0 : Magics[StatJ[s]], RawStats[s], total);

                    case Stat.MAG:
                        return CharacterStats.MAG((sbyte)Level, StatJ[s], StatJ[s] == 0 ? 0 : Magics[StatJ[s]], RawStats[s], total);

                    case Stat.SPR:
                        return CharacterStats.SPR((sbyte)Level, StatJ[s], StatJ[s] == 0 ? 0 : Magics[StatJ[s]], RawStats[s], total);

                    case Stat.STR:
                        return CharacterStats.STR((sbyte)Level, StatJ[s], StatJ[s] == 0 ? 0 : Magics[StatJ[s]], RawStats[s], total, WeaponID);

                    case Stat.VIT:
                        return CharacterStats.VIT((sbyte)Level, StatJ[s], StatJ[s] == 0 ? 0 : Magics[StatJ[s]], RawStats[s], total);

                    default:
                        throw new ArgumentOutOfRangeException(nameof(s), s, null);
                }
            }

            public override ushort TotalStat(Stat s) => TotalStat(s, ID);

            public bool Unlocked(Stat stat) => Unlocked(UnlockedGFAbilities, stat);

            public bool Unlocked(List<Abilities> unlocked, Stat stat)
            {
                switch (stat)
                {
                    case Stat.HP:
                    case Stat.STR:
                    case Stat.VIT:
                    case Stat.MAG:
                    case Stat.SPR:
                    case Stat.SPD:
                    case Stat.EVA:
                    case Stat.HIT:
                    case Stat.Luck:
                    case Stat.None:
                        throw new ArgumentOutOfRangeException(nameof(stat), stat, null);
                    default:
                        return unlocked.Contains(KernelBin.Stat2Ability[stat]);

                    case Stat.ElAtk:
                        return unlocked.Contains(Kernel.Abilities.ElAtkJ);

                    case Stat.ElDef1:
                        return unlocked.Contains(Kernel.Abilities.ElDefJ) ||
                            unlocked.Contains(Kernel.Abilities.ElDefJ2) ||
                            unlocked.Contains(Kernel.Abilities.ElDefJ4);

                    case Stat.ElDef2:
                        return unlocked.Contains(Kernel.Abilities.ElDefJ2) ||
                            unlocked.Contains(Kernel.Abilities.ElDefJ4);

                    case Stat.ElDef3:
                    case Stat.ElDef4:
                        return unlocked.Contains(Kernel.Abilities.ElDefJ4);

                    case Stat.StAtk:
                        return unlocked.Contains(Kernel.Abilities.StAtkJ);

                    case Stat.StDef1:
                        return unlocked.Contains(Kernel.Abilities.StDefJ) ||
                            unlocked.Contains(Kernel.Abilities.StDefJ2) ||
                            unlocked.Contains(Kernel.Abilities.StDefJ4);

                    case Stat.StDef2:
                        return unlocked.Contains(Kernel.Abilities.StDefJ2) ||
                            unlocked.Contains(Kernel.Abilities.StDefJ4);

                    case Stat.StDef3:
                    case Stat.StDef4:
                        return unlocked.Contains(Kernel.Abilities.StDefJ4);
                }
            }

            protected override void ReadData(BinaryReader br, Enum c)
            {
                _id = c as Characters? ?? throw new ArgumentException($"Enum {c} is not Characters");
                Name = Memory.Strings.GetName(_id, Data ?? Memory.State);
                _CurrentHP = br.ReadUInt16();//0x00
                RawHP = br.ReadUInt16();//0x02
                Experience = br.ReadUInt32();//0x04
                ModelID = br.ReadByte();//0x08
                WeaponID = br.ReadByte();//0x09
                RawStats = new Dictionary<Stat, byte>(6)
                {
                    [Stat.STR] = br.ReadByte(),//0x0A
                    [Stat.VIT] = br.ReadByte(),//0x0B
                    [Stat.MAG] = br.ReadByte(),//0x0C
                    [Stat.SPR] = br.ReadByte(),//0x0D
                    [Stat.SPD] = br.ReadByte(),//0x0E
                    [Stat.Luck] = br.ReadByte()//0x0F
                };
                Magics = new OrderedDictionary<byte, byte>(MagicCapacity);
                for (int i = 0; i < MagicCapacity; i++)
                {
                    byte key = br.ReadByte();
                    byte val = br.ReadByte();
                    if (!Magics.ContainsKey(key))
                        Magics.Add(key, val);//0x10
                }
                Commands = Array.ConvertAll(br.ReadBytes(3), item => (Abilities)item).ToList();//0x50
                PaddingOrUnusedCommand = br.ReadByte();//0x53
                Abilities = Array.ConvertAll(br.ReadBytes(4), item => (Abilities)item).ToList();//0x54
                _rawJunctionedGFs = (GFflags)br.ReadUInt16();//0x58 each bit is one gf.
                Unknown1 = br.ReadByte();//0x5A
                AlternativeModel = br.ReadByte();//0x5B (Normal, SeeD, Soldier...)
                StatJ = new Dictionary<Stat, byte>(9);
                for (int i = 0; i < 19; i++)
                {
                    Stat key = (Stat)i;
                    byte val = br.ReadByte();
                    if (!StatJ.ContainsKey(key))
                        StatJ.Add(key, val);
                }

                Unknown2 = br.ReadByte();//0x6F (padding?)
                CompatibilityWithGFs = new Dictionary<GFs, CompatibilitywithGF>(16);
                for (int i = 0; i < 16; i++)
                    CompatibilityWithGFs.Add((GFs)i, br.ReadUInt16());//0x70
                NumberOfKills = br.ReadUInt16();//0x90
                NumberOfKOs = br.ReadUInt16();//0x92
                Exists = (Exists)br.ReadByte();//0x94
                Unknown3 = br.ReadByte();//0x95
                Statuses0 = (PersistentStatuses)br.ReadByte();//0x96
                Unknown4 = br.ReadByte();//0x97
            }

            private void Auto(IEnumerable<Stat> list)
            {
                RemoveMagic();

                List<Abilities> unlockedList = UnlockedGFAbilities;
                foreach (Stat stat in list)
                {
                    if (!Unlocked(unlockedList, stat)) continue;
                    foreach (MagicData spell in SortedMagic(stat))
                    {
                        if (StatJ.ContainsValue(spell.MagicDataID)) continue;
                        //TODO make smarter.
                        //example if you can get max stat with a weaker spell use that first.

                        // if stat is max with out spell skip
                        if (stat != Stat.HP && TotalStat(stat) == KernelBin.MaxStatValue) break;
                        // if hp is max without spell skip
                        if (stat == Stat.HP && TotalStat(stat) == KernelBin.MaxHPValue) break;
                        // junction spell
                        StatJ[stat] = spell.MagicDataID;
                        break;
                    }
                }
            }

            #endregion Methods
        }

        #endregion Classes
    }
}

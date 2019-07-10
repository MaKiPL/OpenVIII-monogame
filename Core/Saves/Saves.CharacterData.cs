using Microsoft.Xna.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OpenVIII
{
    public static partial class Saves
    {
        /// <summary>
        /// </summary>
        /// <remarks>Hyne has switchlocked0 and 1</remarks>
        [Flags]
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
            /// Many have this set. I donno what it does.
            /// </summary>
            Unk8 = 0x8,

            Unk10 = 0x10,
            Unk20 = 0x20,
            Unk30 = 0x40,
            Unk40 = 0x80,
        }

        /// <summary>
        /// Data for each Character
        /// </summary>
        /// <see cref="http://wiki.ffrtt.ru/index.php/FF8/GameSaveFormat#Characters"/>
        public class CharacterData : Damageable
        {
            public FF8String Name; //not saved to file.

            /// <summary>
            /// Raw HP buff from items.
            /// </summary>
            public ushort _HP;

            /// <summary>
            /// Total Exp
            /// </summary>
            public uint Experience;

            /// <summary>
            /// Character Model
            /// </summary>
            public byte ModelID;

            /// <summary>
            /// Weapon
            /// </summary>
            public byte WeaponID;

            /// <summary>
            /// Random number generator seeded with time.
            /// </summary>
            private static Random random = new Random((int)DateTime.Now.Ticks);

            /// <summary>
            /// <para>
            /// This Generates a Crisis Level. Run this each turn in battle. Though in real game it
            /// runs when the menu pops up.
            /// </para>
            /// <para>-1 means no limit break. &gt;=0 has a limit break.</para>
            /// </summary>
            /// <returns>-1 - 4</returns>
            /// <remarks>https://finalfantasy.fandom.com/wiki/Crisis_Level</remarks>
            public sbyte GenerateCrisisLevel()
            {
                ushort current = CurrentHP();
                ushort max = MaxHP();
                if ((ID == Characters.Seifer_Almasy && CurrentHP() < (max * 84 / 100)))
                {
                    int HPMod = CharacterStats.Crisis * 10 * current / max;
                    int DeathBonus = Memory.State.DeadPartyMembers() * 200 + 1600;
                    int StatusBonus = (int)(Statuses0.Count() * 10);
                    int RandomMod = random.Next(0, 255) + 160;
                    int crisislevel = (StatusBonus + DeathBonus - HPMod) / RandomMod;
                    if (crisislevel == 5)
                    {
                        CurrentCrisisLevel = 0;
                        return CurrentCrisisLevel;
                    }
                    else if (crisislevel == 6)
                    {
                        CurrentCrisisLevel = 1;
                        return CurrentCrisisLevel;
                    }
                    else if (crisislevel == 7)
                    {
                        CurrentCrisisLevel = 2;
                        return CurrentCrisisLevel;
                    }
                    else if (crisislevel >= 8)
                    {
                        CurrentCrisisLevel = 3;
                        return CurrentCrisisLevel;
                    }
                }
                return CurrentCrisisLevel = -1;
            }

            /// <summary>
            /// Set by CrisisLevel(), -1 means no limit break. &gt;=0 has a limit break.
            /// </summary>
            /// <returns>-1 - 4</returns>
            /// <remarks>https://finalfantasy.fandom.com/wiki/Crisis_Level</remarks>
            public sbyte CurrentCrisisLevel { get; private set; }

            /// <summary>
            /// Kernel Stats
            /// </summary>
            public Kernel_bin.Character_Stats CharacterStats => Kernel_bin.CharacterStats[ID];

            /// <summary>
            /// Stats that can be incrased via items. Except for HP because it's a ushort not a byte.
            /// </summary>
            public Dictionary<Kernel_bin.Stat, byte> RawStats;

            //public byte _STR; //0x09
            //public byte _VIT; //0x0A
            //public byte _MAG; //0x0B
            //public byte _SPR; //0x0C
            //public byte _SPD; //0x0D
            //public byte _LCK; //0x0E
            public Dictionary<byte, byte> Magics;

            /// <summary>
            /// Junctioned Commands
            /// </summary>
            public List<Kernel_bin.Abilities> Commands;

            public byte Paddingorunusedcommand;

            /// <summary>
            /// Junctioned Abilities
            /// </summary>
            public List<Kernel_bin.Abilities> Abilities;

            /// <summary>
            /// Juctioned GFs
            /// </summary>
            public GFflags JunctionnedGFs;

            public byte Unknown1;

            /// <summary>
            /// <para>Alt Models/different costumes</para>
            /// <para>(Normal, SeeD, Soldier...)</para>
            /// </summary>
            public byte Alternativemodel;

            /// <summary>
            /// Junctioned Magic per stat.
            /// </summary>
            public Dictionary<Kernel_bin.Stat, byte> Stat_J;

            public byte Unknown2; //0x6B (padding?)

            /// <summary>
            /// <para>Compatibility With GFs</para>
            /// <para>Effects Summon speed and such.</para>
            /// </summary>
            public Dictionary<GFs, ushort> CompatibilitywithGFs;

            /// <summary>
            /// Number of Kills
            /// </summary>
            public ushort Numberofkills;

            /// <summary>
            /// Number of KOs
            /// </summary>
            public ushort NumberofKOs;

            /// <summary>
            /// Value determines if a character shows in menu and can be added to party.
            /// <para>
            /// 15,9,7,4,1 shows on menu, 0 locked, 6 hidden // I think I wonder if this is a flags value.
            /// </para>
            /// </summary>
            public Exists Exists;

            /// <summary>
            /// Visable
            /// </summary>
            public bool Available => (Exists & Exists.Available) != 0;

            /// <summary>
            /// Cannot remove from party or add to party.
            /// </summary>
            public bool SwitchLocked => (Exists & (Exists.SwitchLocked0 | Exists.SwitchLocked1)) != 0;

            public byte Unknown3; //0x94
            public byte Unknown4; //0x96

            public CharacterData()
            {
            }

            public CharacterData(BinaryReader br, Characters c) => Read(br, c);

            public Characters ID { get; private set; }
            public List<Kernel_bin.Abilities> UnlockedGFAbilities
            {
                get
                {
                    BitArray total = new BitArray(16 * 8);
                    List<Kernel_bin.Abilities> abilities = new List<Kernel_bin.Abilities>();
                    IEnumerable<Enum> availableFlags = Enum.GetValues(typeof(GFflags)).Cast<Enum>();
                    foreach (Enum flag in availableFlags.Where(JunctionnedGFs.HasFlag))
                    {
                        if ((GFflags)flag == GFflags.None) continue;
                        total.Or(Memory.State.GFs[ConvertGFEnum[(GFflags)flag]].Complete);
                    }
                    for (int i = 1; i < total.Length; i++)//0 is none so skipping it.
                    {
                        if (total[i])
                            abilities.Add((Kernel_bin.Abilities)i);
                    }

                    return abilities;
                }
            }

            /// <summary>
            /// Sorted Enumerable based on best to worst for Stat. Uses character's total magic and
            /// kernel bin's stat value.
            /// </summary>
            /// <param name="Stat">Stat sorting by.</param>
            /// <returns>Ordered Enumberable</returns>
            public IOrderedEnumerable<Kernel_bin.Magic_Data> SortedMagic(Kernel_bin.Stat Stat) => Kernel_bin.MagicData.OrderBy(x => (-x.totalStatVal(Stat) * (Magics.ContainsKey(x.ID) ? Magics[x.ID] : 0)) / 100);

            public void RemoveMagic() => Stat_J = Stat_J.ToDictionary(e => e.Key, e => (byte)0);

            public int CriticalHP(Characters value) => MaxHP(value) / 4;

            public void RemoveAll()
            {
                Stat_J = Stat_J.ToDictionary(e => e.Key, e => (byte)0);
                Commands = Commands.ConvertAll(Item => Kernel_bin.Abilities.None);
                Abilities = Abilities.ConvertAll(Item => Kernel_bin.Abilities.None);
                JunctionnedGFs = GFflags.None;
            }

            public void Read(BinaryReader br, Characters c)
            {
                ID = c;
                _CurrentHP = br.ReadUInt16();//0x00
                _HP = br.ReadUInt16();//0x02
                Experience = br.ReadUInt32();//0x04
                ModelID = br.ReadByte();//0x08
                WeaponID = br.ReadByte();//0x09
                RawStats = new Dictionary<Kernel_bin.Stat, byte>(6)
                {
                    [Kernel_bin.Stat.STR] = br.ReadByte(),//0x0A
                    [Kernel_bin.Stat.VIT] = br.ReadByte(),//0x0B
                    [Kernel_bin.Stat.MAG] = br.ReadByte(),//0x0C
                    [Kernel_bin.Stat.SPR] = br.ReadByte(),//0x0D
                    [Kernel_bin.Stat.SPD] = br.ReadByte(),//0x0E
                    [Kernel_bin.Stat.LUCK] = br.ReadByte()//0x0F
                };
                Magics = new Dictionary<byte, byte>(33);
                for (int i = 0; i < 32; i++)
                {
                    byte key = br.ReadByte();
                    byte val = br.ReadByte();
                    if (key >= 0 && !Magics.ContainsKey(key))
                        Magics.Add(key, val);//0x10
                }
                Commands = Array.ConvertAll(br.ReadBytes(3), Item => (Kernel_bin.Abilities)Item).ToList();//0x50
                Paddingorunusedcommand = br.ReadByte();//0x53
                Abilities = Array.ConvertAll(br.ReadBytes(4), Item => (Kernel_bin.Abilities)Item).ToList();//0x54
                JunctionnedGFs = (GFflags)br.ReadUInt16();//0x58
                Unknown1 = br.ReadByte();//0x5A
                Alternativemodel = br.ReadByte();//0x5B (Normal, SeeD, Soldier...)
                Stat_J = new Dictionary<Kernel_bin.Stat, byte>(9);
                for (int i = 0; i < 19; i++)
                {
                    Kernel_bin.Stat key = (Kernel_bin.Stat)i;
                    byte val = br.ReadByte();
                    if (!Stat_J.ContainsKey(key))
                        Stat_J.Add(key, val);
                }
                //JunctionHP = br.ReadByte();//0x5C
                //JunctionSTR = br.ReadByte();//0x5D
                //JunctionVIT = br.ReadByte();//0x5E
                //JunctionMAG = br.ReadByte();//0x5F
                //JunctionSPR = br.ReadByte();//0x60
                //JunctionSPD = br.ReadByte();//0x61
                //JunctionEVA = br.ReadByte();//0x62
                //JunctionHIT = br.ReadByte();//0x63
                //JunctionLCK = br.ReadByte();//0x64
                //Elem_Atk_J = br.ReadByte();//0x65
                //ST_Atk_J = br.ReadByte();//0x66
                //Elem_Def_J = br.ReadBytes(4).ToList() ;//0x67
                //ST_Def_J = br.ReadBytes(4).ToList();//0x6B
                Unknown2 = br.ReadByte();//0x6F (padding?)
                CompatibilitywithGFs = new Dictionary<GFs, ushort>(16);
                for (int i = 0; i < 16; i++)
                    CompatibilitywithGFs.Add((GFs)i, br.ReadUInt16());//0x70
                Numberofkills = br.ReadUInt16();//0x90
                NumberofKOs = br.ReadUInt16();//0x92
                Exists = (Exists)br.ReadByte();//0x94
                                               //switch((int)Exists)
                                               //{
                                               //    case 0:
                                               //        break;
                                               //    case 1:
                                               //        break;
                                               //    case 9:
                                               //        break;
                                               //    case 6:
                                               //        break;
                                               //    case 7:
                                               //        break;
                                               //    case 15:
                                               //        break;
                                               //    default:
                                               //    break;
                                               //}
                Unknown3 = br.ReadByte();//0x95
                Statuses0 = (Kernel_bin.Persistant_Statuses)br.ReadByte();//0x96
                Unknown4 = br.ReadByte();//0x97
            }

            public void AutoMAG() => Auto(Kernel_bin.AutoMAG);

            public void AutoDEF() => Auto(Kernel_bin.AutoDEF);

            public void AutoATK() => Auto(Kernel_bin.AutoATK);

            private void Auto(IReadOnlyList<Kernel_bin.Stat> list)
            {
                RemoveMagic();

                List<Kernel_bin.Abilities> unlockedlist = UnlockedGFAbilities;
                foreach (Kernel_bin.Stat stat in list)
                {
                    if (Unlocked(unlockedlist, stat))
                        foreach (Kernel_bin.Magic_Data spell in SortedMagic(stat))
                        {
                            if (!Stat_J.ContainsValue(spell.ID))
                            {
                                //TODO make smarter.
                                //example if you can get max stat with a weaker spell use that first.

                                // if stat is max with out spell skip
                                if (stat != Kernel_bin.Stat.HP && TotalStat(stat) == Kernel_bin.MAX_STAT_VALUE) break;
                                // if hp is max without spell skip
                                else if (stat == Kernel_bin.Stat.HP && TotalStat(stat) == Kernel_bin.MAX_HP_VALUE) break;
                                // junction spell
                                else Stat_J[stat] = spell.ID;
                                break;
                            }
                        }
                }
            }

            public bool Unlocked(Kernel_bin.Stat stat) => Unlocked(UnlockedGFAbilities, stat);

            public bool Unlocked(List<Kernel_bin.Abilities> unlocked, Kernel_bin.Stat stat)
            {
                switch (stat)
                {
                    default:
                        return unlocked.Contains(Kernel_bin.Stat2Ability[stat]);

                    case Kernel_bin.Stat.EL_Atk:
                        return unlocked.Contains(Kernel_bin.Abilities.EL_Atk_J);

                    case Kernel_bin.Stat.EL_Def_1:
                        return unlocked.Contains(Kernel_bin.Abilities.EL_Def_Jx1) ||
                            unlocked.Contains(Kernel_bin.Abilities.EL_Def_Jx2) ||
                            unlocked.Contains(Kernel_bin.Abilities.EL_Def_Jx4);

                    case Kernel_bin.Stat.EL_Def_2:
                        return unlocked.Contains(Kernel_bin.Abilities.EL_Def_Jx2) ||
                            unlocked.Contains(Kernel_bin.Abilities.EL_Def_Jx4);

                    case Kernel_bin.Stat.EL_Def_3:
                    case Kernel_bin.Stat.EL_Def_4:
                        return unlocked.Contains(Kernel_bin.Abilities.EL_Def_Jx4);

                    case Kernel_bin.Stat.ST_Atk:
                        return unlocked.Contains(Kernel_bin.Abilities.ST_Atk_J);

                    case Kernel_bin.Stat.ST_Def_1:
                        return unlocked.Contains(Kernel_bin.Abilities.ST_Def_Jx1) ||
                            unlocked.Contains(Kernel_bin.Abilities.ST_Def_Jx2) ||
                            unlocked.Contains(Kernel_bin.Abilities.ST_Def_Jx4);

                    case Kernel_bin.Stat.ST_Def_2:
                        return unlocked.Contains(Kernel_bin.Abilities.ST_Def_Jx2) ||
                            unlocked.Contains(Kernel_bin.Abilities.ST_Def_Jx4);

                    case Kernel_bin.Stat.ST_Def_3:
                    case Kernel_bin.Stat.ST_Def_4:
                        return unlocked.Contains(Kernel_bin.Abilities.ST_Def_Jx4);
                }
            }

            public byte Level => CharacterStats.LEVEL(Experience);
            public ushort ExperienceToNextLevel => (ushort)(Level == 100 ? 0 : MathHelper.Clamp(CharacterStats.EXP(Level) - Experience, 0, CharacterStats.EXP(2)));

            /// <summary>
            /// Max HP
            /// </summary>
            /// <param name="c">Force another character's HP calculation</param>
            /// <returns></returns>
            public ushort MaxHP(Characters c) => TotalStat(Kernel_bin.Stat.HP, c);

            public override ushort MaxHP()
            {
                int ind = -1;
                if (Memory.State?.Characters != null && Memory.State.TeamLaguna && (ind = Memory.State.PartyData.FindIndex(u => u == ID)) >= 0)
                {
                    return MaxHP(Memory.State.Party[ind]);
                }
                return MaxHP(ID);
            }

            public ushort TotalStat(Kernel_bin.Stat s, Characters c = Characters.Blank)
            {
                if (c == Characters.Blank)
                    c = ID;
                int total = 0;
                foreach (Kernel_bin.Abilities i in Abilities)
                {
                    if (Kernel_bin.Statpercentabilities.ContainsKey(i) && Kernel_bin.Statpercentabilities[i].Stat == s)
                        total += Kernel_bin.Statpercentabilities[i].Value;
                }

                switch (s)
                {
                    case Kernel_bin.Stat.HP:
                        return CharacterStats.HP((sbyte)Level, Stat_J[s], Stat_J[s] == 0 ? 0 : Magics[Stat_J[s]], _HP, total);

                    case Kernel_bin.Stat.EVA:
                        //TODO confirm if there is no flat stat buff for eva. If there isn't then remove from function.
                        return CharacterStats.EVA((sbyte)Level, Stat_J[s], Stat_J[s] == 0 ? 0 : Magics[Stat_J[s]], 0, TotalStat(Kernel_bin.Stat.SPD, c), total);

                    case Kernel_bin.Stat.SPD:
                        return CharacterStats.SPD((sbyte)Level, Stat_J[s], Stat_J[s] == 0 ? 0 : Magics[Stat_J[s]], RawStats[s], total);

                    case Kernel_bin.Stat.HIT:
                        return CharacterStats.HIT(Stat_J[s], Stat_J[s] == 0 ? 0 : Magics[Stat_J[s]], WeaponID);

                    case Kernel_bin.Stat.LUCK:
                        return CharacterStats.LUCK((sbyte)Level, Stat_J[s], Stat_J[s] == 0 ? 0 : Magics[Stat_J[s]], RawStats[s], total);

                    case Kernel_bin.Stat.MAG:
                        return CharacterStats.MAG((sbyte)Level, Stat_J[s], Stat_J[s] == 0 ? 0 : Magics[Stat_J[s]], RawStats[s], total);

                    case Kernel_bin.Stat.SPR:
                        return CharacterStats.SPR((sbyte)Level, Stat_J[s], Stat_J[s] == 0 ? 0 : Magics[Stat_J[s]], RawStats[s], total);

                    case Kernel_bin.Stat.STR:
                        return CharacterStats.STR((sbyte)Level, Stat_J[s], Stat_J[s] == 0 ? 0 : Magics[Stat_J[s]], RawStats[s], total, WeaponID);

                    case Kernel_bin.Stat.VIT:
                        return CharacterStats.VIT((sbyte)Level, Stat_J[s], Stat_J[s] == 0 ? 0 : Magics[Stat_J[s]], RawStats[s], total);
                }
                return 0;
            }

            public override ushort CurrentHP()
            {
                int ind = -1;
                if (Memory.State?.Characters != null && Memory.State.TeamLaguna && (ind = Memory.State.PartyData.FindIndex(u => u == ID)) >= 0)
                {
                    return CurrentHP(Memory.State.Party[ind]);
                }
                return CurrentHP(ID);
            }

            public ushort CurrentHP(Characters c)
            {
                ushort max = MaxHP(c);
                if (max < _CurrentHP) _CurrentHP = max;
                return _CurrentHP;
            }

            public void JunctionSpell(Kernel_bin.Stat stat, byte spell)
            {
                //see if magic is in use, if so remove it
                if (Stat_J.ContainsValue(spell))
                {
                    Kernel_bin.Stat key = Stat_J.FirstOrDefault(x => x.Value == spell).Key;
                    Stat_J[key] = 0;
                }
                //junction magic
                Stat_J[stat] = spell;
            }

            public override float PercentFullHP()
            {
                int ind = -1;
                if (Memory.State?.Characters != null && Memory.State.TeamLaguna && (ind = Memory.State.PartyData.FindIndex(u => u == ID)) >= 0)
                {
                    return PercentFullHP(Memory.State.Party[ind]);
                }
                return PercentFullHP(ID);
            }

            public float PercentFullHP(Characters c) => (float)CurrentHP(c) / MaxHP(c);

            public override string ToString() => Name.Length > 0 ? Name.ToString() : base.ToString();

            public CharacterData Clone()
            {
                //Shadowcopy
                CharacterData c = (CharacterData)MemberwiseClone();
                //Deepcopy
                c.Name = Name.Clone();
                c.CompatibilitywithGFs = CompatibilitywithGFs.ToDictionary(e => e.Key, e => e.Value);
                c.Stat_J = Stat_J.ToDictionary(e => e.Key, e => e.Value);
                c.Magics = Magics.ToDictionary(e => e.Key, e => e.Value);
                c.RawStats = RawStats.ToDictionary(e => e.Key, e => e.Value);
                c.Commands = Commands.ConvertAll(Item => Item);
                c.Abilities = Abilities.ConvertAll(Item => Item);
                return c;
            }
        }
    }
}
using Microsoft.Xna.Framework;
using OpenVIII.Battle.Dat;
using OpenVIII.Kernel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OpenVIII
{
    public class Enemy : Damageable, IEnemy
    {
        #region Fields

        private const int StatusDefault = 100;
        private bool _mugged;

        #endregion Fields

        #region Constructors

        private Enemy()
        {
        }

        #endregion Constructors

        #region Properties

        public static List<Enemy> Party { get; set; }

        public Battle.Dat.Abilities[] Abilities => HML(Info.AbilitiesHigh, Info.AbilitiesMed, Info.AbilitiesLow);

        public byte AP => Info.AP;

        public Devour Devour => Info.Devour[LevelGroup()] >= Memory.Kernel_Bin.Devour.Count ?
            Memory.Kernel_Bin.Devour[Memory.Kernel_Bin.Devour.Count - 1] :
            Memory.Kernel_Bin.Devour[Info.Devour[LevelGroup()]];

        public Magic[] DrawList => HML(Info.DrawHigh, Info.DrawMed, Info.DrawLow);

        /// <summary>
        /// Randomly gain 1 or 0 from this list.
        /// </summary>
        public Saves.Item[] DropList => HML(Info.DropHigh, Info.DropMed, Info.DropLow);

        public byte DropRate => (byte)(MathHelper.Clamp(Info.DropRate * 100 / byte.MaxValue, 0, 100));

        public Battle.EnemyInstanceInformation EII { get; set; }

        public IEnumerable<EnemyAttacksData> EnemyAttacksDatas => Abilities.Where(x => x.MONSTER != null).Select(x => x.MONSTER);

        public override byte EVA => Convert2(Info.EVA);

        /// <summary>
        /// The EXP everyone gets.
        /// </summary>
        public override int EXP => Convert3(Info.Exp, Memory.State.AveragePartyLevel);

        public byte FixedLevel { get; set; }

        /// <summary>
        /// Enemy attacks determine hit%
        /// </summary>
        /// <remarks>
        /// LeythalKnight Today at 1:53 PM Enemy attack accuracy is set per ability based on the value
        /// that's called "Attack param" in Doomtrain, and only when they use attack types that have
        /// a miss chance
        /// </remarks>
        public override byte HIT => 0;

        public Information Info => EII.Data.Information;

        public IEnumerable<GFs> JunctionedGFs => Memory.State != null ? DrawList.Select(x => x.GF).Where(gf => gf >= GFs.Quezacotl && gf <= GFs.Eden && !Memory.State.UnlockedGFs.Contains(gf)).Distinct() : null;

        /// <summary>
        /// Level of enemy based on average of party or fixed value.
        /// </summary>
        /// <see cref="https://finalfantasy.fandom.com/wiki/Level#Enemy_levels"/>
        public override byte Level
        {
            get
            {
                if (FixedLevel != default)
                    return FixedLevel;
                byte a = Memory.State.AveragePartyLevel;
                byte d = (byte)(a / 5);
                return (byte)MathHelper.Clamp(a + d, 1, 100);
            }
        }

        /// <summary>
        /// unknown luck stat. putting 0 in here so there is something.
        /// </summary>
        public override byte LUCK => 0;

        public override byte MAG => Convert1(Info.MAG);

        /// <summary>
        /// Randomly gain 1 or 0 from this list.
        /// </summary>
        public Saves.Item[] MugList => HML(Info.MugHigh, Info.MugMed, Info.MugLow);

        public byte MugRate => (byte)(MathHelper.Clamp(Info.MugRate * 100 / byte.MaxValue, 0, 100));

        public override FF8String Name => Info.Name;

        public override byte SPD => Convert2(Info.SPD);

        public override byte SPR => Convert2(Info.SPR);

        public override byte STR => Convert1(Info.STR);

        public override byte VIT => Convert2(Info.VIT);

        #endregion Properties

        #region Methods

        public static implicit operator Battle.EnemyInstanceInformation(Enemy @in) => @in.EII;

        public static implicit operator Enemy(Battle.EnemyInstanceInformation @in) => Load(@in);

        public static Enemy Load(Battle.EnemyInstanceInformation eii, byte fixedLevel = 0, ushort? startingHP = null)
        {
            Enemy r = new Enemy
            {
                EII = eii,
                FixedLevel = fixedLevel
            };
            r._CurrentHP = startingHP ?? r.MaxHP();
            if ((r.Info.BitSwitch & Flag1.Zombie) != 0)
            {
                r.Statuses0 |= PersistentStatuses.Zombie;
            }
            if ((r.Info.BitSwitch & Flag1.AutoProtect) != 0)
            {
                r.Statuses1 |= BattleOnlyStatuses.Protect;
            }
            if ((r.Info.BitSwitch & Flag1.AutoReflect) != 0)
            {
                r.Statuses1 |= BattleOnlyStatuses.Reflect;
            }
            if ((r.Info.BitSwitch & Flag1.AutoShell) != 0)
            {
                r.Statuses1 |= BattleOnlyStatuses.Shell;
            }
            if ((r.Info.BitSwitch & Flag1.Fly) != 0)
            {
                r.Statuses1 |= BattleOnlyStatuses.Float;
            }
            r.Init();
            return r;
        }

        /// <summary>
        /// Return card if succeed at roll
        /// </summary>
        /// <returns></returns>
        /// <see cref="https://gamefaqs.gamespot.com/ps/197343-final-fantasy-viii/faqs/58936"/>
        public Cards.ID Card()
        {
            if (Info.Card.Skip(1).All(x => x == Cards.ID.Immune)) return Cards.ID.Immune;
            int p = (256 * MaxHP() - 255 * CurrentHP()) / MaxHP();
            int r = Memory.Random.Next(256);
            // 2 is rare card, 1 is normal card 0 per ifrit.
            return r < (p + 1) ? (r < 17 ? Info.Card[2] : Info.Card[1]) : Cards.ID.Fail;
        }

        public Cards.ID CardDrop()
        {
            //9 / 256
            if (Info.Card[0] == Cards.ID.Immune) return Info.Card[1];

            int r = Memory.Random.Next(256);
            return r < 9 ? Info.Card[1] : Cards.ID.Fail;
        }

        public override Damageable Clone() => throw new NotImplementedException();

        public Saves.Item Drop(bool rareItem = false)
        {
            if (_mugged) return default;
            int percent = DropRate;
            Saves.Item[] list = DropList;
            int i = Memory.Random.Next(100 + 1);
            if (i >= percent || list.Length <= 0) return default;
            //Slot              |  0        | 1         | 2        | 3
            //------------------|  ---------| --------- | ---------| --------
            //Without Rare Item | 178 / 256 | 51 / 256  | 15 / 256 | 12 / 256
            //------------------|  ---------| --------- | ---------| --------
            //With Rare Item    | 128 / 256 | 114 / 256 | 14 / 256 | 0 / 256 <- kinda makes no scene to me
            int r = Memory.Random.Next(256);
            if (rareItem)
            {
                if (r < 128)
                    return list[0];
                if ((r -= 128) < 114)
                    return list[1];
                return (r - 114) < 14 ? list[2] : list[3];
            }
            if (r < 178)
                return list[0];
            if ((r -= 178) < 51)
                return list[1];
            return (r - 51) < 15 ? list[2] : list[3];
        }

        public override short ElementalResistance(Element @in)
        {
            List<Element> l = (Enum.GetValues(typeof(Element))).Cast<Element>().ToList();
            return @in == Element.NonElemental
                ? (short)100
                : conv(Info.Resistance[l.FindIndex(x => (x & @in) != 0) - 1]);
            short conv(byte val) => (short)MathHelper.Clamp(900 - (val * 10), -100, 400);
        }

        /// <summary>
        /// The character whom lands the last hit gets a little bonus xp.
        /// </summary>
        /// <param name="lastHitLevel">Level of character whom got last hit.</param>
        /// <returns></returns>
        public int EXPExtra(byte lastHitLevel) => Convert3(Info.ExpExtra, lastHitLevel);

        public override ushort MaxHP()
        {
            //from Ifrit's help file
            if (Info.HP == null)
                return 0;
            int i = (Info.HP[0] * Level * Level / 20) + (Info.HP[0] + Info.HP[2] * 100) * Level + Info.HP[1] * 10 + Info.HP[3] * 1000;
            return (ushort)MathHelper.Clamp(i, 0, ushort.MaxValue);
        }

        public Saves.Item Mug(byte spd, bool rareItem = false)
        {
            if (_mugged) return default;
            int percent = (MugRate + spd);
            Saves.Item[] list = DropList;
            int i = Memory.Random.Next(100 + 1);
            try
            {
                if (i < percent && list.Length > 0)
                {
                    byte r = (byte)Memory.Random.Next(256);
                    if (rareItem)
                    {
                        if (r < 128)
                            return list[0];
                        else if ((r -= 128) < 114)
                            return list[1];
                        else if ((r - 114) < 14)
                            return list[2];
                        else
                            return list[3];
                    }
                    if (r < 178)
                        return list[0];
                    else if ((r -= 178) < 51)
                        return list[1];
                    else if ((r - 51) < 15)
                        return list[2];
                    else
                        return list[3];
                }
            }
            finally { _mugged = true; }
            _mugged = false;
            return default;
        }

        /// <summary>
        /// I notice that the resistance reported on the wiki is 100 less than the number in the data.
        /// </summary>
        /// <param name="s">status effect</param>
        /// <returns>percent of resistance</returns>
        /// <see cref="https://finalfantasy.fandom.com/wiki/G-Soldier#Stats"/>
        public override sbyte StatusResistance(PersistentStatuses s)
        {
            byte r = 100;
            switch (s)
            {
                case PersistentStatuses.Death:
                    r = Info.DeathResistanceMental;
                    break;

                case PersistentStatuses.Poison:
                    r = Info.PoisonResistanceMental;
                    break;

                case PersistentStatuses.Petrify:
                    r = Info.PetrifyResistanceMental;
                    break;

                case PersistentStatuses.Darkness:
                    r = Info.DarknessResistanceMental;
                    break;

                case PersistentStatuses.Silence:
                    r = Info.SilenceResistanceMental;
                    break;

                case PersistentStatuses.Berserk:
                    r = Info.BerserkResistanceMental;
                    break;

                case PersistentStatuses.Zombie:
                    r = Info.ZombieResistanceMental;
                    break;
            }

            return (sbyte)MathHelper.Clamp(r - 100, -100, 100);
        }

        public override sbyte StatusResistance(BattleOnlyStatuses s)

        {
            byte r = StatusDefault;
            switch (s)
            {
                case BattleOnlyStatuses.Sleep:
                    r = Info.SleepResistanceMental;
                    break;

                case BattleOnlyStatuses.Haste:
                    r = Info.HasteResistanceMental;
                    break;

                case BattleOnlyStatuses.Slow:
                    r = Info.SlowResistanceMental;
                    break;

                case BattleOnlyStatuses.Stop:
                    r = Info.StopResistanceMental;
                    break;

                case BattleOnlyStatuses.Regen:
                    r = Info.RegenResistanceMental;
                    break;

                case BattleOnlyStatuses.Protect:
                    break;

                case BattleOnlyStatuses.Shell:
                    break;

                case BattleOnlyStatuses.Reflect:
                    r = Info.ReflectResistanceMental;
                    break;

                case BattleOnlyStatuses.Aura:
                    break;

                case BattleOnlyStatuses.Curse:
                    break;

                case BattleOnlyStatuses.Doom:
                    r = Info.DoomResistanceMental;
                    break;

                case BattleOnlyStatuses.Invincible:
                    break;

                case BattleOnlyStatuses.Petrifying:
                    r = Info.SlowPetrifyResistanceMental;
                    break;

                case BattleOnlyStatuses.Float:
                    r = Info.FloatResistanceMental;
                    break;

                case BattleOnlyStatuses.Confuse:
                    r = Info.ConfuseResistanceMental;
                    break;

                case BattleOnlyStatuses.Drain:
                    r = Info.DrainResistanceMental;
                    break;

                case BattleOnlyStatuses.Eject:
                    r = Info.ExpulsionResistanceMental;
                    break;

                case BattleOnlyStatuses.None:
                    break;

                case BattleOnlyStatuses.Double:
                    break;

                case BattleOnlyStatuses.Triple:
                    break;

                case BattleOnlyStatuses.Defend:
                    break;

                case BattleOnlyStatuses.Unk0X100000:
                    break;

                case BattleOnlyStatuses.Unk0X200000:
                    break;

                case BattleOnlyStatuses.Charged:
                    break;

                case BattleOnlyStatuses.BackAttack:
                    break;

                case BattleOnlyStatuses.Vit0:
                    break;

                case BattleOnlyStatuses.AngelWing:
                    break;

                case BattleOnlyStatuses.Unk0X4000000:
                    break;

                case BattleOnlyStatuses.Unk0X8000000:
                    break;

                case BattleOnlyStatuses.Unk0X10000000:
                    break;

                case BattleOnlyStatuses.Unk0X20000000:
                    break;

                case BattleOnlyStatuses.HasMagic:
                    break;

                case BattleOnlyStatuses.SummonGF:
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(s), s, null);
            }
            return (sbyte)MathHelper.Clamp(r - 100, -100, 100);
        }

        /// <summary>
        /// I notice that the resistance reported on the wiki is 100 less than the number in the data.
        /// </summary>
        /// <param name="s">status effect</param>
        /// <returns>percent of resistance</returns>
        /// <see cref="https://finalfantasy.fandom.com/wiki/G-Soldier#Stats"/>
        /// <summary>
        /// The wiki says some areas have forced or random levels. This lets you override the level.
        /// </summary>
        /// <see cref="https://finalfantasy.fandom.com/wiki/Level#Enemy_levels"/>
        public override string ToString() => Name.Value_str;

        public override ushort TotalStat(Stat s)
        {
            switch (s)
            {
                case Stat.HP:
                    return CurrentHP();

                case Stat.EVA:
                    //TODO confirm if there is no flat stat buff for eva. If there isn't then remove from function.
                    return EVA;

                case Stat.SPD:
                    return SPD;

                case Stat.HIT:
                    return HIT;

                case Stat.Luck:
                    return LUCK;

                case Stat.MAG:
                    return MAG;

                case Stat.SPR:
                    return SPR;

                case Stat.STR:
                    return STR;

                case Stat.VIT:
                    return VIT;

                case Stat.ElAtk:
                    break;

                case Stat.StAtk:
                    break;

                case Stat.ElDef1:
                    break;

                case Stat.ElDef2:
                    break;

                case Stat.ElDef3:
                    break;

                case Stat.ElDef4:
                    break;

                case Stat.StDef1:
                    break;

                case Stat.StDef2:
                    break;

                case Stat.StDef3:
                    break;

                case Stat.StDef4:
                    break;

                case Stat.None:
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(s), s, null);
            }
            return 0;
        }

        protected override void ReadData(BinaryReader br, Enum @enum) => throw new NotImplementedException("This method is not used by Enemy");

        private byte Convert1(IReadOnlyList<byte> @in)
        {
            //from Ifrit's help file
            byte level = Level;
            int i = level * @in[0] / 10 + level / @in[1] - level * level / 2 / (@in[3] + @in[2]) / 4;
            //PLEASE NOTE: I'm not 100% sure on the STR/MAG formula, but it should be accurate enough to get the general idea.
            // wiki states something like ([3(Lv)] + [(Lv) / 5] - [(Lv)² / 260] + 12) / 4

            return (byte)MathHelper.Clamp(i, 0, byte.MaxValue);
        }

        private byte Convert2(IReadOnlyList<byte> @in)
        {
            //from Ifrit's help file
            byte level = Level;
            int i = level / @in[1] - level / @in[3] + level * @in[0] + @in[2];
            return (byte)MathHelper.Clamp(i, 0, byte.MaxValue);
        }

        private int Convert3(ushort @in, byte inLevel)
        {
            //from Ifrit's help file
            byte level = Level;
            if (inLevel == 0)
                return 0;
            return @in * (5 * (level - inLevel) / inLevel + 4);
        }

        private T HML<T>(T h, T m, T l)
        {
            byte level = Level;
            if (level > Info.HighLevelStart)
                return h;
            if (level > Info.MedLevelStart)
                return m;
            return l;
        }

        private int LevelGroup()
        {
            byte l = Level;
            if (l > Info.HighLevelStart)
                return 2;
            return l > Info.MedLevelStart ? 1 : 0;
        }

        #endregion Methods
    }
}
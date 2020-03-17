using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using OpenVIII.Battle.Dat;
using OpenVIII.Kernel;

namespace OpenVIII
{
    public class Enemy : Damageable, IEnemy
    {
        #region Fields

        private const int statusdefault = 100;
        private byte _fixedLevel;
        private bool mugged = false;

        #endregion Fields

        #region Constructors

        private Enemy()
        {
        }

        #endregion Constructors

        #region Properties

        public static List<Enemy> Party { get; set; }

        public Battle.Dat.Abilities[] Abilities => hml(Info.abilitiesHigh, Info.abilitiesMed, Info.abilitiesLow);

        public byte AP => Info.ap;

        public Kernel.Devour Devour => Info.devour[levelgroup()] >= Memory.Kernel_Bin.Devour.Count ?
            Memory.Kernel_Bin.Devour[Memory.Kernel_Bin.Devour.Count - 1] :
            Memory.Kernel_Bin.Devour[Info.devour[levelgroup()]];

        public Magic[] DrawList => hml(Info.drawhigh, Info.drawmed, Info.drawlow);

        /// <summary>
        /// Randomly gain 1 or 0 from this list.
        /// </summary>
        public Saves.Item[] DropList => hml(Info.drophigh, Info.dropmed, Info.droplow);

        public byte DropRate => (byte)(MathHelper.Clamp(Info.dropRate * 100 / byte.MaxValue, 0, 100));

        public Battle.EnemyInstanceInformation EII { get; set; }

        public IEnumerable<Kernel.EnemyAttacksData> Enemy_Attacks_Datas => Abilities.Where(x => x.MONSTER != null).Select(x => x.MONSTER);

        public override byte EVA => convert2(Info.eva);

        /// <summary>
        /// The EXP everyone gets.
        /// </summary>
        public override int EXP => convert3(Info.exp, Memory.State.AveragePartyLevel);

        public byte FixedLevel { get => _fixedLevel; set => _fixedLevel = value; }

        /// <summary>
        /// Enemy attacks determine hit%
        /// </summary>
        /// <remarks>
        /// LeythalknightToday at 1:53 PM Enemy attack accuracy is set per ability based on the value
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

        public override byte MAG => convert1(Info.mag);

        /// <summary>
        /// Randomly gain 1 or 0 from this list.
        /// </summary>
        public Saves.Item[] MugList => hml(Info.mughigh, Info.mugmed, Info.muglow);

        public byte MugRate => (byte)(MathHelper.Clamp(Info.mugRate * 100 / byte.MaxValue, 0, 100));

        public override FF8String Name => Info.name;

        public override byte SPD => convert2(Info.spd);

        public override byte SPR => convert2(Info.spr);

        public override byte STR => convert1(Info.str);

        public override byte VIT => convert2(Info.vit);

        #endregion Properties

        #region Methods

        public static implicit operator Battle.EnemyInstanceInformation(Enemy @in) => @in.EII;

        public static implicit operator Enemy(Battle.EnemyInstanceInformation @in) => Load(@in);

        public static Enemy Load(Battle.EnemyInstanceInformation eII, byte fixedLevel = 0, ushort? startinghp = null)
        {
            Enemy r = new Enemy
            {
                EII = eII,
                FixedLevel = fixedLevel
            };
            r._CurrentHP = startinghp ?? r.MaxHP();
            if ((r.Info.bitSwitch & Information.Flag1.Zombie) != 0)
            {
                r.Statuses0 |= Kernel.PersistentStatuses.Zombie;
            }
            if ((r.Info.bitSwitch & Information.Flag1.Auto_Protect) != 0)
            {
                r.Statuses1 |= Kernel.BattleOnlyStatuses.Protect;
            }
            if ((r.Info.bitSwitch & Information.Flag1.Auto_Reflect) != 0)
            {
                r.Statuses1 |= Kernel.BattleOnlyStatuses.Reflect;
            }
            if ((r.Info.bitSwitch & Information.Flag1.Auto_Shell) != 0)
            {
                r.Statuses1 |= Kernel.BattleOnlyStatuses.Shell;
            }
            if ((r.Info.bitSwitch & Information.Flag1.Fly) != 0)
            {
                r.Statuses1 |= Kernel.BattleOnlyStatuses.Float;
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
            if (Info.card.Skip(1).All(x => x == Cards.ID.Immune)) return Cards.ID.Immune;
            int p = (256 * MaxHP() - 255 * CurrentHP()) / MaxHP();
            int r = Memory.Random.Next(256);
            // 2 is rare card, 1 is normal card 0 per ifrit.
            return r < (p + 1) ? (r < 17 ? Info.card[2] : Info.card[1]) : Cards.ID.Fail;
        }

        public Cards.ID CardDrop()
        {
            //9 / 256
            if (Info.card[0] == Cards.ID.Immune) return Info.card[1];

            int r = Memory.Random.Next(256);
            return r < 9 ? Info.card[1] : Cards.ID.Fail;
        }

        public override Damageable Clone() => throw new NotImplementedException();

        public Saves.Item Drop(bool RareITEM = false)
        {
            if (mugged) return default;
            int percent = DropRate;
            Saves.Item[] list = DropList;
            int i = Memory.Random.Next(100 + 1);
            if (i < percent && list.Length > 0)
            {
                //Slot              |  0        | 1         | 2        | 3
                //------------------|  ---------| --------- | ---------| --------
                //Without Rare Item | 178 / 256 | 51 / 256  | 15 / 256 | 12 / 256
                //------------------|  ---------| --------- | ---------| --------
                //With Rare Item    | 128 / 256 | 114 / 256 | 14 / 256 | 0 / 256 <- kinda makes no sence to me
                int r = Memory.Random.Next(256);
                if (RareITEM)
                {
                    if (r < 128)
                        return list[0];
                    else if ((r -= 128) < 114)
                        return list[1];
                    else if ((r -= 114) < 14)
                        return list[2];
                    else
                        return list[3];
                }
                if (r < 178)
                    return list[0];
                else if ((r -= 178) < 51)
                    return list[1];
                else if ((r -= 51) < 15)
                    return list[2];
                else
                    return list[3];
            }
            return default;
        }

        public override short ElementalResistance(Kernel.Element @in)
        {
            List<Kernel.Element> l = (Enum.GetValues(typeof(Kernel.Element))).Cast<Kernel.Element>().ToList();
            if (@in == Kernel.Element.NonElemental)

                return 100;
            // I wonder if i should average the resistances in cases of multiple elements.
            else
                return conv(Info.resistance[l.FindIndex(x => (x & @in) != 0) - 1]);
            short conv(byte val) => (short)MathHelper.Clamp(900 - (val * 10), -100, 400);
        }

        /// <summary>
        /// The character whom lands the last hit gets alittle bonus xp.
        /// </summary>
        /// <param name="lasthitlevel">Level of character whom got last hit.</param>
        /// <returns></returns>
        public int EXPExtra(byte lasthitlevel) => convert3(Info.expExtra, lasthitlevel);

        public override ushort MaxHP()
        {
            //from Ifrit's help file
            if (Info.hp == null)
                return 0;
            int i = (Info.hp[0] * Level * Level / 20) + (Info.hp[0] + Info.hp[2] * 100) * Level + Info.hp[1] * 10 + Info.hp[3] * 1000;
            return (ushort)MathHelper.Clamp(i, 0, ushort.MaxValue);
        }

        public Saves.Item Mug(byte spd, bool RareITEM = false)
        {
            if (mugged) return default;
            int percent = (MugRate + spd);
            Saves.Item[] list = DropList;
            int i = Memory.Random.Next(100 + 1);
            try
            {
                if (i < percent && list.Length > 0)
                {
                    byte r = (byte)Memory.Random.Next(256);
                    if (RareITEM)
                    {
                        if (r < 128)
                            return list[0];
                        else if ((r -= 128) < 114)
                            return list[1];
                        else if ((r -= 114) < 14)
                            return list[2];
                        else
                            return list[3];
                    }
                    if (r < 178)
                        return list[0];
                    else if ((r -= 178) < 51)
                        return list[1];
                    else if ((r -= 51) < 15)
                        return list[2];
                    else
                        return list[3];
                }
            }
            finally { mugged = true; }
            mugged = false;
            return default;
        }

        /// <summary>
        /// I notice that the resistance reported on the wiki is 100 less than the number in the data.
        /// </summary>
        /// <param name="s">status effect</param>
        /// <returns>percent of resistance</returns>
        /// <see cref="https://finalfantasy.fandom.com/wiki/G-Soldier#Stats"/>
        public override sbyte StatusResistance(Kernel.PersistentStatuses s)
        {
            byte r = 100;
            switch (s)
            {
                case Kernel.PersistentStatuses.Death:
                    r = Info.deathResistanceMental;
                    break;

                case Kernel.PersistentStatuses.Poison:
                    r = Info.poisonResistanceMental;
                    break;

                case Kernel.PersistentStatuses.Petrify:
                    r = Info.petrifyResistanceMental;
                    break;

                case Kernel.PersistentStatuses.Darkness:
                    r = Info.darknessResistanceMental;
                    break;

                case Kernel.PersistentStatuses.Silence:
                    r = Info.silenceResistanceMental;
                    break;

                case Kernel.PersistentStatuses.Berserk:
                    r = Info.berserkResistanceMental;
                    break;

                case Kernel.PersistentStatuses.Zombie:
                    r = Info.zombieResistanceMental;
                    break;
            }

            return (sbyte)MathHelper.Clamp(r - 100, -100, 100);
        }

        public override sbyte StatusResistance(Kernel.BattleOnlyStatuses s)

        {
            byte r = statusdefault;
            switch (s)
            {
                case Kernel.BattleOnlyStatuses.Sleep:
                    r = Info.sleepResistanceMental;
                    break;

                case Kernel.BattleOnlyStatuses.Haste:
                    r = Info.hasteResistanceMental;
                    break;

                case Kernel.BattleOnlyStatuses.Slow:
                    r = Info.slowResistanceMental;
                    break;

                case Kernel.BattleOnlyStatuses.Stop:
                    r = Info.stopResistanceMental;
                    break;

                case Kernel.BattleOnlyStatuses.Regen:
                    r = Info.regenResistanceMental;
                    break;

                case Kernel.BattleOnlyStatuses.Protect:
                    break;

                case Kernel.BattleOnlyStatuses.Shell:
                    break;

                case Kernel.BattleOnlyStatuses.Reflect:
                    r = Info.reflectResistanceMental;
                    break;

                case Kernel.BattleOnlyStatuses.Aura:
                    break;

                case Kernel.BattleOnlyStatuses.Curse:
                    break;

                case Kernel.BattleOnlyStatuses.Doom:
                    r = Info.doomResistanceMental;
                    break;

                case Kernel.BattleOnlyStatuses.Invincible:
                    break;

                case Kernel.BattleOnlyStatuses.Petrifying:
                    r = Info.slowPetrifyResistanceMental;
                    break;

                case Kernel.BattleOnlyStatuses.Float:
                    r = Info.floatResistanceMental;
                    break;

                case Kernel.BattleOnlyStatuses.Confuse:
                    r = Info.confuseResistanceMental;
                    break;

                case Kernel.BattleOnlyStatuses.Drain:
                    r = Info.drainResistanceMental;
                    break;

                case Kernel.BattleOnlyStatuses.Eject:
                    r = Info.explusionResistanceMental;
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

        public override ushort TotalStat(Kernel.Stat s)
        {
            switch (s)
            {
                case Kernel.Stat.HP:
                    return CurrentHP();

                case Kernel.Stat.EVA:
                    //TODO confirm if there is no flat stat buff for eva. If there isn't then remove from function.
                    return EVA;

                case Kernel.Stat.SPD:
                    return SPD;

                case Kernel.Stat.HIT:
                    return HIT;

                case Kernel.Stat.Luck:
                    return LUCK;

                case Kernel.Stat.MAG:
                    return MAG;

                case Kernel.Stat.SPR:
                    return SPR;

                case Kernel.Stat.STR:
                    return STR;

                case Kernel.Stat.VIT:
                    return VIT;
            }
            return 0;
        }

        protected override void ReadData(BinaryReader br, Enum @enum) => throw new NotImplementedException("This method is not used by Enemy");

        private byte convert1(byte[] @in)
        {
            //from Ifrit's help file
            byte level = Level;
            int i = level * @in[0] / 10 + level / @in[1] - level * level / 2 / (@in[3] + @in[2]) / 4;
            //PLEASE NOTE: I'm not 100% sure on the STR/MAG formula, but it should be accurate enough to get the general idea.
            // wiki states something like ([3(Lv)] + [(Lv) / 5] - [(Lv)² / 260] + 12) / 4

            return (byte)MathHelper.Clamp(i, 0, byte.MaxValue);
        }

        private byte convert2(byte[] @in)
        {
            //from Ifrit's help file
            byte level = Level;
            int i = level / @in[1] - level / @in[3] + level * @in[0] + @in[2];
            return (byte)MathHelper.Clamp(i, 0, byte.MaxValue);
        }

        private int convert3(ushort @in, byte inLevel)
        {
            //from Ifrit's help file
            byte level = Level;
            if (inLevel == 0)
                return 0;
            else
                return @in * (5 * (level - inLevel) / inLevel + 4);
        }

        private T hml<T>(T h, T m, T l)
        {
            byte level = Level;
            if (level > Info.highLevelStart)
                return h;
            else if (level > Info.medLevelStart)
                return m;
            else return l;
        }

        private int levelgroup()
        {
            byte l = Level;
            if (l > Info.highLevelStart)
                return 2;
            if (l > Info.medLevelStart)
                return 1;
            else return 0;
        }

        #endregion Methods
    }
}
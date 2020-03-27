using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OpenVIII
{
    public class Enemy : Damageable, IEnemy
    {
        #region Fields

        private const int statusDefault = 100;
        private bool mugged = false;

        #endregion Fields

        #region Methods

        /// <summary>
        /// Returns STR/MAG value based on the formula and given bytes.
        /// Calculations are based on the Ifrit's help file.
        /// </summary>
        /// <param name="bytes">Value which will be converted to a numeric value.</param>
        private byte GetPrimaryStat(byte[] bytes) {
            // PLEASE NOTE: I'm not 100% sure on the STR/MAG formula, but it should be accurate enough to get the general idea.
            // Wiki states something like ([3(Lv)] + [(Lv) / 5] - [(Lv)² / 260] + 12) / 4
            byte level = Level;
            int i = level * bytes[0] / 10 + level / bytes[1] - level * level / 2 / (bytes[3] + bytes[2]) / 4;
            return (byte)MathHelper.Clamp(i, 0, byte.MaxValue);
        }

        /// <summary>
        /// Returns EVA/SPD/SPR/VIT value based on the formula and given bytes.
        /// </summary>
        /// <param name="bytes">Value which will be converted to a numeric value.</param>
        private byte GetSecondaryStat(byte[] bytes) {
            byte level = Level;
            int i = level / bytes[1] - level / bytes[3] + level * bytes[0] + bytes[2];
            return (byte)MathHelper.Clamp(i, 0, byte.MaxValue);
        }

        /// <summary>
        /// Returns value of EXP which entity will get after the fight.
        /// </summary>
        /// <param name="bytes">Value which will be converted to a numeric value.</param>
        /// <param name="averagePartyLevel">Average level of party whom EXP will be given.</param>
        private int GetEXP(ushort bytes, byte averagePartyLevel) {
            byte averageEnemyLevel = Level;
            if (averagePartyLevel == 0) {
                return 0;
            }
            else {
                return bytes * (5 * (averageEnemyLevel - averagePartyLevel) / averagePartyLevel + 4);
            }
        }

        private T Hml<T>(T high, T mid, T low) {
            byte averageEnemyLevel = Level;
            if (averageEnemyLevel > Info.highLevelStart) {
                return high;
            }
            else if (averageEnemyLevel > Info.medLevelStart) {
                return mid;
            }
            else {
                return low;
            }
        }

        private int LevelGroup() {
            byte averageEnemyLevel = Level;
            if (averageEnemyLevel > Info.highLevelStart) {
                return 2; // what means 2?
            }
            else if (averageEnemyLevel > Info.medLevelStart) {
                return 1; // what means 1?
            }
            else {
                return 0; // what means 0?
            }
        }

        #endregion Methods

        #region Constructors

        protected override void ReadData(BinaryReader br, Enum @enum) => throw new NotImplementedException("This method is not used by Enemy");

        public static Enemy Load(Battle.EnemyInstanceInformation eii, byte level = 0, ushort? startHp = null) {
            Enemy loadedEnemy = new Enemy
            {
                EII = eii,
                FixedLevel = level
            };

            loadedEnemy._CurrentHP = startHp ?? loadedEnemy.MaxHP();

            if ((loadedEnemy.Info.bitSwitch & Debug_battleDat.Information.Flag1.Zombie) != 0) {
                loadedEnemy.Statuses0 |= Kernel_bin.Persistent_Statuses.Zombie;
            }
            if ((loadedEnemy.Info.bitSwitch & Debug_battleDat.Information.Flag1.Auto_Protect) != 0) {
                loadedEnemy.Statuses1 |= Kernel_bin.Battle_Only_Statuses.Protect;
            }
            if ((loadedEnemy.Info.bitSwitch & Debug_battleDat.Information.Flag1.Auto_Reflect) != 0) {
                loadedEnemy.Statuses1 |= Kernel_bin.Battle_Only_Statuses.Reflect;
            }
            if ((loadedEnemy.Info.bitSwitch & Debug_battleDat.Information.Flag1.Auto_Shell) != 0) {
                loadedEnemy.Statuses1 |= Kernel_bin.Battle_Only_Statuses.Shell;
            }
            if ((loadedEnemy.Info.bitSwitch & Debug_battleDat.Information.Flag1.Fly) != 0) {
                loadedEnemy.Statuses1 |= Kernel_bin.Battle_Only_Statuses.Float;
            }
            loadedEnemy.Init();
            return loadedEnemy;
        }

        #endregion Constructors

        #region Properties
        public IEnumerable<Kernel_bin.Enemy_Attacks_Data> Enemy_Attacks_Datas {
            get {
                return Abilities.Where(x => x.MONSTER != null).Select(x => x.MONSTER);
            }
        }

        public IEnumerable<GFs> JunctionedGFs {
            get {
                if (Memory.State != null) {
                    return DrawList.Select(x => x.GF).Where(gf => gf >= GFs.Quezacotl && gf <= GFs.Eden && !Memory.State.UnlockedGFs.Contains(gf)).Distinct();
                }
                else {
                    return null;
                }
            }
        }

        public static List<Enemy> Party { get; set; }

        public byte AP => Info.ap;

        public Debug_battleDat.Magic[] DrawList => Hml(Info.drawhigh, Info.drawmed, Info.drawlow);

        /// <summary>
        /// Randomly gain 1 or 0 from this list.
        /// </summary>
        public Saves.Item[] DropList => Hml(Info.drophigh, Info.dropmed, Info.droplow);
        public Debug_battleDat.Abilities[] Abilities => Hml(Info.abilitiesHigh, Info.abilitiesMed, Info.abilitiesLow);

        public byte DropRate => (byte)(MathHelper.Clamp(Info.dropRate * 100 / byte.MaxValue, 0, 100));

        public Battle.EnemyInstanceInformation EII { get; set; }

        public override byte EVA => GetSecondaryStat(Info.eva);

        /// <summary>
        /// The EXP everyone gets.
        /// </summary>
        public override int EXP => GetEXP(Info.exp, Memory.State.AveragePartyLevel);

        public byte FixedLevel { get; set; }

        /// <summary>
        /// Enemy attacks determine hit%
        /// </summary>
        /// <remarks>
        /// LeythalknightToday at 1:53 PM Enemy attack accuracy is set per ability based on the value
        /// that's called "Attack param" in Doomtrain, and only when they use attack types that have
        /// a miss chance
        /// </remarks>
        public override byte HIT => 0;

        /// <summary>
        /// Level of enemy based on average of party or fixed value.
        /// </summary>
        /// <see cref="https://finalfantasy.fandom.com/wiki/Level#Enemy_levels"/>
        public override byte Level {
            get {
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

        public override byte MAG => GetPrimaryStat(Info.mag);

        /// <summary>
        /// Randomly gain 1 or 0 from this list.
        /// </summary>
        public Saves.Item[] MugList => Hml(Info.mughigh, Info.mugmed, Info.muglow);

        public byte MugRate => (byte)(MathHelper.Clamp(Info.mugRate * 100 / byte.MaxValue, 0, 100));

        public override FF8String Name => Info.name;

        public override byte SPD => GetSecondaryStat(Info.spd);

        public override byte SPR => GetSecondaryStat(Info.spr);

        public override byte STR => GetPrimaryStat(Info.str);

        public override byte VIT => GetSecondaryStat(Info.vit);

        public Kernel_bin.Devour Devour {
            get {
                if (Info.devour[LevelGroup()] >= Kernel_bin.Devour_.Count) {
                    return Kernel_bin.Devour_[Kernel_bin.Devour_.Count - 1];
                }
                else {
                    return Kernel_bin.Devour_[Info.devour[LevelGroup()]];
                }
            }
        }

        public Debug_battleDat.Information Info => EII.Data.information;

        #endregion Properties

        public static implicit operator Enemy(Battle.EnemyInstanceInformation @in) => Load(@in);

        /// <summary>
        /// Return card if succeed at roll
        /// </summary>
        /// <returns></returns>
        /// <see cref="https://gamefaqs.gamespot.com/ps/197343-final-fantasy-viii/faqs/58936"/>
        public Cards.ID Card() {
            if (Info.card.Skip(1).All(x => x == Cards.ID.Immune)) return Cards.ID.Immune;
            int p = (256 * MaxHP() - 255 * CurrentHP()) / MaxHP();
            int r = Memory.Random.Next(256);
            // 2 is rare card, 1 is normal card 0 per ifrit.
            return r < (p + 1) ? (r < 17 ? Info.card[2] : Info.card[1]) : Cards.ID.Fail;
        }

        public Cards.ID CardDrop() {
            //9 / 256
            if (Info.card[0] == Cards.ID.Immune) return Info.card[1];

            int r = Memory.Random.Next(256);
            return r < 9 ? Info.card[1] : Cards.ID.Fail;
        }

        public override Damageable Clone() => throw new NotImplementedException();

        public Saves.Item Drop(bool rareItem = false) {
            if (mugged) {
                return default;
            }

            int percent = DropRate;
            Saves.Item[] list = DropList;
            int i = Memory.Random.Next(100 + 1);
            if (i < percent && list.Length > 0) {
                //Slot              |  0        | 1         | 2        | 3
                //------------------|  ---------| --------- | ---------| --------
                //Without Rare Item | 178 / 256 | 51 / 256  | 15 / 256 | 12 / 256
                //------------------|  ---------| --------- | ---------| --------
                //With Rare Item    | 128 / 256 | 114 / 256 | 14 / 256 | 0 / 256 <- kinda makes no sence to me
                int r = Memory.Random.Next(256);
                if (rareItem) {
                    if (r < 128) {
                        return list[0];
                    }
                    else if ((r -= 128) < 114) {
                        return list[1];
                    }
                    else {
                        return (r -= 114) < 14 ? list[2] : list[3];
                    }
                }
                if (r < 178) {
                    return list[0];
                }
                else if ((r -= 178) < 51) {
                    return list[1];
                }
                else if ((r -= 51) < 15) {
                    return list[2];
                }
                else {
                    return list[3];
                }
            }
            return default;
        }

        public override short ElementalResistance(Kernel_bin.Element @in) {
            List<Kernel_bin.Element> l = (Enum.GetValues(typeof(Kernel_bin.Element))).Cast<Kernel_bin.Element>().ToList();
            if (@in == Kernel_bin.Element.Non_Elemental)
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
        public int EXPExtra(byte lasthitlevel) => GetEXP(Info.expExtra, lasthitlevel);

        public override ushort MaxHP() {
            //from Ifrit's help file
            if (Info.hp == null)
                return 0;
            int i = (Info.hp[0] * Level * Level / 20) + (Info.hp[0] + Info.hp[2] * 100) * Level + Info.hp[1] * 10 + Info.hp[3] * 1000;
            return (ushort)MathHelper.Clamp(i, 0, ushort.MaxValue);
        }

        // Mug is a combination of the Attack and Steal commands, 
        // allowing the user to attempt to steal an item in the same turn they attack. 
        // Like its counterpart, Steal, Mug is associated with the Thief class
        public Saves.Item Mug(byte spd, bool rareItem = false) {
            if (mugged) {
                return default;
            }
            int percent = (MugRate + spd);
            Saves.Item[] list = DropList;
            int i = Memory.Random.Next(100 + 1);
            try {
                if (i < percent && list.Length > 0) {
                    byte r = (byte)Memory.Random.Next(256);
                    if (rareItem) {
                        if (r < 128) {
                            return list[0];
                        }
                        else if ((r -= 128) < 114) {
                            return list[1];
                        }
                        else if ((r -= 114) < 14) {
                            return list[2];
                        }
                        else {
                            return list[3];
                        }
                    }
                    if (r < 178) {
                        return list[0];
                    }
                    else if ((r -= 178) < 51) {
                        return list[1];
                    }
                    else if ((r -= 51) < 15) {
                        return list[2];
                    }
                    else {
                        return list[3];
                    }
                }
            }
            finally {
                mugged = true;
            }
            // Looks like no matter what happends above, mugged is set to false. Buy why?
            mugged = false;
            return default;
        }

        /// <summary>
        /// I notice that the resistance reported on the wiki is 100 less than the number in the data.
        /// </summary>
        /// <param name="s">status effect</param>
        /// <returns>percent of resistance</returns>
        /// <see cref="https://finalfantasy.fandom.com/wiki/G-Soldier#Stats"/>
        public override sbyte StatusResistance(Kernel_bin.Persistent_Statuses s) {
            byte r = 100;
            switch (s) {
                case Kernel_bin.Persistent_Statuses.Death:
                    r = Info.deathResistanceMental;
                    break;

                case Kernel_bin.Persistent_Statuses.Poison:
                    r = Info.poisonResistanceMental;
                    break;

                case Kernel_bin.Persistent_Statuses.Petrify:
                    r = Info.petrifyResistanceMental;
                    break;

                case Kernel_bin.Persistent_Statuses.Darkness:
                    r = Info.darknessResistanceMental;
                    break;

                case Kernel_bin.Persistent_Statuses.Silence:
                    r = Info.silenceResistanceMental;
                    break;

                case Kernel_bin.Persistent_Statuses.Berserk:
                    r = Info.berserkResistanceMental;
                    break;

                case Kernel_bin.Persistent_Statuses.Zombie:
                    r = Info.zombieResistanceMental;
                    break;
            }

            return (sbyte)MathHelper.Clamp(r - 100, -100, 100);
        }

        /// <summary>
        /// I notice that the resistance reported on the wiki is 100 less than the number in the data.
        /// </summary>
        /// <param name="s">status effect</param>
        /// <returns>percent of resistance</returns>
        /// <see cref="https://finalfantasy.fandom.com/wiki/G-Soldier#Stats"/>
        public override sbyte StatusResistance(Kernel_bin.Battle_Only_Statuses s) {
            byte r = statusDefault;
            switch (s) {
                case Kernel_bin.Battle_Only_Statuses.Sleep:
                    r = Info.sleepResistanceMental;
                    break;

                case Kernel_bin.Battle_Only_Statuses.Haste:
                    r = Info.hasteResistanceMental;
                    break;

                case Kernel_bin.Battle_Only_Statuses.Slow:
                    r = Info.slowResistanceMental;
                    break;

                case Kernel_bin.Battle_Only_Statuses.Stop:
                    r = Info.stopResistanceMental;
                    break;

                case Kernel_bin.Battle_Only_Statuses.Regen:
                    r = Info.regenResistanceMental;
                    break;

                case Kernel_bin.Battle_Only_Statuses.Protect:
                    break;

                case Kernel_bin.Battle_Only_Statuses.Shell:
                    break;

                case Kernel_bin.Battle_Only_Statuses.Reflect:
                    r = Info.reflectResistanceMental;
                    break;

                case Kernel_bin.Battle_Only_Statuses.Aura:
                    break;

                case Kernel_bin.Battle_Only_Statuses.Curse:
                    break;

                case Kernel_bin.Battle_Only_Statuses.Doom:
                    r = Info.doomResistanceMental;
                    break;

                case Kernel_bin.Battle_Only_Statuses.Invincible:
                    break;

                case Kernel_bin.Battle_Only_Statuses.Petrifying:
                    r = Info.slowPetrifyResistanceMental;
                    break;

                case Kernel_bin.Battle_Only_Statuses.Float:
                    r = Info.floatResistanceMental;
                    break;

                case Kernel_bin.Battle_Only_Statuses.Confuse:
                    r = Info.confuseResistanceMental;
                    break;

                case Kernel_bin.Battle_Only_Statuses.Drain:
                    r = Info.drainResistanceMental;
                    break;

                case Kernel_bin.Battle_Only_Statuses.Eject:
                    r = Info.explusionResistanceMental;
                    break;
            }
            return (sbyte)MathHelper.Clamp(r - 100, -100, 100);
        }

        /// <summary>
        /// The wiki says some areas have forced or random levels. This lets you override the level.
        /// </summary>
        /// <see cref="https://finalfantasy.fandom.com/wiki/Level#Enemy_levels"/>
        public override string ToString() => Name.Value_str;

        public override ushort TotalStat(Kernel_bin.Stat s) {
            switch (s) {
                case Kernel_bin.Stat.HP:
                    return CurrentHP();

                case Kernel_bin.Stat.EVA:
                    //TODO confirm if there is no flat stat buff for eva. If there isn't then remove from function.
                    return EVA;

                case Kernel_bin.Stat.SPD:
                    return SPD;

                case Kernel_bin.Stat.HIT:
                    return HIT;

                case Kernel_bin.Stat.LUCK:
                    return LUCK;

                case Kernel_bin.Stat.MAG:
                    return MAG;

                case Kernel_bin.Stat.SPR:
                    return SPR;

                case Kernel_bin.Stat.STR:
                    return STR;

                case Kernel_bin.Stat.VIT:
                    return VIT;
            }
            return 0;
        }

        public static implicit operator Battle.EnemyInstanceInformation(Enemy @in) => @in.EII;
    }
}
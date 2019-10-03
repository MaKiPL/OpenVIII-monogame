using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenVIII
{
    public class Enemy : Damageable, IEnemy
    {
        #region Fields

        private const int statusdefault = 100;
        private byte _fixedLevel;

        #endregion Fields

        #region Methods

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
                if(RareITEM)
                {
                    if (r < 128)
                        return list[0];
                    else if ((r-=128) < 114)
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
        bool mugged = false;
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
            return @in * (5 * (level - inLevel) / inLevel + 4);
        }

        private T hml<T>(T h, T m, T l)
        {
            byte level = Level;
            if (level > info.highLevelStart)
                return h;
            else if (level > info.medLevelStart)
                return m;
            else return l;
        }

        private int levelgroup()
        {
            byte l = Level;
            if (l > info.highLevelStart)
                return 2;
            if (l > info.medLevelStart)
                return 1;
            else return 0;
        }

        #endregion Methods

        #region Constructors

        public Enemy(Module_battle_debug.EnemyInstanceInformation eII, byte fixedLevel = 0)
        {
            EII = eII;
            FixedLevel = fixedLevel;
            _CurrentHP = MaxHP();
            if ((info.bitSwitch & Debug_battleDat.Information.Flag1.Zombie) != 0)
            {
                Statuses0 |= Kernel_bin.Persistant_Statuses.Zombie;
            }
            if ((info.bitSwitch & Debug_battleDat.Information.Flag1.Auto_Protect) != 0)
            {
                Statuses1 |= Kernel_bin.Battle_Only_Statuses.Protect;
            }
            if ((info.bitSwitch & Debug_battleDat.Information.Flag1.Auto_Reflect) != 0)
            {
                Statuses1 |= Kernel_bin.Battle_Only_Statuses.Reflect;
            }
            if ((info.bitSwitch & Debug_battleDat.Information.Flag1.Auto_Shell) != 0)
            {
                Statuses1 |= Kernel_bin.Battle_Only_Statuses.Shell;
            }
            if ((info.bitSwitch & Debug_battleDat.Information.Flag1.Fly) != 0)
            {
                Statuses1 |= Kernel_bin.Battle_Only_Statuses.Float;
            }
        }

        #endregion Constructors

        #region Properties

        public static List<Enemy> Party { get; set; }

        public byte AP => info.ap;
        public Debug_battleDat.Magic[] DrawList => hml(info.drawhigh, info.drawmed, info.drawlow);

        /// <summary>
        /// Randomly gain 1 or 0 from this list.
        /// </summary>
        public Saves.Item[] DropList => hml(info.drophigh, info.dropmed, info.droplow);

        public byte DropRate => (byte)(MathHelper.Clamp(info.dropRate * 100 / byte.MaxValue, 0, 100));

        public Module_battle_debug.EnemyInstanceInformation EII { get; set; }

        public override byte EVA => convert2(info.eva);

        /// <summary>
        /// The EXP everyone gets.
        /// </summary>
        public override int EXP => convert3(info.exp, Memory.State.AveragePartyLevel);

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

        public override byte MAG => convert1(info.mag);

        /// <summary>
        /// Randomly gain 1 or 0 from this list.
        /// </summary>
        public Saves.Item[] MugList => hml(info.mughigh, info.mugmed, info.muglow);

        public byte MugRate => (byte)(MathHelper.Clamp(info.mugRate * 100 / byte.MaxValue, 0, 100));

        public override FF8String Name => info.name;

        public override byte SPD => convert2(info.spd);

        public override byte SPR => convert2(info.spr);

        public override byte STR => convert1(info.str);

        public override byte VIT => convert2(info.vit);

        public Kernel_bin.Devour Devour => info.devour[levelgroup()] >= Kernel_bin.Devour_.Count ?
            Kernel_bin.Devour_[Kernel_bin.Devour_.Count - 1] :
            Kernel_bin.Devour_[info.devour[levelgroup()]];

        private Debug_battleDat.Information info => EII.Data.information;

        #endregion Properties

        public static implicit operator Enemy(Module_battle_debug.EnemyInstanceInformation @in) => new Enemy(@in);

        /// <summary>
        /// Return card if succeed at roll
        /// </summary>
        /// <returns></returns>
        /// <see cref="https://gamefaqs.gamespot.com/ps/197343-final-fantasy-viii/faqs/58936"/>
        public Cards.ID Card()
        {
            if (info.card.Skip(1).All(x => x == Cards.ID.Immune)) return Cards.ID.Immune;
            int p = (256 * MaxHP() - 255 * CurrentHP()) / MaxHP();
            int r = Memory.Random.Next(256);
            // 2 is rare card, 1 is normal card 0
            // per ifrit.
            return r < (p + 1) ? (r < 17 ? info.card[2] : info.card[1]) : Cards.ID.Fail;
        }

        public Cards.ID CardDrop()
        {
            //9 / 256
            if (info.card[0] == Cards.ID.Immune) return info.card[1];

            int r = Memory.Random.Next(256);
            return r < 9 ? info.card[1] : Cards.ID.Fail;
        }

        public override short ElementalResistance(Kernel_bin.Element @in)
        {
            List<Kernel_bin.Element> l = (Enum.GetValues(typeof(Kernel_bin.Element))).Cast<Kernel_bin.Element>().ToList();
            if (@in == Kernel_bin.Element.Non_Elemental)
                return 100;
            // I wonder if i should average the resistances in cases of multiple elements.
            else
                return conv(info.resistance[l.FindIndex(x => (x & @in) != 0) - 1]);
            short conv(byte val) => (short)MathHelper.Clamp(900 - (val * 10), -100, 400);
        }

        /// <summary>
        /// The character whom lands the last hit gets alittle bonus xp.
        /// </summary>
        /// <param name="lasthitlevel">Level of character whom got last hit.</param>
        /// <returns></returns>
        public int EXPExtra(byte lasthitlevel) => convert3(info.expExtra, lasthitlevel);

        public override ushort MaxHP()
        {
            //from Ifrit's help file
            int i = (info.hp[0] * Level * Level / 20) + (info.hp[0] + info.hp[2] * 100) * Level + info.hp[1] * 10 + info.hp[3] * 1000;
            return (ushort)MathHelper.Clamp(i, 0, ushort.MaxValue);
        }


        /// <summary>
        /// I notice that the resistance reported on the wiki is 100 less than the number in the data.
        /// </summary>
        /// <param name="s">status effect</param>
        /// <returns>percent of resistance</returns>
        /// <see cref="https://finalfantasy.fandom.com/wiki/G-Soldier#Stats"/>
        public override sbyte StatusResistance(Kernel_bin.Persistant_Statuses s)
        {
            byte r = 100;
            switch (s)
            {
                case Kernel_bin.Persistant_Statuses.Death:
                    r = info.deathResistanceMental;
                    break;

                case Kernel_bin.Persistant_Statuses.Poison:
                    r = info.poisonResistanceMental;
                    break;

                case Kernel_bin.Persistant_Statuses.Petrify:
                    r = info.petrifyResistanceMental;
                    break;

                case Kernel_bin.Persistant_Statuses.Darkness:
                    r = info.darknessResistanceMental;
                    break;

                case Kernel_bin.Persistant_Statuses.Silence:
                    r = info.silenceResistanceMental;
                    break;

                case Kernel_bin.Persistant_Statuses.Berserk:
                    r = info.berserkResistanceMental;
                    break;

                case Kernel_bin.Persistant_Statuses.Zombie:
                    r = info.zombieResistanceMental;
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
        public override sbyte StatusResistance(Kernel_bin.Battle_Only_Statuses s)
        {
            byte r = statusdefault;
            switch (s)
            {
                case Kernel_bin.Battle_Only_Statuses.Sleep:
                    r = info.sleepResistanceMental;
                    break;

                case Kernel_bin.Battle_Only_Statuses.Haste:
                    r = info.hasteResistanceMental;
                    break;

                case Kernel_bin.Battle_Only_Statuses.Slow:
                    r = info.slowResistanceMental;
                    break;

                case Kernel_bin.Battle_Only_Statuses.Stop:
                    r = info.stopResistanceMental;
                    break;

                case Kernel_bin.Battle_Only_Statuses.Regen:
                    r = info.regenResistanceMental;
                    break;

                case Kernel_bin.Battle_Only_Statuses.Protect:
                    break;

                case Kernel_bin.Battle_Only_Statuses.Shell:
                    break;

                case Kernel_bin.Battle_Only_Statuses.Reflect:
                    r = info.reflectResistanceMental;
                    break;

                case Kernel_bin.Battle_Only_Statuses.Aura:
                    break;

                case Kernel_bin.Battle_Only_Statuses.Curse:
                    break;

                case Kernel_bin.Battle_Only_Statuses.Doom:
                    r = info.doomResistanceMental;
                    break;

                case Kernel_bin.Battle_Only_Statuses.Invincible:
                    break;

                case Kernel_bin.Battle_Only_Statuses.Petrifying:
                    r = info.slowPetrifyResistanceMental;
                    break;

                case Kernel_bin.Battle_Only_Statuses.Float:
                    r = info.floatResistanceMental;
                    break;

                case Kernel_bin.Battle_Only_Statuses.Confuse:
                    r = info.confuseResistanceMental;
                    break;

                case Kernel_bin.Battle_Only_Statuses.Drain:
                    r = info.drainResistanceMental;
                    break;

                case Kernel_bin.Battle_Only_Statuses.Eject:
                    r = info.explusionResistanceMental;
                    break;
            }
            return (sbyte)MathHelper.Clamp(r - 100, -100, 100);
        }

        /// <summary>
        /// The wiki says some areas have forced or random levels. This lets you override the level.
        /// </summary>
        /// <see cref="https://finalfantasy.fandom.com/wiki/Level#Enemy_levels"/>
        public override string ToString() => Name.Value_str;

        public override ushort TotalStat(Kernel_bin.Stat s)
        {
            switch (s)
            {
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

        public override Damageable Clone() => throw new NotImplementedException();

        public static implicit operator Module_battle_debug.EnemyInstanceInformation(Enemy @in) => @in.EII;
    }
}
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace OpenVIII
{
    namespace Kernel
    {
        /// <summary>
        /// Character Stats from Kernel
        /// </summary>
        /// <see cref="https://github.com/alexfilth/doomtrain/wiki/Characters"/>
        /// <seealso cref="http://forums.qhimm.com/index.php?topic=16923.msg240609#msg240609"/>
        public sealed class CharacterStats
        {
            #region Fields

            public const int Count = 11;
            public const int ID = 6;
            private const int MaxLevel = 100;
            private const int PercentMod = 100;
            private readonly byte[] _exp;

            private readonly byte[] _hp;

            private readonly byte _limitID;
            private readonly byte[] _luck;
            private readonly byte[] _mag;
            private readonly byte[] _spd;

            private readonly byte[] _spr;

            private readonly byte[] _str;

            private readonly byte[] _vit;

            #endregion Fields

            #region Constructors

            private CharacterStats(BinaryReader br, Characters charID)
            {
                CharID = charID;
                //Offset = br.ReadUInt16(); //0x0000; 2 bytes; Offset to character name
                // ReSharper disable once CommentTypo
                //Squall and Rinoa have name offsets of 0xFFFF because their name is in the save game data rather than kernel.bin.
                br.BaseStream.Seek(2, SeekOrigin.Current);
                Crisis = br.ReadByte(); //0x0002; 1 byte; Crisis level hp multiplier
                Gender = br.ReadByte() == 0 ? Gender.Male : Gender.Female; //0x0003; 1 byte; Gender; 0x00 - Male 0x01 - Female
                _limitID = br.ReadByte(); //0x0004; 1 byte; Limit Break ID
                LimitParam = br.ReadByte(); //0x0005; 1 byte; Limit Break Param used for the power of each renzokuken hit before finisher
                _exp = br.ReadBytes(2); //0x0006; 2 bytes; EXP modifier
                _hp = br.ReadBytes(4); //0x0008; 4 bytes; HP modifiers
                _str = br.ReadBytes(4); //0x000C; 4 bytes; STR modifiers
                _vit = br.ReadBytes(4); //0x0010; 4 bytes; VIT modifiers
                _mag = br.ReadBytes(4); //0x0014; 4 bytes; MAG modifiers
                _spr = br.ReadBytes(4); //0x0018; 4 bytes; SPR modifiers
                _spd = br.ReadBytes(4); //0x001C; 4 bytes; SPD modifiers
                _luck = br.ReadBytes(4); //0x0020; 4 bytes; LUCK modifiers
            }

            #endregion Constructors

            #region Properties

            /// <summary>
            /// Crisis level modifier
            /// </summary>
            /// <see cref="https://finalfantasy.fandom.com/wiki/Crisis_Level#Crisis_Level"/>
            public byte Crisis { get; }

            public Gender Gender { get; }

            public BattleCommand Limit => Memory.Kernel_Bin.BattleCommands[_limitID];

            public byte LimitParam { get; }

            public FF8String Name => Memory.Strings.GetName((Faces.ID)CharID);
            private Characters CharID { get; }

            #endregion Properties

            #region Methods

            public static IReadOnlyDictionary<Characters, CharacterStats> Read(BinaryReader br)
                => Enumerable.Range(0, Count).ToDictionary(i => (Characters)i, i => CreateInstance(br, (Characters)i));

            public byte Eva(int lvl, int magicID = 0, int magicCount = 0, int statBonus = 0, int spd = 0, int percentMod = 0)
            {
                int value = (((Memory.Kernel_Bin.MagicData[magicID].JVal[Stat.EVA] * magicCount) / 100 + spd / 4) *
                             (percentMod + PercentMod)) / 100;
                return (byte)MathHelper.Clamp(value, 0, KernelBin.MaxStatValue);
            }

            /// <summary>
            /// Experience to reach level
            /// </summary>
            /// <param name="lvl">Level</param>
            /// <returns></returns>
            public int Exp(byte lvl) => ((lvl - 1) * (lvl - 1) * _exp[1]) / 256 + (lvl - 1) * _exp[0] * 10;

            public byte Hit(int magicID = 0, int magicCount = 0, int weapon = 0)
            {
                int value = Memory.Kernel_Bin.MagicData[magicID].JVal[Stat.HIT] * magicCount + Memory.Kernel_Bin.WeaponsData[weapon].Hit;
                return (byte)MathHelper.Clamp(value, 0, KernelBin.MaxStatValue);
            }

            /// <summary>
            /// </summary>
            /// <param name="lvl">Level</param>
            /// <param name="magicID">Bonus Value of Junctioned Magic</param>
            /// <param name="magicCount">Total amount of Magic in slot</param>
            /// <param name="statBonus">Bonus integer based HP</param>
            /// <param name="percentMod">50% = 50, 100%=100, etc</param>
            /// <returns></returns>
            public ushort HP(sbyte lvl, int magicID = 0, int magicCount = 0, int statBonus = 0, int percentMod = 0)
            {
                if (Memory.Kernel_Bin == null) return 0;
                int value = (((Memory.Kernel_Bin.MagicData[magicID].JVal[Stat.HP] * magicCount) + statBonus + (lvl * _hp[0]) - ((10 * lvl * lvl) / _hp[1]) + _hp[2]) * (percentMod + PercentMod)) / 100;
                return (ushort)MathHelper.Clamp(value, 0, KernelBin.MaxHPValue);
            }

            public byte Level(uint exp)
            {
                //by default no character has this set.
                Debug.Assert(_exp[1] == 0); // if set we need to update the formula.
                return (byte)MathHelper.Clamp(exp / (_exp[0] * 10) + 1, 0, MaxLevel);
            }

            public byte Luck(int lvl, int magicID = 0, int magicCount = 0, int statBonus = 0, int percentMod = PercentMod)
                => SPD_LUCK(_luck[0], _luck[1], _luck[2], _luck[3], lvl, Memory.Kernel_Bin.MagicData[magicID].JVal[Stat.Luck], magicCount, statBonus, percentMod);

            public byte MAG(int lvl, int magicID = 0, int magicCount = 0, int statBonus = 0, int percentMod = PercentMod)
                => STR_VIT_MAG_SPR(_mag[0], _mag[1], _mag[2], _mag[3], lvl, Memory.Kernel_Bin.MagicData[magicID].JVal[Stat.MAG], magicCount, statBonus, percentMod);

            public byte SPD(int lvl, int magicID = 0, int magicCount = 0, int statBonus = 0, int percentMod = PercentMod)
                => SPD_LUCK(_spd[0], _spd[1], _spd[2], _spd[3], lvl, Memory.Kernel_Bin.MagicData[magicID].JVal[Stat.SPD], magicCount, statBonus, percentMod);

            public byte SPR(int lvl, int magicID = 0, int magicCount = 0, int statBonus = 0, int percentMod = PercentMod)
                => STR_VIT_MAG_SPR(_spr[0], _spr[1], _spr[2], _spr[3], lvl, Memory.Kernel_Bin.MagicData[magicID].JVal[Stat.SPR], magicCount, statBonus, percentMod);

            public byte STR(int lvl, int magicID = 0, int magicCount = 0, int statBonus = 0, int percentMod = PercentMod, int weapon = 0)
                => STR_VIT_MAG_SPR(_str[0], _str[1], _str[2], _str[3], lvl, Memory.Kernel_Bin.MagicData[magicID].JVal[Stat.STR], magicCount, statBonus, percentMod, weapon);

            public override string ToString() => Name;

            public byte VIT(int lvl, int magicID = 0, int magicCount = 0, int statBonus = 0, int percentMod = PercentMod)
                => STR_VIT_MAG_SPR(_vit[0], _vit[1], _vit[2], _vit[3], lvl, Memory.Kernel_Bin.MagicData[magicID].JVal[Stat.VIT], magicCount, statBonus, percentMod);

            private static CharacterStats CreateInstance(BinaryReader br, Characters charID)
                => new CharacterStats(br, charID);

            private static byte SPD_LUCK(int a, int b, int c, int d, int lvl, int magicJVal, int magicCount, int statBonus, int percentMod = 0, int unk = 0)
            {
                int value = ((unk + (magicJVal * magicCount) / 100 + statBonus + lvl / b - lvl / d + lvl * a + c) * (percentMod + PercentMod)) / 100;
                return (byte)MathHelper.Clamp(value, 0, KernelBin.MaxStatValue);
            }

            private static byte STR_VIT_MAG_SPR(int a, int b, int c, int d, int lvl, int magicJVal, int magicCount, int statBonus, int percentMod = 0, int unk = 0)
            {
                int value = ((unk + (magicJVal * magicCount) / 100 + statBonus + ((lvl * a) / 10 + lvl / b - (lvl * lvl) / d / 2 + c) / 4) * (percentMod + PercentMod)) / 100;

                return (byte)MathHelper.Clamp(value, 0, KernelBin.MaxStatValue);
            }

            #endregion Methods
        }
    }
}
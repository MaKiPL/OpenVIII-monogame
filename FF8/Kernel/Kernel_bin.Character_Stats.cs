using System.Collections.Generic;
using System.IO;

namespace FF8
{
    public partial class Kernel_bin
    {
        /// <summary>
        /// Character Stats from Kernel
        /// </summary>
        /// <see cref="https://github.com/alexfilth/doomtrain/wiki/Characters"/>
        /// <seealso cref="http://forums.qhimm.com/index.php?topic=16923.msg240609#msg240609"/>
        public class Character_Stats
        {
            public const int id = 6;
            public const int count = 11;
            public const ushort MAX_HP_VALUE = 9999;
            public const byte MAX_STAT_VALUE = 255;
            private Characters char_id { get; set; }
            public FF8String Name => Memory.Strings.GetName((Faces.ID)char_id);

            public override string ToString() => Name;

            //public ushort Offset; //0x0000; 2 bytes; Offset to character name
            //Squall and Rinoa have name offsets of 0xFFFF because their name is in the save game data rather than kernel.bin.
            /// <summary>
            /// Crisis level modifier
            /// </summary>
            /// <see cref="https://finalfantasy.fandom.com/wiki/Crisis_Level#Crisis_Level"/>
            public byte Crisis { get; private set; } //0x0002; 1 byte; Crisis level hp multiplier

            public Gender Gender { get; private set; } //0x0003; 1 byte; Gender; 0x00 - Male 0x01 - Female
            public byte LimitID { get; private set; } //0x0004; 1 byte; Limit Break ID
            public byte LimitParam { get; private set; } //0x0005; 1 byte; Limit Break Param used for the power of each renzokuken hit before finisher
            private byte[] _EXP { get; set; } //0x0006; 2 bytes; EXP modifier
            private byte[] _HP { get; set; } //0x0008; 4 bytes; HP modifiers
            private byte[] _STR { get; set; } //0x000C; 4 bytes; STR modifiers
            private byte[] _VIT { get; set; } //0x0010; 4 bytes; VIT modifiers
            private byte[] _MAG { get; set; } //0x0014; 4 bytes; MAG modifiers
            private byte[] _SPR { get; set; } //0x0018; 4 bytes; SPR modifiers
            private byte[] _SPD { get; set; } //0x001C; 4 bytes; SPD modifiers
            private byte[] _LUCK { get; set; } //0x0020; 4 bytes; LUCK modifiers

            public void Read(BinaryReader br, Characters char_id)
            {
                this.char_id = char_id;
                //Offset = br.ReadUInt16(); //0x0000; 2 bytes; Offset to character name
                //Squall and Rinoa have name offsets of 0xFFFF because their name is in the save game data rather than kernel.bin.
                br.BaseStream.Seek(2, SeekOrigin.Current);
                Crisis = br.ReadByte(); //0x0002; 1 byte; Crisis level hp multiplier
                Gender = br.ReadByte() == 0 ? Gender.Male : Gender.Female; //0x0003; 1 byte; Gender; 0x00 - Male 0x01 - Female
                LimitID = br.ReadByte(); //0x0004; 1 byte; Limit Break ID
                LimitParam = br.ReadByte(); //0x0005; 1 byte; Limit Break Param used for the power of each renzokuken hit before finisher
                _EXP = br.ReadBytes(2); //0x0006; 2 bytes; EXP modifier
                _HP = br.ReadBytes(4); //0x0008; 4 bytes; HP modifiers
                _STR = br.ReadBytes(4); //0x000C; 4 bytes; STR modifiers
                _VIT = br.ReadBytes(4); //0x0010; 4 bytes; VIT modifiers
                _MAG = br.ReadBytes(4); //0x0014; 4 bytes; MAG modifiers
                _SPR = br.ReadBytes(4); //0x0018; 4 bytes; SPR modifiers
                _SPD = br.ReadBytes(4); //0x001C; 4 bytes; SPD modifiers
                _LUCK = br.ReadBytes(4); //0x0020; 4 bytes; LUCK modifiers
                int hp = HP(8);
            }

            public static Dictionary<Characters, Character_Stats> Read(BinaryReader br)
            {
                Dictionary<Characters, Character_Stats> ret = new Dictionary<Characters, Character_Stats>(count);

                for (int i = 0; i < count; i++)
                {
                    Character_Stats tmp = new Character_Stats();
                    tmp.Read(br, (Characters)i);
                    ret[(Characters)i] = tmp;
                }
                return ret;
            }

            //public static Character_Stats[] Read(BinaryReader br)
            //{
            //    Character_Stats[] ret = new Character_Stats[count];

            //    for (int i = 0; i < count; i++)
            //    {
            //        Character_Stats tmp = new Character_Stats();
            //        tmp.Read(br, (Characters)i);
            //        ret[i] = tmp;
            //    }
            //    return ret;
            //}

            private const int _percent_mod = 100;

            /// <summary>
            /// Experence to reach level
            /// </summary>
            /// <param name="lvl">Level</param>
            /// <returns></returns>
            public int EXP(sbyte lvl) => ((lvl - 1) * (lvl - 1) * _EXP[1]) / 256 + (lvl - 1) * _EXP[0] * 10;

            /// <summary>
            /// </summary>
            /// <param name="lvl">Level</param>
            /// <param name="MagicID">Bonus Value of Junctioned Magic</param>
            /// <param name="magic_count">Total amount of Magic in slot</param>
            /// <param name="stat_bonus">Bonus integer based HP</param>
            /// <param name="percent_mod">50% = 50, 100%=100, etc</param>
            /// <returns></returns>
            public ushort HP(sbyte lvl, int MagicID = 0, int magic_count = 0, int stat_bonus = 0, int percent_mod = 0)
            {
                int value = (((MagicData[MagicID].J_Val[Stat.HP] * magic_count) + stat_bonus + (lvl * _HP[0]) - ((10 * lvl * lvl) / _HP[1]) + _HP[2]) * (percent_mod + _percent_mod)) / 100;
                return (ushort)(value > MAX_HP_VALUE ? MAX_HP_VALUE : value);
            }

            public byte STR(int lvl, int MagicID = 0, int magic_count = 0, int stat_bonus = 0, int percent_mod = _percent_mod, int weapon = 0)
                => STR_VIT_MAG_SPR(_STR[0], _STR[1], _STR[2], _STR[3], lvl, MagicData[MagicID].J_Val[Stat.STR], magic_count, stat_bonus, percent_mod, weapon);

            public byte VIT(int lvl, int MagicID = 0, int magic_count = 0, int stat_bonus = 0, int percent_mod = _percent_mod)
                => STR_VIT_MAG_SPR(_VIT[0], _VIT[1], _VIT[2], _VIT[3], lvl, MagicData[MagicID].J_Val[Stat.VIT], magic_count, stat_bonus, percent_mod);

            public byte MAG(int lvl, int MagicID = 0, int magic_count = 0, int stat_bonus = 0, int percent_mod = _percent_mod)
                => STR_VIT_MAG_SPR(_MAG[0], _MAG[1], _MAG[2], _MAG[3], lvl, MagicData[MagicID].J_Val[Stat.MAG], magic_count, stat_bonus, percent_mod);

            public byte SPR(int lvl, int MagicID = 0, int magic_count = 0, int stat_bonus = 0, int percent_mod = _percent_mod)
                => STR_VIT_MAG_SPR(_SPR[0], _SPR[1], _SPR[2], _SPR[3], lvl, MagicData[MagicID].J_Val[Stat.SPR], magic_count, stat_bonus, percent_mod);

            private byte STR_VIT_MAG_SPR(int a, int b, int c, int d, int lvl, int magic_J_val, int magic_count, int stat_bonus, int percent_mod = 0, int UNK = 0)
            {
                int value = ((UNK + (magic_J_val * magic_count) / 100 + stat_bonus + ((lvl * a) / 10 + lvl / b - (lvl * lvl) / d / 2 + c) / 4) * (percent_mod + _percent_mod)) / 100;

                return (byte)(value > MAX_STAT_VALUE ? MAX_STAT_VALUE : value);
            }

            public byte SPD(int lvl, int MagicID = 0, int magic_count = 0, int stat_bonus = 0, int percent_mod = _percent_mod)
                => SPD_LUCK(_SPD[0], _SPD[1], _SPD[2], _SPD[3], lvl, MagicData[MagicID].J_Val[Stat.SPD], magic_count, stat_bonus, percent_mod);

            public byte LUCK(int lvl, int MagicID = 0, int magic_count = 0, int stat_bonus = 0, int percent_mod = _percent_mod)
                => SPD_LUCK(_LUCK[0], _LUCK[1], _LUCK[2], _LUCK[3], lvl, MagicData[MagicID].J_Val[Stat.LUCK], magic_count, stat_bonus, percent_mod);

            private byte SPD_LUCK(int a, int b, int c, int d, int lvl, int magic_J_val, int magic_count, int stat_bonus, int percent_mod = 0, int UNK = 0)
            {
                int value = ((UNK + (magic_J_val * magic_count) / 100 + stat_bonus + lvl / b - lvl / d + lvl * a + c) * (percent_mod + _percent_mod)) / 100;
                return (byte)(value > MAX_STAT_VALUE ? MAX_STAT_VALUE : value);
            }

            public byte EVA(int lvl, int MagicID = 0, int magic_count = 0, int stat_bonus = 0, int spd = 0, int percent_mod = 0)
            {
                int value = (((MagicData[MagicID].J_Val[Stat.EVA] * magic_count) / 100 + spd / 4) * (percent_mod + _percent_mod)) / 100;
                return (byte)(value > MAX_STAT_VALUE ? MAX_STAT_VALUE : value);
            }

            public byte HIT(int MagicID = 0, int magic_count = 0, int weapon = 0)
            {
                int value = MagicData[MagicID].J_Val[Stat.HIT] * magic_count + WeaponsData[weapon].HIT;
                return (byte)(value > MAX_STAT_VALUE ? MAX_STAT_VALUE : value);
            }
        }
    }
}
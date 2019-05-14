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
        public struct Character_Stats
        {
            //public ushort Offset; //0x0000; 2 bytes; Offset to character name
            //Squall and Rinoa have name offsets of 0xFFFF because their name is in the save game data rather than kernel.bin.

            /// <summary>
            /// Crisis level modifier
            /// </summary>
            /// <see cref="https://finalfantasy.fandom.com/wiki/Crisis_Level#Crisis_Level"/>
            public byte Crisis; //0x0002; 1 byte; Crisis level hp multiplier
            public Gender Gender; //0x0003; 1 byte; Gender; 0x00 - Male 0x01 - Female
            public byte LimitID; //0x0004; 1 byte; Limit Break ID
            public byte LimitParam; //0x0005; 1 byte; Limit Break Param used for the power of each renzokuken hit before finisher
            private byte[] _EXP; //0x0006; 2 bytes; EXP modifier
            private byte[] _HP; //0x0008; 4 bytes; HP
            private byte[] _STR; //0x000C; 4 bytes; STR
            private byte[] _VIT; //0x0010; 4 bytes; VIT
            private byte[] _MAG; //0x0014; 4 bytes; MAG
            private byte[] _SPR; //0x0018; 4 bytes; SPR
            private byte[] _SPD; //0x001C; 4 bytes; SPD
            private byte[] _LUCK; //0x0020; 4 bytes; LUCK

            public void Read(BinaryReader br)
            {
                //Offset = br.ReadUInt16(); //0x0000; 2 bytes; Offset to character name
                //Squall and Rinoa have name offsets of 0xFFFF because their name is in the save game data rather than kernel.bin.
                br.BaseStream.Seek(2, SeekOrigin.Current);
                Crisis = br.ReadByte(); //0x0002; 1 byte; Crisis level hp multiplier
                Gender = br.ReadByte() == 0 ? Gender.Male : Gender.Female; //0x0003; 1 byte; Gender; 0x00 - Male 0x01 - Female
                LimitID = br.ReadByte(); //0x0004; 1 byte; Limit Break ID
                LimitParam = br.ReadByte(); //0x0005; 1 byte; Limit Break Param used for the power of each renzokuken hit before finisher
                _EXP = br.ReadBytes(2); //0x0006; 2 bytes; EXP modifier
                _HP = br.ReadBytes(4); //0x0008; 4 bytes; HP
                _STR = br.ReadBytes(4); //0x000C; 4 bytes; STR
                _VIT = br.ReadBytes(4); //0x0010; 4 bytes; VIT
                _MAG = br.ReadBytes(4); //0x0014; 4 bytes; MAG
                _SPR = br.ReadBytes(4); //0x0018; 4 bytes; SPR
                _SPD = br.ReadBytes(4); //0x001C; 4 bytes; SPD
                _LUCK = br.ReadBytes(4); //0x0020; 4 bytes; LUCK
                int hp = HP(8);
            }

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
            public int HP(sbyte lvl, int MagicID = 0, int magic_count = 0, int stat_bonus = 0, int percent_mod = _percent_mod)
            {
                return (((MagicData[MagicID].HP_J * magic_count) + stat_bonus + (lvl * _HP[0]) - ((10 * lvl * lvl) / _HP[1]) + _HP[2]) * percent_mod) / 100;
            }

            public int STR(int lvl, int MagicID = 0, int magic_count = 0, int stat_bonus = 0, int percent_mod = _percent_mod, int weapon = 0)
                => STR_VIT_MAG_SPR(_STR[0], _STR[1], _STR[2], _STR[3], lvl, MagicData[MagicID].STR_J, magic_count, stat_bonus, percent_mod,weapon);
            public int VIT(int lvl, int MagicID = 0, int magic_count = 0, int stat_bonus = 0, int percent_mod = _percent_mod)
                => STR_VIT_MAG_SPR(_VIT[0], _VIT[1], _VIT[2], _VIT[3], lvl, MagicData[MagicID].VIT_J, magic_count, stat_bonus, percent_mod);
            public int MAG(int lvl, int MagicID = 0, int magic_count = 0, int stat_bonus = 0, int percent_mod = _percent_mod)
                => STR_VIT_MAG_SPR(_MAG[0], _MAG[1], _MAG[2], _MAG[3], lvl, MagicData[MagicID].MAG_J, magic_count, stat_bonus, percent_mod);
            public int SPR(int lvl, int MagicID = 0, int magic_count = 0, int stat_bonus = 0, int percent_mod = _percent_mod)
                => STR_VIT_MAG_SPR(_SPR[0], _SPR[1], _SPR[2], _SPR[3], lvl, MagicData[MagicID].SPR_J, magic_count, stat_bonus, percent_mod);

            private int STR_VIT_MAG_SPR(int a, int b, int c, int d, int lvl, int magic_J_val, int magic_count, int stat_bonus, int percent_mod = _percent_mod, int UNK = 0)
            {
                return ((UNK + (magic_J_val * magic_count) / 100 + stat_bonus + ((lvl * a) / 10 + lvl / b - (lvl * lvl) / d / 2 + c) / 4) * percent_mod) / 100;
            }

            public int SPD(int lvl, int MagicID = 0, int magic_count = 0, int stat_bonus = 0, int percent_mod = _percent_mod)
                => SPD_LUCK(_SPD[0], _SPD[1], _SPD[2], _SPD[3], lvl, MagicData[MagicID].SPD_J, magic_count, stat_bonus, percent_mod);
            public int LUCK(int lvl, int MagicID = 0, int magic_count = 0, int stat_bonus = 0, int percent_mod = _percent_mod)
                => SPD_LUCK(_LUCK[0], _LUCK[1], _LUCK[2], _LUCK[3], lvl, MagicData[MagicID].LUCK_J, magic_count, stat_bonus, percent_mod);

            private int SPD_LUCK(int a, int b, int c, int d, int lvl, int magic_J_val, int magic_count, int stat_bonus, int percent_mod = _percent_mod, int UNK = 0)
            {
                return ((UNK + (magic_J_val * magic_count) / 100 + stat_bonus + lvl / b - lvl / d + lvl * a + c) * percent_mod) / 100;
            }

            public int EVA(int lvl, int MagicID = 0, int magic_count = 0, int stat_bonus = 0, int spd=0, int percent_mod = _percent_mod)
            {
                return (((MagicData[MagicID].EVA_J * magic_count) / 100 + spd / 4) * percent_mod) / 100;
            }

            public int HIT(int MagicID = 0, int magic_count = 0, int weapon = 0)
            {
                return MagicData[MagicID].HIT_J * magic_count + weapon;
            }
        }
    }
}
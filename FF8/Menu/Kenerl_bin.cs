using FF8.Menu;
using System;
using System.Collections.Generic;
using System.IO;

namespace FF8
{
    public class Kernel_bin
    {
        /// <summary>
        /// Character Stats from Kernel
        /// </summary>
        /// <see cref="https://github.com/alexfilth/doomtrain/wiki/Characters"/>
        /// <seealso cref="http://forums.qhimm.com/index.php?topic=16923.msg240609#msg240609"/>
        public struct Character_Stats
        {
            public ushort Offset; //0x0000; 2 bytes; Offset to character name
                                   //Squall and Rinoa have name offsets of 0xFFFF because their name is in the save game data rather than kernel.bin.

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
                Offset = br.ReadUInt16(); //0x0000; 2 bytes; Offset to character name
                                          //Squall and Rinoa have name offsets of 0xFFFF because their name is in the save game data rather than kernel.bin.
                Crisis = br.ReadByte(); //0x0002; 1 byte; Crisis level hp multiplier
                Gender = br.ReadByte()==0?Gender.Male:Gender.Female; //0x0003; 1 byte; Gender; 0x00 - Male 0x01 - Female
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
            public int HP(int lvl, int magic_J_val=0,int magic_count=0, int stat_bonus=0, int percent_mod= _percent_mod)
            {
                int magic = (magic_J_val * magic_count);
                int a = (lvl * _HP[0]);
                int b = ((10 * lvl * lvl) / _HP[1]);
                int c = _HP[2];
                return ((magic + stat_bonus + a - b + c) * percent_mod) / 100;
            }
        }

        private ArchiveWorker aw;
        private string ArchiveString = Memory.Archives.A_MAIN;
        Character_Stats[] CharacterStats;
        /// <summary>
        /// Read binary data from into structures and arrays
        /// </summary>
        /// <see cref="http://forums.qhimm.com/index.php?topic=16923.msg240609#msg240609"/>
        /// <seealso cref="https://github.com/alexfilth/doomtrain"/>
        /// <seealso cref="https://github.com/alexfilth/doomtrain/wiki/Kernel.bin"/>
        public Kernel_bin()
        {
            aw = new ArchiveWorker(ArchiveString);
            byte[] buffer = aw.GetBinaryFile(Memory.Strings.Filenames[(int)Strings.FileID.KERNEL]);
            List<Loc> subPositions = Memory.Strings.Files[Strings.FileID.KERNEL].subPositions;
            int id = 6; //6 is characters
            int count = 11;
            CharacterStats = new Character_Stats[count];
            using (MemoryStream ms = new MemoryStream(buffer))
            using (BinaryReader br = new BinaryReader(ms))
            {
                ms.Seek(subPositions[id], SeekOrigin.Begin);
                for(int i = 0; i<count; i++)
                {
                    CharacterStats[i].Read(br);
                }
            }
        }
    }
}

namespace FF8.Menu
{
    public enum Gender
    {
        Male,
        Female
    }
}
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
        public struct Character_Stats
        {
            public ushort Offset; //0x0000; 2 bytes; Offset to character name
                                   //Squall and Rinoa have name offsets of 0xFFFF because their name is in the save game data rather than kernel.bin.

            public byte Crisis; //0x0002; 1 byte; Crisis level hp multiplier
            public byte Gender; //0x0003; 1 byte; Gender; 0x00 - Male 0x01 - Female
            public byte LimitID; //0x0004; 1 byte; Limit Break ID
            public byte LimitParam; //0x0005; 1 byte; Limit Break Param used for the power of each renzokuken hit before finisher
            public ushort EXP; //0x0006; 2 bytes; EXP modifier
            public uint HP; //0x0008; 4 bytes; HP
            public uint STR; //0x000C; 4 bytes; STR
            public uint VIT; //0x0010; 4 bytes; VIT
            public uint MAG; //0x0014; 4 bytes; MAG
            public uint SPR; //0x0018; 4 bytes; SPR
            public uint SPD; //0x001C; 4 bytes; SPD
            public uint LUCK; //0x0020; 4 bytes; LUCK

            public void Read(BinaryReader br)
            {
                Offset = br.ReadUInt16(); //0x0000; 2 bytes; Offset to character name
                                          //Squall and Rinoa have name offsets of 0xFFFF because their name is in the save game data rather than kernel.bin.
                Crisis = br.ReadByte(); //0x0002; 1 byte; Crisis level hp multiplier
                Gender = br.ReadByte(); //0x0003; 1 byte; Gender; 0x00 - Male 0x01 - Female
                LimitID = br.ReadByte(); //0x0004; 1 byte; Limit Break ID
                LimitParam = br.ReadByte(); //0x0005; 1 byte; Limit Break Param used for the power of each renzokuken hit before finisher
                EXP = br.ReadUInt16(); //0x0006; 2 bytes; EXP modifier
                HP = br.ReadUInt32(); //0x0008; 4 bytes; HP
                STR = br.ReadUInt32(); //0x000C; 4 bytes; STR
                VIT = br.ReadUInt32(); //0x0010; 4 bytes; VIT
                MAG = br.ReadUInt32(); //0x0014; 4 bytes; MAG
                SPR = br.ReadUInt32(); //0x0018; 4 bytes; SPR
                SPD = br.ReadUInt32(); //0x001C; 4 bytes; SPD
                LUCK = br.ReadUInt32(); //0x0020; 4 bytes; LUCK
            }
        }

        private ArchiveWorker aw;
        private string ArchiveString = Memory.Archives.A_MAIN;
        Character_Stats[] CharacterStats;
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
using System;
using System.Collections.Generic;
using System.IO;

namespace FF8
{
    public partial class Kernel_bin
    {

        private ArchiveWorker aw;
        private string ArchiveString = Memory.Archives.A_MAIN;
        public static Character_Stats[] CharacterStats;
        public static Magic_Data[] MagicData;
        public static Junctionable_GFs_Data[] JunctionableGFsData;
        public static Battle_Commands[] BattleCommands;

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
            int id; //6 is characters
            int count;

            using (MemoryStream ms = new MemoryStream(buffer))
            using (BinaryReader br = new BinaryReader(ms))
            {
                id = 0; //Battle commands
                count = 39;
                BattleCommands = new Battle_Commands[count];
                ms.Seek(subPositions[id], SeekOrigin.Begin);
                for (int i = 0; i < count; i++)
                {
                    BattleCommands[i].Read(br);
                }

                id = 1; //Magic data
                count = 57;
                MagicData = new Magic_Data[count];
                ms.Seek(subPositions[id], SeekOrigin.Begin);
                for (int i = 0; i < count; i++)
                {
                    MagicData[i].Read(br);
                }

                id = 2; //Junctionable GFs data
                count = 16;
                MagicData = new Magic_Data[count];
                ms.Seek(subPositions[id], SeekOrigin.Begin);
                for (int i = 0; i < count; i++)
                {
                    JunctionableGFsData[i].Read(br);
                }

                id = 6; //Characters
                count = 11;
                CharacterStats = new Character_Stats[count];
                ms.Seek(subPositions[id], SeekOrigin.Begin);
                for (int i = 0; i < count; i++)
                {
                    CharacterStats[i].Read(br);
                }
            }
        }
    }
}


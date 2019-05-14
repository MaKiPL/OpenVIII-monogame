using System.Collections.Generic;
using System.IO;

namespace FF8
{
    public partial class Kernel_bin
    {

        private ArchiveWorker aw;
        private string ArchiveString = Memory.Archives.A_MAIN;
        private Character_Stats[] CharacterStats;

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
                for (int i = 0; i < count; i++)
                {
                    CharacterStats[i].Read(br);
                }
            }
        }
    }
}
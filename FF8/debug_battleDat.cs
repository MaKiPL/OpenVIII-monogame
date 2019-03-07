using System;
using System.IO;
using System.Linq;

namespace FF8
{
    public class Debug_battleDat
    {
        int id;
        public Debug_battleDat(int monsterId)
        {
            id = monsterId;
            ArchiveWorker aw = new ArchiveWorker(Memory.Archives.A_BATTLE);
            string[] path = aw.GetListOfFiles();
        }

        public int GetId => id;
    }
}

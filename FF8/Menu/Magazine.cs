using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FF8
{
    partial class Magazine : SP2
    {
        /// <summary>
        /// Contains Magazines and parts of tutorials; some tutorial images are in Icons.
        /// </summary>
        /// TODO test this.
        public Magazine()
        {
            TextureFilename = new string[] { "mag{0:00}.tex", "magita.TEX" };
            TextureCount = new int[] { 20, 1 };
            TextureStartOffset = 0;
            IndexFilename = "";
            EntriesPerTexture = -1;
            Init();
        }
        /// <summary>
        /// not used in magzine there is no sp2 file.
        /// </summary>
        /// <param name="aw"></param>
        protected override void InitEntries(ArchiveWorker aw = null) { }
    }
}

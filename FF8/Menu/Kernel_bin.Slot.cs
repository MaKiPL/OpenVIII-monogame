using System.IO;

namespace FF8
{
    internal partial class Kernel_bin
    {
        /// <summary>
        /// Slot Magic Data
        /// </summary>
        /// <see cref="https://github.com/alexfilth/doomtrain/wiki/Selphie-limit-break-sets"/>
        internal struct Slot
        {
            internal byte MagicID { get; private set; }
            internal byte Count { get; private set; }

            internal void Read(BinaryReader br, int i = 0)
            {
                MagicID = br.ReadByte();
                Count = br.ReadByte();
            }
        }
    }
}
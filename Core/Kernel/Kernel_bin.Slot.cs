using System.IO;

namespace OpenVIII
{
    public partial class Kernel_bin
    {
        /// <summary>
        /// Slot Magic Data
        /// </summary>
        /// <see cref="https://github.com/alexfilth/doomtrain/wiki/Selphie-limit-break-sets"/>
        public struct Slot
        {
            public byte MagicID { get; private set; }
            public byte Count { get; private set; }

            public void Read(BinaryReader br, int i = 0)
            {
                MagicID = br.ReadByte();
                Count = br.ReadByte();
            }
        }
    }
}